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

    [Header("Scene Transition")]
    [Tooltip("Scene loaded when Minigame 3 is won. For the debt-collector flow this is SubMinigame3, NOT the Office — NoMP is credited only after SubMinigame3 finishes.")]
    public string subMiniGameSceneName = "SubMinigame3";

    [Tooltip("Legacy field, no longer used for the scene load (kept so existing Inspector setups don't break). Safe to ignore.")]
    public string sceneToLoad = "Office";
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
            Debug.LogWarning($"WinManager: duplicate instance found on '{name}', destroying it. Check your scene for multiple GameManagers.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (autoDetectRequiredCount)
        {
            // Wait one frame: RandomSpawner also runs its spawning in Start(),
            // and Unity doesn't guarantee it runs before this Start(). Counting
            // immediately can catch the scene before any win objects exist yet.
            // By next frame, every Start() in the scene (including the spawner's
            // Instantiate calls) has already finished.
            StartCoroutine(AutoDetectNextFrame());
        }
    }

    private IEnumerator AutoDetectNextFrame()
    {
        yield return null;

        requiredCount = FindObjectsByType<WinConditionObject>(FindObjectsSortMode.None).Length;
        Debug.Log($"WinManager auto-detected {requiredCount} win objects.");
    }

    public void RegisterWin(WinConditionObject obj)
    {
        if (hasWon) return;

        if (!registered.Add(obj))
        {
            Debug.LogWarning($"WinManager: '{obj.name}' already registered, ignoring duplicate call.");
            return;
        }

        Debug.Log($"Win objects interacted with: {registered.Count}/{requiredCount} (just registered: {obj.name})");
        onProgress?.Invoke(registered.Count, requiredCount);

        // Explicit equality check, not just >=, so this only fires exactly
        // once, exactly when the count matches requiredCount precisely.
        if (registered.Count == requiredCount && !hasWon)
        {
            hasWon = true;
            Debug.Log("All win objects found — starting win sequence.");
            StartCoroutine(WinSequence());
            onWin?.Invoke();
        }
    }

    private IEnumerator WinSequence()
    {
        Debug.Log("You Win!");
        yield return new WaitForSeconds(delayBeforeSceneLoad);
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        // Minigame 3 is only truly "passed" once SubMinigame3 is ALSO finished,
        // so we do NOT credit NoMP here. Instead we hand off to SubMinigame3;
        // the SubMinigame3 return script calls MiniGameResult.ReportWin(3) when
        // the player heads back to the Office. This way, quitting during
        // SubMinigame3 doesn't wrongly count Minigame 3 as done.
        if (SceneFader.Instance != null)
        {
            SceneFader.LoadScene(subMiniGameSceneName);
        }
        else
        {
            SceneManager.LoadScene(subMiniGameSceneName);
        }
    }
}