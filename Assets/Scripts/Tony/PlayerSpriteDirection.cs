using UnityEngine;

/// <summary>
/// Swaps the player's SpriteRenderer between 4 directional sprites
/// (Up/Down/Left/Right) based on current movement input from PlayerMovement.
/// Keeps facing the last moved direction while standing still.
///
/// Setup:
/// 1. Add this script to the same GameObject as PlayerMovement (and the
///    SpriteRenderer that shows the player).
/// 2. Drag your 4 sprites into the matching fields below.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSpriteDirection : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Leave empty to auto-find on this GameObject.")]
    public PlayerMovement playerMovement;

    [Tooltip("Leave empty to auto-find on this GameObject.")]
    public SpriteRenderer spriteRenderer;

    [Header("Directional Sprites")]
    public Sprite frontSprite; // facing down / toward camera
    public Sprite backSprite;  // facing up / away from camera
    public Sprite leftSprite;
    public Sprite rightSprite;

    private enum FacingDirection { Down, Up, Left, Right }
    private FacingDirection currentFacing = FacingDirection.Down;

    void Awake()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (playerMovement == null || spriteRenderer == null)
            return;

        Vector2 input = playerMovement.CurrentMoveInput;

        // Only update facing while actually moving — standing still keeps
        // whatever direction was last used, matching typical top-down games.
        if (input.sqrMagnitude > 0.01f)
        {
            // Pick whichever axis has the larger magnitude as the dominant
            // facing direction (so diagonal movement still picks one clear sprite).
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                currentFacing = input.x > 0 ? FacingDirection.Right : FacingDirection.Left;
            }
            else
            {
                currentFacing = input.y > 0 ? FacingDirection.Up : FacingDirection.Down;
            }
        }

        ApplyFacingSprite();
    }

    void ApplyFacingSprite()
    {
        switch (currentFacing)
        {
            case FacingDirection.Down:
                spriteRenderer.sprite = frontSprite;
                break;
            case FacingDirection.Up:
                spriteRenderer.sprite = backSprite;
                break;
            case FacingDirection.Left:
                spriteRenderer.sprite = leftSprite;
                break;
            case FacingDirection.Right:
                spriteRenderer.sprite = rightSprite;
                break;
        }
    }
}
