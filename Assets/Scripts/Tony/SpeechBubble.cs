using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class SpeechBubble : MonoBehaviour
{
    [Tooltip("The independent GameObject that gets shown/hidden (NOT a child of this character).")]
    public GameObject bubbleRoot;

    [Tooltip("The text component that displays the message.")]
    public TMP_Text bubbleText;

    [Tooltip("What the bubble follows. Leave empty to follow this character's own transform.")]
    public Transform followTarget;

    [Tooltip("World-space offset above the target (not affected by character scale).")]
    public Vector3 worldOffset = new Vector3(0f, 1f, 0f);

    [Header("Talk Sound")]
    [Tooltip("Sound played each time this character's bubble appears. Assign a short 'blip' or 'talk' clip for an Animal Crossing-style effect.")]
    public AudioClip talkSound;

    [Tooltip("AudioSource used to play the talk sound. Auto-assigned from this GameObject if left empty.")]
    public AudioSource audioSource;

    [Tooltip("Volume of the talk sound.")]
    [Range(0f, 1f)]
    public float talkVolume = 0.8f;

    [Tooltip("Randomizes pitch slightly each time so it doesn't sound robotic/repetitive.")]
    public bool randomizePitch = true;

    [Tooltip("Minimum pitch when randomizing.")]
    public float minPitch = 0.92f;

    [Tooltip("Maximum pitch when randomizing.")]
    public float maxPitch = 1.08f;

    void Awake()
    {
        // Moved from Start() to Awake() so this is guaranteed to be set
        // before ANY other script's Start() runs and potentially calls Show().
        if (followTarget == null)
            followTarget = transform;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Make sure it never plays automatically on its own.
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
        else
            Debug.LogWarning($"SpeechBubble on '{name}': Bubble Root not assigned.");

        if (bubbleText == null)
            Debug.LogWarning($"SpeechBubble on '{name}': Bubble Text not assigned.");
    }

    void LateUpdate()
    {
        if (bubbleRoot != null && bubbleRoot.activeSelf)
            bubbleRoot.transform.position = followTarget.position + worldOffset;
    }

    public void Show(string message)
    {
        if (bubbleRoot == null || bubbleText == null)
        {
            Debug.LogWarning($"SpeechBubble on '{name}': cannot show, missing Bubble Root or Bubble Text.");
            return;
        }

        bubbleText.text = message;
        bubbleRoot.transform.position = followTarget.position + worldOffset;
        bubbleRoot.SetActive(true);

        PlayTalkSound();
    }

    public void Hide()
    {
        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
    }

    private void PlayTalkSound()
    {
        if (talkSound == null || audioSource == null) return;

        audioSource.pitch = randomizePitch ? Random.Range(minPitch, maxPitch) : 1f;
        audioSource.PlayOneShot(talkSound, talkVolume);
    }
}