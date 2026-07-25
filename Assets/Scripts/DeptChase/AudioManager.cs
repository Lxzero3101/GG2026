using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central audio bridge — the ONLY class gameplay/UI scripts should call to play
/// sound. Owns a pool of SFX AudioSources (so overlapping sounds like rapid
/// obstacle hits don't cut each other off) plus a single looping BGM source.
/// Lives under DontDestroyOnLoad so it survives scene loads (win/lose transitions).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [Range(0f, 1f)][SerializeField] private float bgmVolume = 0.6f;

    [Header("SFX Pool")]
    [Tooltip("How many SFX can play at the exact same time without cutting each other off.")]
    [SerializeField] private int sfxPoolSize = 6;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    private List<AudioSource> sfxPool;
    private int nextSfxIndex;

    private void Awake()
    {
        // Standard singleton guard — if a second AudioManager sneaks into a
        // freshly loaded scene, destroy the newcomer and keep the original alive.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildSfxPool();

        if (bgmSource != null)
        {
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume;
        }
        else
        {
            Debug.LogWarning("[AudioManager] bgmSource reference is missing!");
        }
    }

    private void OnDestroy()
    {
        // Prevents a stale Instance from lingering when Domain Reload is
        // disabled in Enter Play Mode Settings — without this, the next Play
        // session's calls hit AudioSources that no longer exist.
        if (Instance == this)
            Instance = null;
    }

    private void BuildSfxPool()
    {
        sfxPool = new List<AudioSource>(sfxPoolSize);

        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.volume = sfxVolume;
            sfxPool.Add(src);
        }
    }

    // ─────────────────────────────────────────────
    //  Public API — this is what other scripts call
    // ─────────────────────────────────────────────

    /// <summary>Plays a one-shot sound effect. Safe to call rapidly — pulls the next free pooled AudioSource.</summary>
    /// <summary>Plays a one-shot sound effect. volumeScale (0-1) multiplies the global sfxVolume — use it to make a specific sound quieter without affecting others.</summary>
    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxPool == null || sfxPool.Count == 0) return;

        AudioSource src = sfxPool[nextSfxIndex];
        nextSfxIndex = (nextSfxIndex + 1) % sfxPool.Count;

        if (src == null) return;
        src.PlayOneShot(clip, sfxVolume * volumeScale);
    }
    /// <summary>Starts (or restarts) looping background music. Passing the same clip that's already playing does nothing.</summary>
    public void PlayBgm(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBgm()
    {
        bgmSource?.Stop();
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxPool == null) return;
        foreach (var src in sfxPool)
            src.volume = sfxVolume;
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
            bgmSource.volume = bgmVolume;
    }
}
