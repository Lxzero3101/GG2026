using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Debt Chase mini-game loop.
/// - Catch Progress: fills over time → WIN at 100
/// - Boss Patience:  drains over time → LOSE at 0
/// - Crash:          penalizes both meters
/// - Debtor:         lerps closer as catch progress increases (visual fake)
/// </summary>
public class MiniGameManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Inspector References
    // ─────────────────────────────────────────────

    [Header("Player References")]
    public PlayerCrash playerCrash;
    public PlayerLaneController playerLaneController;

    [Header("Spawner Reference")]
    public ObstacleSpawner obstacleSpawner;

    [Header("Debtor Reference")]
    public Transform debtorTransform;

    // ─────────────────────────────────────────────
    //  UI
    // ─────────────────────────────────────────────

    [Header("UI Sliders")]
    public Slider catchProgressBar;   // 0 → 100, fills green
    public Slider patienceBar;        // 100 → 0, drains red

    // ─────────────────────────────────────────────
    //  Catch Progress Tuning
    // ─────────────────────────────────────────────

    [Header("Catch Progress")]
    [Tooltip("Points per second the catch meter fills automatically")]
    public float catchFillRate = 8f;

    [Tooltip("Points subtracted from catch meter on crash")]
    public float catchCrashPenalty = 10f;

    // ─────────────────────────────────────────────
    //  Boss Patience Tuning
    // ─────────────────────────────────────────────

    [Header("Boss Patience")]
    [Tooltip("Points per second the patience meter drains automatically")]
    public float patienceDrainRate = 5f;

    [Tooltip("Points subtracted from patience meter on crash")]
    public float patienceCrashPenalty = 15f;

    // ─────────────────────────────────────────────
    //  Debtor Fake Movement
    // ─────────────────────────────────────────────

    [Header("Debtor Movement")]
    [Tooltip("Debtor X position at 0% catch progress (far away)")]
    public float debtorStartX = 4f;

    [Tooltip("How many units ahead of the Player the Debtor stops when caught (0 = overlap)")]
    public float debtorCatchOffset = 0.5f;

    // ─────────────────────────────────────────────
    //  Private State
    // ─────────────────────────────────────────────

    private float catchProgress = 0f;   // range [0, 100]
    private float patience = 100f;  // range [0, 100]
    private bool isGameOver = false;
    private float debtorEndX = 0f;   // calculated at Start() from player's actual X

    // ─────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────

    void Start()
    {
        // Init internal state
        catchProgress = 0f;
        patience = 100f;

        // Auto-calculate debtorEndX from Player's actual X position so
        // the debtor always ends up just ahead of the player, not hardcoded.
        if (playerLaneController != null)
            debtorEndX = playerLaneController.transform.position.x + debtorCatchOffset;
        else
            debtorEndX = -2.5f; // fallback if reference missing

        // Init UI sliders
        InitSlider(catchProgressBar, 0f, 100f, catchProgress);
        InitSlider(patienceBar, 0f, 100f, patience);

        // Subscribe to crash event (always unsubscribe in OnDestroy)
        if (playerCrash != null)
            playerCrash.OnCrash += HandleCrash;
        else
            Debug.LogWarning("[MiniGameManager] PlayerCrash reference is missing!");

        // Hand control of spawning to this manager
        // NOTE: Make sure ObstacleSpawner.Start() does NOT auto-set IsSpawning = true
        if (obstacleSpawner != null)
            obstacleSpawner.StartSpawning();
        else
            Debug.LogWarning("[MiniGameManager] ObstacleSpawner reference is missing!");

        // Place debtor at starting position
        UpdateDebtorPosition();
    }

    void OnDestroy()
    {
        // Always clean up event subscriptions to avoid memory leaks
        if (playerCrash != null)
            playerCrash.OnCrash -= HandleCrash;
    }

    void Update()
    {
        if (isGameOver) return;

        // ── Catch Progress fills over time ──
        catchProgress = Mathf.Clamp(catchProgress + catchFillRate * Time.deltaTime, 0f, 100f);

        // ── Patience drains over time ──
        patience = Mathf.Clamp(patience - patienceDrainRate * Time.deltaTime, 0f, 100f);

        // ── Update UI ──
        if (catchProgressBar != null) catchProgressBar.value = catchProgress;
        if (patienceBar != null) patienceBar.value = patience;

        // ── Update debtor fake position ──
        UpdateDebtorPosition();

        // ── Win / Lose check ──
        if (catchProgress >= 100f)
            TriggerWin();
        else if (patience <= 0f)
            TriggerLose();
    }

    // ─────────────────────────────────────────────
    //  Crash Handler
    // ─────────────────────────────────────────────

    private void HandleCrash()
    {
        if (isGameOver) return;

        catchProgress = Mathf.Max(0f, catchProgress - catchCrashPenalty);
        patience = Mathf.Max(0f, patience - patienceCrashPenalty);

        Debug.Log($"[MiniGameManager] CRASH — CatchProgress: {catchProgress:F1} | Patience: {patience:F1}");
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
    //  Win / Lose
    // ─────────────────────────────────────────────

    private void TriggerWin()
    {
        EndGame();
        Debug.Log("[MiniGameManager] WIN — Debtor caught! 🎉");
        // TODO: Fire a public event or call boss UI / scene transition here
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
    public float Patience => patience;
    public bool IsGameOver => isGameOver;
}
