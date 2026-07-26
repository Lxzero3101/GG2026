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

    // True until the intro-countdown freeze has been resolved (released) once.
    // After that, PollCountdownLock() does nothing, so later gameplay locks
    // (win/lose freezes via SetLocked) are never fought by the countdown poll.
    private bool introFreezeResolved;

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

        // NOTE: We intentionally do NOT decide the lock state here, and we do NOT
        // rely on subscribing to CountdownUI's finished event. This script lives
        // on the Player PREFAB, instantiated at runtime, so its Awake/OnEnable can
        // run either BEFORE or AFTER the scene's CountdownUI sets its static
        // Instance. Event-subscription timing is therefore unreliable in a build
        // (it worked in the Editor by luck of init order, but the WebGL build
        // ordered things so the subscription was skipped and the unlock event was
        // never received — the player stayed frozen forever).
        //
        // Instead we stay locked (IsLocked defaults to true) and simply POLL the
        // countdown's state every frame while locked (see Update). This is
        // completely order-independent and costs one bool check per frame.
    }

    void Start()
    {
        // If freezing is disabled, or there's genuinely no countdown in the
        // scene, don't hold the player. Otherwise leave IsLocked = true and let
        // the per-frame poll in Update() release it when the countdown finishes.
        if (!freezeDuringCountdown || CountdownUI.Instance == null)
        {
            if (freezeDuringCountdown && CountdownUI.Instance == null)
            {
                Debug.LogWarning("[PlayerMovement] No CountdownUI.Instance found in the scene — skipping the intro freeze.");
            }

            IsLocked = false;
            introFreezeResolved = true;
        }
    }

    void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        StopFootstepSound();
    }

    /// <summary>
    /// While locked for the intro countdown, releases the player the moment the
    /// countdown reports it has finished. Polling (instead of the event) makes
    /// this immune to prefab-vs-scene init order in builds. Once released here,
    /// the flag stays cleared for the rest of the countdown lifetime.
    /// </summary>
    private void PollCountdownLock()
    {
        // Only ever acts on the ONE-TIME intro freeze. Once resolved, it never
        // touches IsLocked again, so win/lose freezes via SetLocked are safe.
        if (introFreezeResolved || !freezeDuringCountdown)
        {
            return;
        }

        CountdownUI countdown = CountdownUI.Instance;
        if (countdown == null)
        {
            // No countdown ever appeared — don't strand the player.
            IsLocked = false;
            introFreezeResolved = true;
            return;
        }

        // Unlock only once the countdown has actually completed. "Not started
        // yet" and "currently counting" both keep the player frozen.
        if (countdown.HasFinished)
        {
            IsLocked = false;
            introFreezeResolved = true;
        }
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
        // Release the intro-countdown freeze as soon as the countdown finishes.
        // Done here by polling rather than via an event, so it can't be broken by
        // prefab-vs-scene script init order in a build.
        PollCountdownLock();

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