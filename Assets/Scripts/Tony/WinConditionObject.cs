using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WinConditionObject : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite newSprite;

    private bool hasReported;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
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