using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the Debt Chase mini-game loop.
/// - Countdown:      the round is fully paused (no spawn, no patience drain,
///   no catch-progress fill, no parallax scroll) until CountdownUI finishes.
/// - Catch Progress: fills over time → WIN at 100 (owned locally by this class)
/// - Boss Patience:  owned entirely by GameUI/PatienceBarUI (the real UI) —
///   this class only listens for GameUI.OnPatienceDepleted to trigger a LOSE.
/// - Crash:          penalizes catch progress here; the obstacle's own
///   ObstaclePatiencePenalty already applies the patience penalty + boss
///   reaction on the same collision, so this class must NOT touch patience too.
/// - Debtor:         lerps closer as catch progress increases (visual fake)
/// - Win:            pauses everything, waits winLoadDelay seconds, then loads winSceneName.
/// </summary>
public class MiniGameManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Inspector References
    // ─────────────────────────────────────────────

    [Header("Player References")]
    [SerializeField] private PlayerCrash playerCrash;
    [SerializeField] private PlayerLaneController playerLaneController;

    [Header("Spawner Reference")]
    [SerializeField] private ObstacleSpawner obstacleSpawner;

    [Header("Debtor Reference")]
    [SerializeField] private Transform debtorTransform;

    [Header("Boss UI")]
    [Tooltip("The real GameUI bridge — owns the patience bar + boss portrait. This replaces the old placeholder patience Slider.")]
    [SerializeField] private GameUI gameUI;

    [Header("Countdown")]
    [Tooltip("Round stays fully paused (no spawn/drain/fill/parallax) until this finishes.")]
    [SerializeField] private CountdownUI countdownUI;

    [Header("Parallax Backgrounds")]
    [Tooltip("All scrolling background layers — paused during countdown and after win/lose.")]
    [SerializeField] private ParallaxBackground[] parallaxBackgrounds;

    // ─────────────────────────────────────────────
    //  UI
    // ─────────────────────────────────────────────

    [Header("UI Sliders")]
    [SerializeField] private Slider catchProgressBar; // 0 → 100, fills green

    // ─────────────────────────────────────────────
    //  Catch Progress Tuning
    // ─────────────────────────────────────────────

    [Header("Catch Progress")]
    [Tooltip("Points per second the catch meter fills automatically")]
    [SerializeField] private float catchFillRate = 8f;

    [Tooltip("Points subtracted from catch meter on crash")]
    [SerializeField] private float catchCrashPenalty = 10f;

    // ─────────────────────────────────────────────
    //  Debtor Fake Movement
    // ─────────────────────────────────────────────

    [Header("Debtor Movement")]
    [Tooltip("Debtor X position at 0% catch progress (far away)")]
    [SerializeField] private float debtorStartX = 4f;

    [Tooltip("How many units ahead of the Player the Debtor stops when caught (0 = overlap)")]
    [SerializeField] private float debtorCatchOffset = 0.5f;

    // ─────────────────────────────────────────────
    //  Win Sequence
    // ─────────────────────────────────────────────

    [Header("Win Sequence")]
    [Tooltip("Seconds to wait after winning (fully paused) before loading the next scene.")]
    [SerializeField] private float winLoadDelay = 5f;

    [Tooltip("Scene to load after the win delay elapses.")]
    [SerializeField] private string winSceneName = "NextLevel";

    // ─────────────────────────────────────────────
    //  Private State
    // ─────────────────────────────────────────────

    private float catchProgress = 0f;   // range [0, 100]
    private bool isGameOver = false;
    private bool roundStarted = false;  // true once the countdown finishes
    private float debtorEndX = 0f;      // calculated at Start() from player's actual X

    // ─────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────

    void Start()
    {
        // Init internal state
        catchProgress = 0f;

        // Auto-calculate debtorEndX from Player's actual X position so
        // the debtor always ends up just ahead of the player, not hardcoded.
        if (playerLaneController != null)
            debtorEndX = playerLaneController.transform.position.x + debtorCatchOffset;
        else
            debtorEndX = -2.5f; // fallback if reference missing

        // Init catch progress UI slider only — patience bar UI is owned by GameUI.
        InitSlider(catchProgressBar, 0f, 100f, catchProgress);

        // Subscribe to crash event (always unsubscribe in OnDestroy)
        if (playerCrash != null)
            playerCrash.OnCrash += HandleCrash;
        else
            Debug.LogWarning("[MiniGameManager] PlayerCrash reference is missing!");

        // Subscribe to the REAL patience-depleted event instead of polling a
        // locally-duplicated patience float. Keep it paused until the round starts.
        if (gameUI != null)
        {
            gameUI.OnPatienceDepleted += HandlePatienceDepleted;
            gameUI.ResetPatience();
            gameUI.PausePatience();
        }
        else
        {
            Debug.LogWarning("[MiniGameManager] GameUI reference is missing!");
        }

        if (obstacleSpawner == null)
            Debug.LogWarning("[MiniGameManager] ObstacleSpawner reference is missing!");

        // Place debtor at starting position and keep backgrounds still until the round starts.
        UpdateDebtorPosition();
        SetParallaxPaused(true);
        // Lock player movement until the countdown finishes.
        if (playerLaneController != null)
            playerLaneController.InputLocked = true;

        // Everything (spawning, patience drain, catch-progress fill) stays paused
        // until the countdown finishes.
        if (countdownUI != null)
        {
            countdownUI.OnCountdownFinished += HandleCountdownFinished;
            countdownUI.StartCountdown();
        }
        else
        {
            Debug.LogWarning("[MiniGameManager] CountdownUI reference missing — starting round immediately.");
            HandleCountdownFinished();
        }
    }

    void OnDestroy()
    {
        // Always clean up event subscriptions to avoid memory leaks
        if (playerCrash != null)
            playerCrash.OnCrash -= HandleCrash;

        if (gameUI != null)
            gameUI.OnPatienceDepleted -= HandlePatienceDepleted;

        if (countdownUI != null)
            countdownUI.OnCountdownFinished -= HandleCountdownFinished;
    }

    void Update()
    {
        if (isGameOver || !roundStarted) return;

        // ── Catch Progress fills over time ──
        catchProgress = Mathf.Clamp(catchProgress + catchFillRate * Time.deltaTime, 0f, 100f);

        // ── Update UI ──
        if (catchProgressBar != null) catchProgressBar.value = catchProgress;

        // ── Update debtor fake position ──
        UpdateDebtorPosition();

        // ── Win check (Lose is event-driven — see HandlePatienceDepleted) ──
        if (catchProgress >= 100f)
            TriggerWin();
    }

    // ─────────────────────────────────────────────
    //  Countdown Handler
    // ─────────────────────────────────────────────

    private void HandleCountdownFinished()
    {
        roundStarted = true;

        SetParallaxPaused(false);
        gameUI?.ResumePatience();
        obstacleSpawner?.StartSpawning();

            // Player can only start moving once the countdown ends.
        if (playerLaneController != null)
            playerLaneController.InputLocked = false;
    }

    // ─────────────────────────────────────────────
    //  Crash Handler
    // ─────────────────────────────────────────────

    private void HandleCrash()
    {
        if (isGameOver) return;

        // Patience penalty is NOT applied here on purpose: the obstacle that
        // caused this crash also carries an ObstaclePatiencePenalty component,
        // which already calls gameUI.ApplyObstacleHit() on the same trigger
        // event. Penalizing patience again here would double-count the hit.
        catchProgress = Mathf.Max(0f, catchProgress - catchCrashPenalty);

        Debug.Log($"[MiniGameManager] CRASH — CatchProgress: {catchProgress:F1}");
    }

    private void HandlePatienceDepleted()
    {
        if (isGameOver) return;
        TriggerLose();
    }

    // ─────────────────────────────────────────────
    //  Debtor Position
    // ─────────────────────────────────────────────

    private void UpdateDebtorPosition()
    {
        if (debtorTransform == null) return;

        float t = catchProgress / 100f;
        float targetX = Mathf.Lerp(debtorStartX, debtorEndX, t);

        // Only move on X axis — preserve current Y (lane handled elsewhere if needed)
        debtorTransform.position = new Vector3(targetX,
                                               debtorTransform.position.y,
                                               debtorTransform.position.z);
    }

    // ─────────────────────────────────────────────
    //  Parallax Helper
    // ─────────────────────────────────────────────

    private void SetParallaxPaused(bool paused)
    {
        if (parallaxBackgrounds == null) return;

        foreach (var bg in parallaxBackgrounds)
            bg?.SetPaused(paused);
    }

    // ─────────────────────────────────────────────
    //  Win / Lose
    // ─────────────────────────────────────────────

    private void TriggerWin()
    {
        EndGame();
        Debug.Log("[MiniGameManager] WIN — Debtor caught! 🎉");
        StartCoroutine(WinSequenceRoutine());
    }

    private IEnumerator WinSequenceRoutine()
    {
        yield return new WaitForSeconds(winLoadDelay);

        if (!string.IsNullOrEmpty(winSceneName))
            SceneManager.LoadScene(winSceneName);
        else
            Debug.LogWarning("[MiniGameManager] winSceneName is empty — not loading a scene.");
    }

    private void TriggerLose()
    {
        EndGame();
        Debug.Log("[MiniGameManager] LOSE — Boss patience ran out! 💢");
        // TODO: Fire a public event or call boss UI / scene transition here
    }

    private void EndGame()
    {
        isGameOver = true;

        // Stop obstacles
        obstacleSpawner?.StopSpawning();

        // Freeze the boss's drain so the top bar doesn't keep ticking after game over
        gameUI?.PausePatience();

        // Freeze the scrolling background too
        SetParallaxPaused(true);

        // Lock player input
        if (playerLaneController != null)
            playerLaneController.InputLocked = true;
    }

    // ─────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────

    private static void InitSlider(Slider slider, float min, float max, float value)
    {
        if (slider == null) return;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
    }

    // ─────────────────────────────────────────────
    //  Public Accessors (for boss UI / other systems)
    // ─────────────────────────────────────────────

    public float CatchProgress => catchProgress;
    public float Patience => gameUI != null ? gameUI.GetNormalizedPatience() * 100f : 0f;
    public bool IsGameOver => isGameOver;
}