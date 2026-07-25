using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WinManager : MonoBehaviour
{
    public static WinManager Instance { get; private set; }

    [Header("Win Settings")]
    public bool autoDetectRequiredCount = true;
    public int requiredCount = 4;

    [Header("Player Speech")]
    [Tooltip("Auto-filled at runtime when the spawned Player's SpeechBubble registers itself. No need to assign manually.")]
    public SpeechBubble playerSpeechBubble;

    [TextArea]
    public string playerWinLine = "You guys are so busted";
    public float playerLineDuration = 2f;

    [Header("Events")]
    public UnityEvent<int, int> onProgress;
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

    /// <summary>Called automatically by the Player's SpeechBubble when it spawns.</summary>
    public void SetPlayerSpeechBubble(SpeechBubble bubble)
    {
        playerSpeechBubble = bubble;
        Debug.Log("WinManager: Player speech bubble registered.");
    }

    public void RegisterWin(WinConditionObject obj)
    {
        if (hasWon) return;
        if (!registered.Add(obj)) return;

        Debug.Log($"Win objects interacted with: {registered.Count}/{requiredCount}");
        onProgress?.Invoke(registered.Count, requiredCount);

        if (registered.Count >= requiredCount)
        {
            hasWon = true;
            StartCoroutine(WinSequence());
            onWin?.Invoke();
        }
    }

    private IEnumerator WinSequence()
    {
        Debug.Log("You Win!");

        if (playerSpeechBubble != null)
            playerSpeechBubble.Show(playerWinLine, playerLineDuration);
        else
            Debug.LogWarning("WinManager: no player SpeechBubble registered — did the Player spawn correctly?");

        yield return new WaitForSeconds(playerLineDuration);

        foreach (WinConditionObject obj in registered)
        {
            if (obj != null)
                obj.SayWinLine();
        }
    }
}