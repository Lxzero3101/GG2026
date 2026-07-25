using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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

    [Header("Scene Transition")]
    [Tooltip("Exact name of the scene to load after the win sequence (must be added to Build Settings).")]
    public string sceneToLoad = "WinScene";

    [Tooltip("Delay, in seconds, after the win conversation before switching scenes.")]
    public float delayBeforeSceneLoad = 3f;

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

        // Player speaks first
        if (playerSpeechBubble != null)
            playerSpeechBubble.Show(playerWinLine, playerLineDuration);
        else
            Debug.LogWarning("WinManager: no player SpeechBubble registered — did the Player spawn correctly?");

        yield return new WaitForSeconds(playerLineDuration);

        // Then all 4 win objects reply, like a conversation
        foreach (WinConditionObject obj in registered)
        {
            if (obj != null)
                obj.SayWinLine();
        }

        // Wait, then load the next scene
        yield return new WaitForSeconds(delayBeforeSceneLoad);
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("WinManager: Scene To Load is empty — set it in the Inspector.");
            return;
        }

        Debug.Log($"WinManager: loading scene '{sceneToLoad}'.");
        SceneManager.LoadScene(sceneToLoad);
    }
}