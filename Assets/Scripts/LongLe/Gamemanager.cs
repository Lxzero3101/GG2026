using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Per-scene orchestrator for a mini-game run: plays the intro countdown, resets
/// patience for the fresh run, listens for the patience-depleted (lose) event and
/// for PowerBarController's win/lose events, and handles both end-of-round flows
/// (pause patience, hide the tug-of-war bar, wait, then load the next scene).
/// Talks to UI exclusively through <see cref="GameUI"/> — never PatienceBarUI or
/// BossPortraitUI directly — keeping this class free to focus on game flow only.
/// PowerBarController is gameplay (not UI), so it's referenced directly.
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private GameUI gameUI;
    [SerializeField] private CountdownUI countdownUI;
    [SerializeField] private PowerBarController powerBarController;
    [SerializeField] private GameObject winButton;

    [Tooltip("Scene loaded when the round is won.")]
    [SerializeField] private string nextSceneName = "NextLevel";

    [Tooltip("Scene loaded when the round is lost.")]
    [SerializeField] private string loseSceneName = "Lose";

    [Header("Round End Transition")]
    [Tooltip("Seconds to wait after winning (bar already hidden instantly) before loading the next scene.")]
    [SerializeField] private float winSceneLoadDelay = 5f;

    [Tooltip("Seconds to wait after losing (bar already hidden instantly) before loading the lose scene.")]
    [SerializeField] private float loseSceneLoadDelay = 5f;

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

        if (powerBarController != null)
        {
            powerBarController.OnMiniGameWon += HandleMiniGameWon;
            powerBarController.OnMiniGameLost += HandleMiniGameLost;
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
        }
    }

    private void Start()
    {
        BeginMiniGameIntro();
    }

    /// <summary>
    /// Resets patience to full, pauses draining, and plays the intro countdown.
    /// Call this whenever a fresh mini-game run begins (e.g. on scene load).
    /// The tug-of-war bar (PowerBarController) stays inactive — indicator won't
    /// move — until the countdown finishes.
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
        powerBarController?.BeginRound();
    }

    private void HandlePatienceDepleted()
    {
        if (!isRoundActive)
        {
            return;
        }

        isRoundActive = false;
        TriggerLoss();
    }

    private void HandleMiniGameWon()
    {
        if (!isRoundActive)
        {
            return;
        }

        isRoundActive = false;
        TriggerWin();
    }

    private void HandleMiniGameLost()
    {
        if (!isRoundActive)
        {
            return;
        }

        isRoundActive = false;
        TriggerLoss();
    }

    /// <summary>
    /// Call this when the player wins the mini-game through some other path
    /// (e.g. a manual "catch the debtor" trigger). Pauses patience draining,
    /// hides the bar, and starts the delayed transition to the next scene.
    /// </summary>
    public void OnPlayerWin()
    {
        if (!isRoundActive)
        {
            return;
        }

        isRoundActive = false;
        TriggerWin();
    }

    private void TriggerWin()
    {
        gameUI?.PausePatience();
        powerBarController?.HideBar();

        if (winButton != null)
        {
            winButton.SetActive(true);
        }

        StartCoroutine(LoadSceneAfterDelay(nextSceneName, winSceneLoadDelay));
    }

    private void TriggerLoss()
    {
        gameUI?.PausePatience();
        powerBarController?.HideBar();

        StartCoroutine(LoadSceneAfterDelay(loseSceneName, loseSceneLoadDelay));
    }

    private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>Optional: wire this to the win button's OnClick() to skip the wait and advance immediately.</summary>
    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}