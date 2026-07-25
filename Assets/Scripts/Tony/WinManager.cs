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

    [Header("Win Speech Bubbles")]
    public SpeechBubble playerSpeechBubble;

    [TextArea]
    public string playerWinMessage = "You guys are so busted";

    public float playerMessageDuration = 2f;

    [TextArea]
    public string winObjectMessage = "okay";

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
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerSpeechBubble = player.GetComponent<SpeechBubble>();
        }
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
            WinGame();
        }
    }

    private void WinGame()
    {
        Debug.Log("You Win!");
        StartCoroutine(WinSequence());
        onWin?.Invoke();
    }

    private IEnumerator WinSequence()
    {
        if (playerSpeechBubble != null)
            playerSpeechBubble.Show(playerWinMessage, playerMessageDuration);
        else
            Debug.LogWarning("WinManager: no player SpeechBubble assigned/found.");

        yield return new WaitForSeconds(playerMessageDuration);

        foreach (WinConditionObject obj in registered)
        {
            if (obj != null && obj.speechBubble != null)
                obj.speechBubble.Show(winObjectMessage);
        }
    }
}