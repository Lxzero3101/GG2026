using UnityEngine;

/// <summary>
/// Snappy, Brotato-style top-down movement: full speed instantly in any of 8
/// directions, no acceleration/inertia. Works with WASD or Arrow Keys.
///
/// Uses explicit GetKey checks instead of Input.GetAxis, so it doesn't depend
/// on the "Horizontal"/"Vertical" virtual axes being configured correctly in
/// Project Settings > Input Manager.
///
/// IMPORTANT: This requires the legacy Input Manager to be active.
/// Go to Edit > Project Settings > Player > Other Settings > Active Input Handling
/// and make sure it's set to "Input Manager (Old)" or "Both".
/// If it's set to "Input System Package (New)" only, this script's Input.GetKey
/// calls will throw an exception — let me know and I'll give you the New Input
/// System version instead.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Constant movement speed — no ramp-up, matches Brotato's snappy feel.")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        // Continuous collision detection prevents tunneling through thin walls at speed.
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v -= 1f;

        moveInput = new Vector2(h, v);

        // Normalize so diagonal movement isn't faster than straight movement.
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();
    }

    void FixedUpdate()
    {
        // Directly setting velocity (rather than MovePosition) gives that instant,
        // no-inertia stop/start feel Brotato has, while still respecting collisions.
        rb.linearVelocity = moveInput * moveSpeed;
    }
}
