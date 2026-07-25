using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// A simple world-space speech bubble. Call Show("text") to pop it up
/// above the object for a few seconds, then it auto-hides.
///
/// SETUP:
/// 1. Create a child GameObject under the target (Player or win object),
///    positioned above its head/sprite. Name it e.g. "SpeechBubbleRoot".
/// 2. Under THAT, add a TextMeshPro - Text (world space) child.
/// 3. On this script (attached to the Player/win object itself):
///    - Bubble Root = the "SpeechBubbleRoot" child
///    - Bubble Text = the TMP_Text child
/// 4. Leave both active in the Hierarchy — this script disables the root
///    on Start.
/// </summary>
public class SpeechBubble : MonoBehaviour
{
    public GameObject bubbleRoot;
    public TMP_Text bubbleText;
    public float displayDuration = 2.5f;

    private Coroutine activeRoutine;

    void Start()
    {
        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
        else
            Debug.LogWarning($"SpeechBubble on '{name}': Bubble Root not assigned.");

        if (bubbleText == null)
            Debug.LogWarning($"SpeechBubble on '{name}': Bubble Text not assigned.");
    }

    public void Show(string message)
    {
        Show(message, displayDuration);
    }

    public void Show(string message, float duration)
    {
        if (bubbleRoot == null || bubbleText == null)
        {
            Debug.LogWarning($"SpeechBubble on '{name}': cannot show, missing Bubble Root or Bubble Text.");
            return;
        }

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(ShowRoutine(message, duration));
    }

    public void Hide()
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
    }

    private IEnumerator ShowRoutine(string message, float duration)
    {
        bubbleText.text = message;
        bubbleRoot.SetActive(true);

        yield return new WaitForSeconds(duration);

        bubbleRoot.SetActive(false);
        activeRoutine = null;
    }
}