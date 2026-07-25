using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WinConditionObject : MonoBehaviour
{
    [Tooltip("Leave empty to auto-find the SpriteRenderer on this GameObject.")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("The sprite this object changes into once the player interacts with it.")]
    public Sprite newSprite;

    [Tooltip("Optional speech bubble on this object, used when the win condition is met.")]
    public SpeechBubble speechBubble;

    private bool hasReported;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (speechBubble == null)
            speechBubble = GetComponent<SpeechBubble>();
    }

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