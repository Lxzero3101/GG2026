using UnityEngine;

/// <summary>
/// Guarantees AudioManager exists before ANY scene's Awake/Start runs — no matter
/// which scene a teammate opens first while testing (Menu, Settings, or their own
/// minigame scene directly in the Editor).
///
/// Requires: the AudioManager prefab must live at Assets/Resources/AudioManager.prefab
/// (exact name, case-sensitive — Resources.Load looks it up by that path).
///
/// No GameObject in any scene needs to reference this — it runs automatically.
/// </summary>
public static class AudioManagerBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureAudioManagerExists()
    {
        if (AudioManager.Instance != null) return;

        AudioManager prefab = Resources.Load<AudioManager>("AudioManager");

        if (prefab == null)
        {
            Debug.LogError("[AudioManagerBootstrap] Missing Assets/Resources/AudioManager.prefab — BGM/SFX will not work in this scene.");
            return;
        }

        Object.Instantiate(prefab);
        // AudioManager.Awake() handles Instance assignment + DontDestroyOnLoad itself.
    }
}
