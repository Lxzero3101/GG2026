using UnityEngine;

/// <summary>
/// Handles player movement (WASD or Arrow Keys) and interaction input (Space).
/// Attach to the Player GameObject along with a Rigidbody2D (set Body Type = Kinematic or Dynamic,
/// Gravity Scale = 0) and a Collider2D.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in units/second.")]
    public float moveSpeed = 4f;

    [Header("Interaction")]
    [Tooltip("Key used to interact with nearby objects.")]
    public KeyCode interactKey = KeyCode.Space;

    [Tooltip("How far the player can reach to interact with an object.")]
    public float interactRadius = 1.2f;

    [Header("Animation (optional)")]
    [Tooltip("Optional Animator with 'MoveX', 'MoveY', 'Speed' float/bool parameters.")]
    public Animator animator;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        // GetAxisRaw gives instant -1/0/1 values (no smoothing) which feels snappier
        // for tile-based movement. Works with WASD and Arrow Keys automatically
        // (both are bound to "Horizontal"/"Vertical" in Unity's default Input Manager).
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(h, v);
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        if (moveInput.sqrMagnitude > 0.01f)
            lastMoveDirection = moveInput;

        UpdateAnimator();

        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    void FixedUpdate()
    {
        // MovePosition respects other physics colliders (walls, obstacles) automatically.
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetFloat("MoveX", moveInput.x);
        animator.SetFloat("MoveY", moveInput.y);
        animator.SetFloat("Speed", moveInput.sqrMagnitude);

        // Useful if you want idle animations to face the last direction moved
        animator.SetFloat("LastX", lastMoveDirection.x);
        animator.SetFloat("LastY", lastMoveDirection.y);
    }

    void TryInteract()
    {
        if (InteractableManager.Instance == null) return;

        InteractableObject nearest =
            InteractableManager.Instance.GetNearestInRange(transform.position, interactRadius);

        if (nearest != null)
        {
            nearest.Interact();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
