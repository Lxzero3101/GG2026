using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MiniGameManager4 : MonoBehaviour
{
    public static MiniGameManager4 Instance { get; private set; }

    [Header("Win Condition Settings")]
    [SerializeField] private int targetMoneyRequirement = 100;

    [Header("Counter Settings")]
    [SerializeField] private int maxAllowedAttempts = 4;

    [Header("Boss Reaction Settings")]
    [Tooltip("Items worth less than this trigger a boss warning (facial flash + shake) with no patience penalty.")]
    [SerializeField] private int lowValueThreshold = 5;

    [Header("Win Flow Settings")]
    [Tooltip("Scene object reference — used to load the next scene after winning (its own Next Scene Name field decides where to go).")]
    [SerializeField] private GameManager gameManager;
    [Tooltip("Seconds to wait after winning before automatically loading the next scene.")]
    [SerializeField] private float winToNextSceneDelay = 3f;

    [Tooltip("Seconds to hold on the boss's furious reaction before loading the lose scene (attempts exhausted).")]
    [SerializeField] private float loseToLoseSceneDelay = 2f;

    [Header("Game State Readouts")]
    [SerializeField] private int currentTotalMoney = 0;
    [SerializeField] private int currentAttempts = 0;

    [Header("Events (Optional UI hookup)")]
    [SerializeField] private UnityEvent onWin;
    [SerializeField] private UnityEvent onLose;

    private bool isGameOver = false;

    /// <summary>Items worth less than this trigger the boss's low-value warning reaction.</summary>
    public int LowValueThreshold => lowValueThreshold;

    /// <summary>The money total needed to win — read-only, for HUD display.</summary>
    public int TargetMoneyRequirement => targetMoneyRequirement;

    /// <summary>The player's current money total — read-only, for HUD display.</summary>
    public int CurrentTotalMoney => currentTotalMoney;

    /// <summary>How many attempts/clicks the player has used so far — read-only, for HUD display.</summary>
    public int CurrentAttempts => currentAttempts;

    /// <summary>The maximum number of attempts allowed — read-only, for HUD display.</summary>
    public int MaxAllowedAttempts => maxAllowedAttempts;

    /// <summary>Raised whenever the money total (and attempts) changes, passing the new money total. Lets a HUD counter react without polling every frame.</summary>
    public event Action<int> OnMoneyChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ProcessItemClick(int itemMoney)
    {
        if (isGameOver) return;

        // Increment attempts and update score
        currentAttempts++;
        currentTotalMoney += itemMoney;

        Debug.Log($"Attempt: {currentAttempts}/{maxAllowedAttempts} | Current Money: ${currentTotalMoney}");

        OnMoneyChanged?.Invoke(currentTotalMoney);

        EvaluateGameState();
    }

    private void EvaluateGameState()
    {
        // Check Win Condition: player reached target money requirement
        if (currentTotalMoney >= targetMoneyRequirement)
        {
            TriggerWin();
            return;
        }

        // Check Lose Condition: reached max attempts without meeting money requirement
        if (currentAttempts >= maxAllowedAttempts)
        {
            TriggerLose();
        }
    }

    private void TriggerWin()
    {
        isGameOver = true;
        Debug.Log("<color=green><b>YOU WIN!</b></color> You reached the required money target!");

        // Freeze the player and hold patience/boss reaction steady so the reward
        // is the last thing shown before the scene transition.
        PlayerMovement.Instance?.SetLocked(true);
        GameUI.Instance?.PausePatience();
        GameUI.Instance?.ImproveBossExpression(2);

        onWin?.Invoke();
        StartCoroutine(LoadNextSceneAfterDelay());
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(winToNextSceneDelay);

        if (gameManager != null)
        {
            gameManager.LoadNextScene();
        }
        else
        {
            Debug.LogWarning("[MiniGameManager4] GameManager reference is missing — can't auto-load the next scene.");
        }
    }

    private void TriggerLose()
    {
        isGameOver = true;
        Debug.Log("<color=red><b>YOU LOSE!</b></color> Out of attempts and did not reach the target money!");

        PlayerMovement.Instance?.SetLocked(true);
        GameUI.Instance?.PausePatience();
        GameUI.Instance?.SetBossExpression(BossExpression.Furious);

        onLose?.Invoke();
        StartCoroutine(LoadLoseSceneAfterDelay());
    }

    private IEnumerator LoadLoseSceneAfterDelay()
    {
        yield return new WaitForSeconds(loseToLoseSceneDelay);

        if (gameManager != null)
        {
            gameManager.LoadLoseScene();
        }
        else
        {
            Debug.LogWarning("[MiniGameManager4] GameManager reference is missing — can't auto-load the lose scene.");
        }
    }
}