using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Scene-specific orchestrator for the debt-collector tug-of-war minigame.
/// Everything here belongs to THIS scene only: it plays the intro countdown,
/// starts/stops <see cref="PowerBarController"/>, reacts to the indicator leaving
/// or entering its TargetZone (forces an angry, shaking boss portrait), handles
/// win/lose (pause patience drain, improve the boss's mood by a few stages on a
/// win, hide the bar, wait, then load the next scene).
///
/// Deliberately does NOT modify GameUI, GameManager, or BossExpressionController —
/// those live on the reusable UI Canvas prefab and are shared across scenes. This
/// script only calls their existing public members. The one exception is the
/// "out of zone" reaction: BossExpressionController re-applies the patience-based
/// expression every single frame (it listens to PatienceBarUI's per-frame drain
/// event), so a plain external SetExpression call would get overwritten within a
/// frame. To win that race honestly — without adding any override flag to
/// BossExpressionController — this script re-asserts the override in LateUpdate,
/// which Unity always runs after every script's Update this frame, guaranteeing
/// it has the final say while the override is active. The moment the override
/// turns off, BossExpressionController's own per-frame updates simply resume
/// controlling the portrait on the very next frame — nothing to hand back manually.
///
/// SETUP: attach this to an empty GameObject. Drag the UI Canvas (containing
/// GameUI, CountdownUI, PatienceBarUI, BossExpressionController, BossPortraitShake)
/// into <see cref="uiCanvas"/>, and drag the tug-of-war object into
/// <see cref="powerBarController"/>. Do NOT also keep GameManager active in this
/// scene — this script replaces its job here. GameManager stays untouched for use
/// in other scenes that don't have this minigame.
/// </summary>
public class MiniGameFlowController : MonoBehaviour
{
    [Header("Scene References")]
    [Tooltip("The UI Canvas containing GameUI, CountdownUI, PatienceBarUI, BossExpressionController, BossPortraitShake, etc.")]
    [SerializeField] private GameObject uiCanvas;

    [Tooltip("The tug-of-war gameplay object for this scene.")]
    [SerializeField] private PowerBarController powerBarController;

    [Tooltip("Optional. Shown when the round is won.")]
    [SerializeField] private GameObject winButton;

    [Header("Scene Transition")]
    [Tooltip("Scene loaded when the round is won.")]
    [SerializeField] private string nextSceneName = "NextLevel";

    [Tooltip("Scene loaded when the round is lost.")]
    [SerializeField] private string loseSceneName = "Lose";

    [Tooltip("Seconds to wait after winning (bar already hidden instantly) before loading the next scene.")]
    [SerializeField] private float winSceneLoadDelay = 5f;

    [Tooltip("Seconds to wait after losing (bar already hidden instantly) before loading the lose scene.")]
    [SerializeField] private float loseSceneLoadDelay = 5f;

    [Header("Win Reward")]
    [Tooltip("How many expression stages the boss calms down by on a win (e.g. 2: stage 'Disappointed' -> stage 'Neutral'). Clamped so it never goes past the calmest stage.")]
    [SerializeField] private int winStageImprovement = 2;

    [Header("Out Of Zone Reaction")]
    [Tooltip("Expression forced while the indicator is outside the TargetZone.")]
    [SerializeField] private BossExpression outOfZoneExpression = BossExpression.Angry;

    // Discovered from uiCanvas at Awake — not hand-assigned, so the Inspector stays
    // to just the two references above plus tuning values.
    private GameUI gameUI;
    private CountdownUI countdownUI;
    private BossExpressionController bossExpressionController;
    private BossPortraitShake portraitShake;

    private bool isRoundActive;
    private bool isOutOfZoneOverride;

