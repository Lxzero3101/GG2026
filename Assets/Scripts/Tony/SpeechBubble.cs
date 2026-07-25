using UnityEngine;
using TMPro;

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

    void Awake()
    {
        // Moved from Start() to Awake() so this is guaranteed to be set
        // before ANY other script's Start() runs and potentially calls Show().
        if (followTarget == null)
            followTarget = transform;
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
    }

    public void Hide()
    {
        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
    }
}