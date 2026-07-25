using UnityEngine;
using TMPro;

/// <summary>
/// A world-space speech bubble that shows above a character. Call Show()
/// to display a line of text — it stays up until Hide() is called (no
/// auto-timer), which suits click-to-advance dialogue.
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

    void Start()
    {
        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
        else
            Debug.LogWarning($"SpeechBubble on '{name}': Bubble Root not assigned.");

        if (bubbleText == null)
            Debug.LogWarning($"SpeechBubble on '{name}': Bubble Text not assigned.");
    }

    /// <summary>Shows a line of text. Stays visible until Hide() is called.</summary>
    public void Show(string message)
    {
        if (bubbleRoot == null || bubbleText == null)
        {
            Debug.LogWarning($"SpeechBubble on '{name}': cannot show, missing Bubble Root or Bubble Text.");
            return;
        }

        bubbleText.text = message;
        bubbleRoot.SetActive(true);
    }

    public void Hide()
    {
        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
    }
}