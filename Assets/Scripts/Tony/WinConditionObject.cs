using UnityEngine;

/// <summary>
/// Swaps this object's sprite to a different one when interacted with, AND
/// reports the interaction to WinManager so it counts toward the win
/// condition.
///
/// SETUP:
/// 1. Add this script to the object (same GameObject as its SpriteRenderer
///    and InteractableObstacle component).
/// 2. Assign "New Sprite" — the sprite it should change into.
/// 3. On that object's InteractableObstacle component, UNCHECK
///    "Destroy On Interact" (the object should stay and show the new
///    sprite), and UNCHECK "Can Interact Repeatedly" (it should only count
///    once).
/// 4. In InteractableObstacle's "On Interact" UnityEvent list, click "+",
///    drag this GameObject in, and pick WinConditionObject > OnWinInteract
///    from the function dropdown (NOT SwapSprite — OnWinInteract does both).
/// 5. Make sure a "GameManager" GameObject with WinManager.cs exists in
///    the scene. Repeat steps 1–4 for all 4 win objects.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class WinConditionObject : MonoBehaviour
{
    [Tooltip("Leave empty to auto-find the SpriteRenderer on this GameObject.")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("The sprite this object changes into once the player interacts with it.")]
    public Sprite newSprite;

    private bool hasReported;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>Hook this up to InteractableObstacle's On Interact event.</summary>
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