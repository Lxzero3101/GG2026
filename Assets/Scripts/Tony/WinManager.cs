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
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("WinManager: Scene To Load is empty — set it in the Inspector.");
            return;
        }

        Debug.Log($"WinManager: loading scene '{sceneToLoad}'.");
        SceneManager.LoadScene(sceneToLoad);
    }
}