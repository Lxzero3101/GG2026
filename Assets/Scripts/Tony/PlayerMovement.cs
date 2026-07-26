using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Snappy, Brotato-style top-down movement: full speed instantly in any of 8
/// directions, no acceleration/inertia. Works with WASD or Arrow Keys.
///
/// This version supports BOTH input backends automatically, using Unity's
/// built-in compiler flags (ENABLE_INPUT_SYSTEM / ENABLE_LEGACY_INPUT_MANAGER).
///
/// Also owns the "input lock" used to freeze the player during the intro
/// countdown (and reused for the win screen — see <see cref="SetLocked"/>).
/// Uses <see cref="CountdownUI.Instance"/> rather than a serialized field:
/// this script lives on the Player PREFAB, which RandomSpawner instantiates
/// at runtime, so it can never hold a working drag-and-drop reference to the
/// scene's CountdownUI object. Exposes a static Instance (same pattern as
/// GameUI.Instance) so prefab-asset scripts like InteractableItem, or other
/// systems like WinScreenUI, can reach the player without a scene reference.
///
/// Also plays a looping footstep sound while the player is actively moving,
/// and stops it the instant movement stops (including while locked).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }

    [Header("Movement")]
    [Tooltip("Constant movement speed — no ramp-up, matches Brotato's snappy feel.")]
    public float moveSpeed = 5f;

    [Header("Countdown Lock")]
    [Tooltip("If true, movement/interaction stays frozen until CountdownUI.Instance finishes counting down.")]
    [SerializeField] private bool freezeDuringCountdown = true;

    [Header("Footstep Sound")]
    [Tooltip("AudioSource that plays the walking loop. Leave empty to auto-find one on this GameObject.")]
    public AudioSource footstepAudioSource;

    [Tooltip("The walking sound clip. Assign here, or leave empty and set the clip directly on the AudioSource instead.")]
    public AudioClip walkClip;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    /// <summary>Current movement input this frame (-1..1 on each axis). Read-only for other scripts.</summary>
    public Vector2 CurrentMoveInput => moveInput;

    /// <summary>True while movement and interactable clicks should be frozen (e.g. during the intro countdown or a win screen).</summary>
    public bool IsLocked { get; private set; } = true;

    void Awake()
    {
        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (footstepAudioSource == null)
            footstepAudioSource = GetComponent<AudioSource>();

        if (footstepAudioSource != null)
        {
            footstepAudioSource.loop = true;
            footstepAudioSource.playOnAwake = false;

            if (walkClip != null)
                footstepAudioSource.clip = walkClip;
        }

        // NOTE: We intentionally do NOT decide the lock state here based on
        // CountdownUI.Instance. This script lives on the Player PREFAB, which is
        // instantiated at runtime, so its Awake can run BEFORE the scene's
        // CountdownUI has set its static Instance (this is exactly what happened
        // in the WebGL build: the player wasn't frozen for the first 5 seconds
        // because Instance was still null here and we unlocked immediately).
        //
        // Instead we stay locked (IsLocked defaults to true) and resolve the
        // real state in Start()/OnEnable(), by which point every Awake has run.
    }

    void Start()
    {
        // Runs after ALL Awake() calls, so CountdownUI.Instance is reliable here
        // regardless of spawn/init order (fixes the WebGL "no freeze" bug).
        if (!freezeDuringCountdown)
        {
            IsLocked = false;
            return;
        }

        CountdownUI countdown = CountdownUI.Instance;
        if (countdown == null)
        {
            Debug.LogWarning("[PlayerMovement] No CountdownUI.Instance found in the scene — skipping the intro freeze.");
            IsLocked = false;
            return;
        }

        // Only unlock if the countdown has ALREADY run to completion (we missed
        // the event because we spawned too late). If it hasn't started yet OR is
        // currently counting, we STAY locked and wait for the finished event —
        // this is the key: "not started yet" must NOT unlock the player, which is
        // what caused the WebGL no-freeze bug when init order put us first.
        if (countdown.HasFinished)
        {
            IsLocked = false;
        }
    }

    void OnEnable()
    {
        if (freezeDuringCountdown && CountdownUI.Instance != null)
        {
            CountdownUI.Instance.OnCountdownFinished += HandleCountdownFinished;

            // If the countdown already completed before we subscribed, reconcile
            // immediately instead of waiting for an event that won't fire again.
            if (CountdownUI.Instance.HasFinished)
            {
                IsLocked = false;
            }
        }
    }

    void OnDisable()
    {
        if (freezeDuringCountdown && CountdownUI.Instance != null)
        {
            CountdownUI.Instance.OnCountdownFinished -= HandleCountdownFinished;
        }

        if (Instance == this)
        {
            Instance = null;
        }

        StopFootstepSound();
    }

    private void HandleCountdownFinished()
    {
        IsLocked = false;
    }

    /// <summary>
    /// Locks or unlocks movement/interaction on demand. Used internally for the
    /// intro countdown, and reusable for anything else that should freeze the
    /// player — e.g. WinScreenUI freezing the player once the mini-game is won.
    /// </summary>
    public void SetLocked(bool locked)
    {
        IsLocked = locked;

        if (locked)
        {
            moveInput = Vector2.zero;
            StopFootstepSound();
        }
    }

    void Update()
    {
        if (IsLocked)
        {
            moveInput = Vector2.zero;
            StopFootstepSound();
            return;
        }

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

        UpdateFootstepSound();
    }

    void UpdateFootstepSound()
    {
        if (footstepAudioSource == null || footstepAudioSource.clip == null)
            return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (isMoving && !footstepAudioSource.isPlaying)
        {
            footstepAudioSource.Play();
        }
        else if (!isMoving && footstepAudioSource.isPlaying)
        {
            footstepAudioSource.Stop();
        }
    }

    void StopFootstepSound()
    {
        if (footstepAudioSource != null && footstepAudioSource.isPlaying)
            footstepAudioSource.Stop();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
}