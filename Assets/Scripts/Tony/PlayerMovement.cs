using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Snappy, Brotato-style top-down movement: full speed instantly in any of 8
/// directions, no acceleration/inertia. Works with WASD or Arrow Keys.
///
/// This version supports BOTH input backends automatically, using Unity's
/// built-in compiler flags (ENABLE_INPUT_SYSTEM / ENABLE_LEGACY_INPUT_MANAGER),
/// which are set based on your Project Settings > Player > Active Input
/// Handling. You don't need to change anything or maintain two scripts —
/// whichever backend your project is using, this compiles the matching code
/// path automatically. If "Both" is selected, it uses the new Input System
/// path (which reads the keyboard the same way either way).
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
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        float h = 0f;
        float v = 0f;

#if ENABLE_INPUT_SYSTEM
        // New Input System package
        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1f;
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        // Old Input Manager
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v -= 1f;
#endif

        moveInput = new Vector2(h, v);

        // Normalize so diagonal movement isn't faster than straight movement.
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();
    }

    void FixedUpdate()
    {
        // Directly setting velocity gives the instant, no-inertia stop/start
        // feel Brotato has, while still respecting physics collisions.
        rb.linearVelocity = moveInput * moveSpeed;
    }
}
