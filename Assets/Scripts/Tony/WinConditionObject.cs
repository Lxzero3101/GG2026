using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WinConditionObject : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite newSprite;

    [Tooltip("Speech bubble on THIS object, shown after the player's line.")]
    public SpeechBubble speechBubble;

    [TextArea]
    public string winLine = "okay";

    private bool hasReported;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (speechBubble == null)
            speechBubble = GetComponent<SpeechBubble>();
    }

    /// <summary>Hook this to InteractableObstacle's On Interact event.</summary>
    public void OnWinInteract()
    {
        SwapSprite();
        ReportToWinManager();
    }

    public void SwapSprite()
    {
        if (spriteRenderer == null || newSprite == null) return;
        spriteRenderer.sprite = newSprite;
    }

    /// <summary>Called by WinManager once all 4 are collected, after the player's line.</summary>
    public void SayWinLine()
    {
        if (speechBubble != null)
            speechBubble.Show(winLine);
    }

    private void ReportToWinManager()
    {
        if (hasReported) return;
        hasReported = true;

        if (WinManager.Instance != null)
            WinManager.Instance.RegisterWin(this);
        else
            Debug.LogWarning("No WinManager found in scene — win won't be tracked.");
    }
}