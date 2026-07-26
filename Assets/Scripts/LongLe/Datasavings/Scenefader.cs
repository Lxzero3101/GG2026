using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent fade-to-black scene transition. Lives across all scenes (one
/// instance, DontDestroyOnLoad) and draws a full-screen black Image on top of
/// everything. Call SceneFader.LoadScene("Name") from anywhere to fade out,
/// swap scenes, then fade back in — no per-scene setup needed.
///
/// It builds its own Canvas + Image in code, so there's nothing to design or
/// wire in the Inspector. The overlay starts transparent and ignores clicks
/// except during a fade.
///
/// SETUP:
/// 1. Put an empty GameObject named "SceneFader" in your FIRST scene (Menu)
///    and attach this script. That's it — it persists from there.
///    (It also self-creates if something calls SceneFader.LoadScene before one
///    exists, so direct-scene testing still fades.)
///
/// HOW TO USE IT with the routing you already have — swap the plain load for a
/// faded one in each place that changes scenes:
///
///   Instead of:  SceneManager.LoadScene(name);
///   Use:         SceneFader.LoadScene(name);
///
/// For the GameData-driven routes, MiniGameResult / SceneRouter can call
/// SceneFader.LoadScene internally (see the note in those files). If you'd
/// rather keep this optional, leaving the plain loads also works — the fader
/// is purely cosmetic.
/// </summary>
public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [Tooltip("Seconds for one fade (out uses this; in uses this too).")]
    [SerializeField] private float fadeDuration = 0.4f;

    [Tooltip("Color faded to between scenes. Black by default.")]
    [SerializeField] private Color fadeColor = Color.black;

    [Tooltip("Sorting order of the fade canvas — high so it covers all other UI.")]
    [SerializeField] private int canvasSortOrder = 9999;

    private CanvasGroup canvasGroup;
    private Image overlayImage;
    private bool isFading;

    /// <summary>Lazily creates the fader if none exists, so any call works.</summary>
    public static SceneFader Instance_OrCreate
    {
        get
        {
            if (Instance == null)
            {
                GameObject go = new GameObject("SceneFader (auto-created)");
                go.AddComponent<SceneFader>();
            }
            return Instance;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlay();

        // Fade in on first appearance so the very first scene isn't a hard cut
        // if something loaded us mid-transition. Starts transparent otherwise.
        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Static entry point: fade out, load <paramref name="sceneName"/>, fade in.
    /// Safe to call from anywhere; self-creates a fader if needed.
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        SceneFader fader = Instance_OrCreate;
        fader.StartFadeLoad(sceneName);
    }

    /// <summary>Instance version of <see cref="LoadScene(string)"/>.</summary>
    public void StartFadeLoad(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[SceneFader] Scene name is empty — not loading.");
            return;
        }

        if (isFading)
        {
            // A fade is already mid-flight; ignore the second request rather
            // than stacking transitions.
            return;
        }

        StartCoroutine(FadeLoadRoutine(sceneName));
    }

    private IEnumerator FadeLoadRoutine(string sceneName)
    {
        isFading = true;

        // Block input during the fade so the player can't double-trigger things.
        canvasGroup.blocksRaycasts = true;

        yield return Fade(0f, 1f);          // to black
        SceneManager.LoadScene(sceneName);  // swap
        yield return null;                  // let the new scene's Awake/Start run one frame
        yield return Fade(1f, 0f);          // back in

        canvasGroup.blocksRaycasts = false;
        isFading = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeDuration <= 0f)
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaled so pauses don't freeze fades
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private void BuildOverlay()
    {
        // Canvas
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = canvasSortOrder;

        // Full-screen black image
        GameObject imageGo = new GameObject("FadeOverlay");
        imageGo.transform.SetParent(transform, false);

        overlayImage = imageGo.AddComponent<Image>();
        overlayImage.color = fadeColor;

        RectTransform rt = overlayImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // CanvasGroup drives alpha + raycast blocking as one unit.
        canvasGroup = imageGo.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}