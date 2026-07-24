using UnityEngine;

/// <summary>
/// Swaps this object's sprite to a different one when interacted with —
/// used for the "win" object, which should visually change (instead of
/// disappearing like most objects) to show the player what happened.
///
/// SETUP:
/// 1. Add this script to the object (same GameObject as its SpriteRenderer
///    and InteractableObstacle component).
/// 2. Assign "New Sprite" — the sprite it should change into.
/// 3. On that object's InteractableObstacle component, UNCHECK
///    "Destroy On Interact" (the object should stay and show the new
///    sprite, not disappear).
/// 4. In InteractableObstacle's "On Interact" UnityEvent list, click "+",
///    drag this GameObject in, and pick WinConditionObject > SwapSprite
///    from the function dropdown.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class WinConditionObject : MonoBehaviour
{
    [Tooltip("Leave empty to auto-find the SpriteRenderer on this GameObject.")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("The sprite this object changes into once the player interacts with it.")]
    public Sprite newSprite;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>Call this from the object's interact event to swap the sprite.</summary>
    public void SwapSprite()
    {
        if (spriteRenderer == null || newSprite == null) return;

        spriteRenderer.sprite = newSprite;
    }
}
