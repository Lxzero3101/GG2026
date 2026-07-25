using System.Collections;
using UnityEngine;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// An interactable object that ALSO physically blocks the player (like a wall
/// or a chest you can't walk through). Requires TWO Collider2D components on
/// this GameObject:
///
///   1. Solid Collider  — a normal (non-trigger) Collider2D matching the
///      object's visual size. This is what stops the player from walking
///      through it (works the same way as WallTilemapCollider: a Dynamic
///      Rigidbody2D on the player is stopped by any non-trigger collider).
///
///   2. Trigger Collider — a second, LARGER Collider2D with "Is Trigger"
///      checked. This defines the detection range: when the player enters
///      it, the "Press F to interact" prompt appears.
///
/// SETUP:
/// 1. On the object, add a BoxCollider2D (or CircleCollider2D) sized to match
///    the sprite — leave "Is Trigger" OFF. This is your solid/blocking collider.
/// 2. Add a SECOND Collider2D of the same or different type, made noticeably
///    bigger than the first, with "Is Trigger" checked ON. This is the
///    detection zone.
/// 3. Create a child GameObject for the prompt (e.g. a world-space
///    TextMeshPro that says "Press F" or a small icon+text bubble),
///    positioned above the object, and assign it to "Prompt Root" below.
///    Leave it active in the Hierarchy — this script disables it on Start.
/// 4. Add this script to the object itself.
/// 5. Make sure your Player GameObject is tagged "Player".
/// </summary>
public class InteractableObstacle : MonoBehaviour
{
    [Header("Interaction")]
    [Tooltip("Object shown/hidden as the interact prompt (e.g. 'Press F' text or icon).")]
    public GameObject promptRoot;

    [Tooltip("Friendly name for debug logging.")]
    public string objectLabel = "Object";

    [Tooltip("If false, this object can only be interacted with once.")]
    public bool canInteractRepeatedly = true;

    [Tooltip("If true, the object is destroyed after being interacted with.")]
    public bool destroyOnInteract = true;

    [Header("Events")]
    [Tooltip("Hook up per-object behavior here (open a chest, play a sound, give an item, etc).")]
    public UnityEvent onInteract;

    [Header("Prompt Animation")]
    [Tooltip("How long the pop-in bounce takes.")]
    public float popInDuration = 0.18f;

    [Tooltip("How far the prompt overshoots before settling (1 = no overshoot, 1.2 = 20% overshoot).")]
    public float overshootScale = 1.2f;

    [Tooltip("Gentle up/down bob while the prompt is visible, like Genshin's chest icon.")]
    public bool bobWhileVisible = true;
    public float bobHeight = 0.05f;
    public float bobSpeed = 2f;

    private bool playerInRange;
    private bool hasBeenUsed;
    private Coroutine animRoutine;
    private Vector3 promptBasePosition;

    void Start()
    {
        if (promptRoot != null)
        {
            promptBasePosition = promptRoot.transform.localPosition;
            promptRoot.SetActive(false);
        }
    }

    void Update()
    {
        if (!playerInRange || (hasBeenUsed && !canInteractRepeatedly))
            return;

        if (InteractKeyPressed())
        {
            Interact();
        }
    }

    bool InteractKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        return kb != null && kb.fKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.F);
#else
        return false;
#endif
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasBeenUsed && !canInteractRepeatedly) return;

        playerInRange = true;
        ShowPrompt();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        HidePrompt();
    }

    public void Interact()
    {
        if (hasBeenUsed && !canInteractRepeatedly)
            return;

        hasBeenUsed = true;
        Debug.Log($"Interacted with {objectLabel}");
        onInteract?.Invoke();

        if (destroyOnInteract)
        {
            RemoveFromGameplay();
            return;
        }

        if (!canInteractRepeatedly)
            HidePrompt();
    }

    /// <summary>
    /// Instantly takes the object out of play — disables its colliders and
    /// visuals right away (so the player isn't briefly blocked by a "dead"
    /// object) and stops any in-progress prompt animation before destroying it.
    /// </summary>
    void RemoveFromGameplay()
    {
        playerInRange = false;
        StopAllCoroutines();

        // If there's an AudioSource currently playing, detach it into a
        // temporary object so it can finish playing even after this
        // object is destroyed.
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.isPlaying)
        {
            GameObject tempAudio = new GameObject("TempAudio_" + name);
            tempAudio.transform.position = transform.position;

            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
            tempSource.clip = audioSource.clip;
            tempSource.volume = audioSource.volume;
            tempSource.pitch = audioSource.pitch;
            tempSource.spatialBlend = audioSource.spatialBlend;
            tempSource.Play();

            Destroy(tempAudio, audioSource.clip.length);
        }

        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;

        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = false;

        if (promptRoot != null)
            promptRoot.SetActive(false);

        Destroy(gameObject);
    }

    void ShowPrompt()
    {
        if (promptRoot == null || !gameObject.activeInHierarchy) return;

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(PopIn());
    }

    void HidePrompt()
    {
        if (promptRoot == null || !gameObject.activeInHierarchy) return;

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(PopOut());
    }

    IEnumerator PopIn()
    {
        promptRoot.SetActive(true);
        float t = 0f;

        // Scale 0 -> overshoot -> settle at 1, for that bouncy Genshin-style pop.
        while (t < popInDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / popInDuration);

            float scale = progress < 0.7f
                ? Mathf.Lerp(0f, overshootScale, progress / 0.7f)
                : Mathf.Lerp(overshootScale, 1f, (progress - 0.7f) / 0.3f);

            promptRoot.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        promptRoot.transform.localScale = Vector3.one;

        if (bobWhileVisible)
            animRoutine = StartCoroutine(Bob());
    }

    IEnumerator Bob()
    {
        while (true)
        {
            float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            promptRoot.transform.localPosition = promptBasePosition + new Vector3(0f, offset, 0f);
            yield return null;
        }
    }

    IEnumerator PopOut()
    {
        float t = 0f;
        const float popOutDuration = 0.1f;
        Vector3 startScale = promptRoot.transform.localScale;

        while (t < popOutDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / popOutDuration);
            promptRoot.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            yield return null;
        }

        promptRoot.transform.localPosition = promptBasePosition;
        promptRoot.SetActive(false);
    }
}
