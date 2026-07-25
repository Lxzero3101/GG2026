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

    [Header("Game State Readouts")]
    [SerializeField] private int currentTotalMoney = 0;
    [SerializeField] private int currentAttempts = 0;

    [Header("Events (Optional UI hookup)")]
    [SerializeField] private UnityEvent onWin;
    [SerializeField] private UnityEvent onLose;

    private bool isGameOver = false;

    /// <summary>Items worth less than this trigger the boss's low-value warning reaction.</summary>
    public int LowValueThreshold => lowValueThreshold;

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
        onWin?.Invoke();
    }

    private void TriggerLose()
    {
        isGameOver = true;
        Debug.Log("<color=red><b>YOU LOSE!</b></color> Out of attempts and did not reach the target money!");
        onLose?.Invoke();
    }
}