    private void Awake()
    {
        if (uiCanvas != null)
        {
            gameUI = uiCanvas.GetComponentInChildren<GameUI>(true);
            countdownUI = uiCanvas.GetComponentInChildren<CountdownUI>(true);
            bossExpressionController = uiCanvas.GetComponentInChildren<BossExpressionController>(true);
            portraitShake = uiCanvas.GetComponentInChildren<BossPortraitShake>(true);
        }
        else
        {
            Debug.LogWarning("[MiniGameFlowController] uiCanvas reference is missing.");
        }

        if (gameUI == null) Debug.LogWarning("[MiniGameFlowController] Could not find GameUI under uiCanvas.");
        if (countdownUI == null) Debug.LogWarning("[MiniGameFlowController] Could not find CountdownUI under uiCanvas.");
        if (bossExpressionController == null) Debug.LogWarning("[MiniGameFlowController] Could not find BossExpressionController under uiCanvas.");
        if (portraitShake == null) Debug.LogWarning("[MiniGameFlowController] Could not find BossPortraitShake under uiCanvas.");
        if (powerBarController == null) Debug.LogWarning("[MiniGameFlowController] PowerBarController reference is missing.");

        if (winButton != null)
        {
            winButton.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (gameUI != null)
        {
            gameUI.OnPatienceDepleted += HandlePatienceDepleted;
        }

        if (countdownUI != null)
        {
            countdownUI.OnCountdownFinished += HandleCountdownFinished;
        }

        if (powerBarController != null)
        {
            powerBarController.OnMiniGameWon += HandleMiniGameWon;
            powerBarController.OnMiniGameLost += HandleMiniGameLost;
            powerBarController.OnTargetZoneStatusChanged += HandleTargetZoneStatusChanged;
        }
    }

    private void OnDisable()
    {
        if (gameUI != null)
        {
            gameUI.OnPatienceDepleted -= HandlePatienceDepleted;
        }

        if (countdownUI != null)
        {
            countdownUI.OnCountdownFinished -= HandleCountdownFinished;
        }

        if (powerBarController != null)
        {
            powerBarController.OnMiniGameWon -= HandleMiniGameWon;
            powerBarController.OnMiniGameLost -= HandleMiniGameLost;
            powerBarController.OnTargetZoneStatusChanged -= HandleTargetZoneStatusChanged;
        }
    }

    private void Start()
    {
        BeginMiniGameIntro();
    }

    private void LateUpdate()
    {
        // Always runs after every other script's Update this frame — including
        // BossExpressionController's per-frame patience-driven expression refresh —
        // so this has the final say while the override is active.
        if (isOutOfZoneOverride)
        {
            gameUI?.SetBossExpression(outOfZoneExpression);
            portraitShake?.SetIdleShaking(true);
        }
    }

    private void BeginMiniGameIntro()
    {
        isRoundActive = false;
        isOutOfZoneOverride = false;

        gameUI?.ResetPatience();
        gameUI?.PausePatience();

        if (winButton != null)
        {
            winButton.SetActive(false);
        }

        if (countdownUI != null)
        {
            countdownUI.StartCountdown();
        }
        else
        {
            // No countdown assigned — start the round immediately.
            HandleCountdownFinished();
        }
    }

    private void HandleCountdownFinished()
    {
        isRoundActive = true;
        gameUI?.ResumePatience();
        powerBarController?.BeginRound();
    }

    private void HandleTargetZoneStatusChanged(bool isOutsideZone)
    {
        isOutOfZoneOverride = isOutsideZone;
        // Nothing else to do on revert: once this is false, LateUpdate stops
        // overriding and BossExpressionController's own per-frame updates take
        // back over — expression and shake both self-correct within a frame.
    }

    private void HandlePatienceDepleted()
    {
        if (!isRoundActive) return;

        isRoundActive = false;
        TriggerLoss();
    }

    private void HandleMiniGameWon()
    {
        if (!isRoundActive) return;

        isRoundActive = false;
        ApplyWinExpressionImprovement();
        TriggerWin();
    }

    private void HandleMiniGameLost()
    {
        if (!isRoundActive) return;

        isRoundActive = false;
        TriggerLoss();
    }

    /// <summary>
    /// Moves the boss expression stage back toward Neutral by <see cref="winStageImprovement"/>
    /// steps (e.g. stage index 2 "Disappointed" - 2 = stage index 0 "Neutral"). Clamped at 0
    /// so an already-calm boss just stays at Neutral rather than wrapping or going negative.
    /// Assumes BossExpression's enum order matches BossExpressionController's stage order
    /// (Neutral, Annoyed, Disappointed, Angry, Furious) — true for the default 5-stage setup.
    /// </summary>
    private void ApplyWinExpressionImprovement()
    {
        if (bossExpressionController == null || gameUI == null)
        {
            return;
        }

        int newStage = Mathf.Max(bossExpressionController.CurrentStageIndex - winStageImprovement, 0);
        gameUI.SetBossExpression((BossExpression)newStage);
    }

    private void TriggerWin()
    {
        isOutOfZoneOverride = false;
        gameUI?.PausePatience();
        powerBarController?.HideBar();

        if (winButton != null)
        {
            winButton.SetActive(true);
        }

        // Keep the win delay/feedback, but route through GameData instead of a
        // hardcoded scene: +1 NoMP, then Office (or Finish at 4/4).
        StartCoroutine(ReportAfterDelay(true, winSceneLoadDelay));
    }

    private void TriggerLoss()
    {
        isOutOfZoneOverride = false;
        gameUI?.PausePatience();
        powerBarController?.HideBar();

        // Reset NoMP to 0, then go to the Lose scene — handled by MiniGameResult.
        StartCoroutine(ReportAfterDelay(false, loseSceneLoadDelay));
    }

    private IEnumerator ReportAfterDelay(bool won, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (won)
            MiniGameResult.ReportWin();
        else
            MiniGameResult.ReportLoss();
    }
}