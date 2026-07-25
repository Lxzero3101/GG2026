using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// A world-space speech bubble that shows above a character. Call Show()
/// to display a line of text for a set duration, then it auto-hides.
///
/// SETUP (per character - Player, NPC, etc):
/// 1. Create a child GameObject under the character, positioned above its
///    head/sprite. Name it "SpeechBubbleRoot".
/// 2. Under THAT, add a TextMeshPro - Text (World Space) child for the
///    message text.
/// 3. Add this script to the character's root GameObject:
///    - Bubble Root = the "SpeechBubbleRoot" child
///    - Bubble Text = the TMP_Text child
/// 4. Leave both active in the Hierarchy — this script disables the root
///    automatically on Start.
/// </summary>
public class SpeechBubble : MonoBehaviour
{
    [Tooltip("The GameObject that gets shown/hidden (bubble background + text container).")]
    public GameObject bubbleRoot;

    [Tooltip("The text component that displays the message.")]
    public TMP_Text bubbleText;

    [Tooltip("Default seconds a line stays visible if no override duration is given.")]
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

    /// <summary>Shows a line using this bubble's default duration.</summary>
    public void Show(string message)
    {
        Show(message, displayDuration);
    }

    /// <summary>Shows a line for a specific duration (seconds).</summary>
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