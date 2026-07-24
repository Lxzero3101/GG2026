using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("The exact name of the scene to load.")]
    [SerializeField] private string _nextSceneName;

    /// <summary>
    /// Call this method from the Button's OnClick() event in the Inspector.
    /// </summary>
    public void LoadNextScene()
    {
        if (string.IsNullOrEmpty(_nextSceneName))
        {
            Debug.LogWarning($"[{nameof(SceneLoader)}] Scene name is empty! Please set a scene name in the Inspector.", this);
            return;
        }

        // Check if the scene is added to Build Settings before trying to load
        if (Application.CanStreamedLevelBeLoaded(_nextSceneName))
        {
            SceneManager.LoadScene(_nextSceneName);
        }
        else
        {
            Debug.LogError($"[{nameof(SceneLoader)}] Cannot load scene '{_nextSceneName}'. " +
                           $"Make sure it is added to your Build Settings (File > Build Profiles / Build Settings).", this);
        }
    }
}