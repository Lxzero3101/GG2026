using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WinManager : MonoBehaviour
{
    public static WinManager Instance { get; private set; }

    [Header("Win Settings")]
    [Tooltip("If true, automatically counts all WinConditionObject instances in the scene at Start.")]
    public bool autoDetectRequiredCount = true;

    [Tooltip("Only used if Auto Detect is off.")]
    public int requiredCount = 4;

    [Header("Events")]
    public UnityEvent<int, int> onProgress; // (current, total)
    public UnityEvent onWin;

    private readonly HashSet<WinConditionObject> registered = new HashSet<WinConditionObject>();
    private bool hasWon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (autoDetectRequiredCount)
        {
            requiredCount = FindObjectsByType<WinConditionObject>(FindObjectsSortMode.None).Length;
            Debug.Log($"WinManager auto-detected {requiredCount} win objects.");
        }
    }

    public void RegisterWin(WinConditionObject obj)
    {
        if (hasWon) return;

        // HashSet prevents double-counting the same object even if it's
        // interacted with more than once.
        if (!registered.Add(obj)) return;

        Debug.Log($"Win objects interacted with: {registered.Count}/{requiredCount}");
        onProgress?.Invoke(registered.Count, requiredCount);

        if (registered.Count >= requiredCount)
        {
            hasWon = true;
            WinGame();
        }
    }

    private void WinGame()
    {
        Debug.Log("You Win!");
        onWin?.Invoke();
        // e.g. UnityEngine.SceneManagement.SceneManager.LoadScene("WinScene");
    }
}