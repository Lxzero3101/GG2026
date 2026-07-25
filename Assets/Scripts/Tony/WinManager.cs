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
    [Tooltip("Drag the Player GameObject's SpeechBubble component here directly.")]
    public SpeechBubble playerSpeechBubble;

    [TextArea]
    public string playerWinLine = "You guys are so busted";

    [Tooltip("How long the player's line stays up before the win objects reply.")]
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

        if (playerSpeechBubble == null)
            Debug.LogWarning("WinManager: Player Speech Bubble not assigned in Inspector. Drag the Player object into the field.");
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

        // Player speaks first
        if (playerSpeechBubble != null)
            playerSpeechBubble.Show(playerWinLine, playerLineDuration);

        yield return new WaitForSeconds(playerLineDuration);

        // Then all 4 win objects reply, like a conversation
        foreach (WinConditionObject obj in registered)
        {
            if (obj != null)
                obj.SayWinLine();
        }
    }
}