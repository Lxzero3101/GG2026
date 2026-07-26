using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Per-scene orchestrator for a mini-game run: plays the intro countdown, resets
/// patience for the fresh run, listens for the patience-depleted (lose) event,
/// and handles the win flow (pause patience, reveal win button, load next scene).
/// Talks to UI exclusively through <see cref="GameUI"/> — never PatienceBarUI or
/// BossPortraitUI directly — keeping this class free to focus on game flow only.
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private GameUI gameUI;
    [SerializeField] private CountdownUI countdownUI;
    [SerializeField] private GameObject winButton;

    [Tooltip("Scene loaded when the player clicks the win button.")]
    [SerializeField] private string nextSceneName = "NextLevel";

    [Tooltip("Scene loaded when patience reaches zero.")]
    [SerializeField] private string loseSceneName = "Lose";

    [Tooltip("Seconds to hold on the boss's furious reaction before loading the lose scene.")]
    [SerializeField] private float loseTransitionDelay = 1.5f;

    private bool isRoundActive;

    private void Awake()
    {
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
    }

    private void Start()
    {
        BeginMiniGameIntro();
    }

    /// <summary>
    /// Resets patience to full, pauses draining, and plays the intro countdown.
    /// Call this whenever a fresh mini-game run begins (e.g. on scene load).
    /// </summary>
    public void BeginMiniGameIntro()
    {
        isRoundActive = false;

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
    }

    private void HandlePatienceDepleted()
    {
        if (!isRoundActive)
        {
            return;
        }

        isRoundActive = false;

        gameUI?.PausePatience();
        gameUI?.SetBossExpression(BossExpression.Furious);
        PlayerMovement.Instance?.SetLocked(true);

        StartCoroutine(LoadLoseSceneAfterDelay());
    }

    private IEnumerator LoadLoseSceneAfterDelay()
    {
        yield return new WaitForSeconds(loseTransitionDelay);
        // Global patience loss — reset NoMP to 0 and go to Lose (via MiniGameResult).
        MiniGameResult.ReportLoss();
    }

    /// <summary>
    /// Loads the same lose scene used for patience depletion. Exposed publicly so
    /// other lose conditions (e.g. MiniGameManager4 running out of attempts) can
    /// reuse it instead of duplicating the scene name.
    /// </summary>
    public void LoadLoseScene()
    {
        // Single lose funnel: reset NoMP to 0, then load the Lose scene.
        MiniGameResult.ReportLoss();
    }

    /// <summary>
    /// Call this when the player wins the mini-game (e.g. catches the debtor).
    /// Pauses patience draining and reveals the win button.
    /// </summary>
    public void OnPlayerWin()
    {
        if (!isRoundActive)
        {
            return;
        }

        isRoundActive = false;
        gameUI?.PausePatience();

        if (winButton != null)
        {
            winButton.SetActive(true);
        }
    }

    /// <summary>Wire this to the win button's OnClick() to advance to the next scene.</summary>
    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}