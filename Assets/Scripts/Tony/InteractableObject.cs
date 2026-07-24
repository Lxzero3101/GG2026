using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attach to any prefab that the player should be able to interact with
/// (box, cupboard, trash can, vase, etc). Shows a prompt ("Interact") when the
/// player enters range, and invokes onInteract when the player presses the
/// interact key while this object is the nearest one in range.
///
/// Suggested hierarchy for the prefab:
///   Box (this script)
///     Sprite (SpriteRenderer)
///     InteractPrompt (a small Canvas/SpriteRenderer/TextMeshPro child, e.g. a
///       speech-bubble icon or "Interact" text, positioned above the object,
///       disabled by default in the prefab)
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Leave empty to auto-find the object tagged 'Player' at runtime.")]
    public Transform player;

    [Tooltip("Child object shown when the player is in range (e.g. an 'Interact' label or icon).")]
    public GameObject interactPrompt;

    [Header("Settings")]
    [Tooltip("How close the player must be for the prompt to appear.")]
    public float detectionRadius = 1.5f;

    [Tooltip("Friendly name used for logging/debug only.")]
    public string objectLabel = "Object";

    [Tooltip("If false, this object stops reacting after being used (e.g. a one-time pickup).")]
    public bool canInteractRepeatedly = true;

    [Header("Events")]
    [Tooltip("Hook up per-object behavior here in the Inspector (open a box, toggle a door, etc), " +
             "or leave empty and override via a subclass.")]
    public UnityEvent onInteract;

    private bool playerInRange;
    private bool hasBeenUsed;

    protected virtual void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        if (InteractableManager.Instance != null)
            InteractableManager.Instance.Register(this);
    }

    protected virtual void OnDestroy()
    {
        if (InteractableManager.Instance != null)
            InteractableManager.Instance.Unregister(this);
    }

    protected virtual void Update()
    {
        if (player == null || (hasBeenUsed && !canInteractRepeatedly))
            return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool inRangeNow = dist <= detectionRadius;

        if (inRangeNow != playerInRange)
        {
            playerInRange = inRangeNow;
            if (interactPrompt != null)
                interactPrompt.SetActive(playerInRange);
        }
    }

    /// <summary>Called by PlayerController when the player presses the interact key nearby.</summary>
    public virtual void Interact()
    {
        if (hasBeenUsed && !canInteractRepeatedly)
            return;

        hasBeenUsed = true;
        Debug.Log($"Interacted with {objectLabel}");
        onInteract?.Invoke();

        if (!canInteractRepeatedly && interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
