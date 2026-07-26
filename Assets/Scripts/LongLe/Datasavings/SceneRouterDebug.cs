using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TEMPORARY DIAGNOSTIC version of SceneRouter. Same behaviour, but logs loudly
/// at every step so you can see in the Console EXACTLY where a button click
/// stops working. Once you've found the problem, switch back to the normal
/// SceneRouter.
///
/// TO USE:
/// 1. On your Lose scene's "Main menu button", remove the old SceneRouter
///    component and add this one instead (Add Component -> Scene Router Debug).
/// 2. Re-point the button's OnClick to SceneRouterDebug.GoToMenu.
/// 3. Play to the Lose scene, click the button, and read the Console.
///
/// WHAT THE LOGS TELL YOU:
/// - If you see NO "[SceneRouterDebug] GoToMenu called" at all when you click,
///   the click never reaches the button -> something is covering it (fader
///   overlay, another Canvas) OR the OnClick isn't wired to this component.
/// - If you see "GoToMenu called" then "Loading 'Menu'..." but the scene never
///   changes, look for a RED error right after — almost always "scene not in
///   Build Settings" or a name mismatch.
/// </summary>
public class SceneRouterDebug : MonoBehaviour
{
    [SerializeField] private string menuScene = "Menu";

    public void GoToMenu()
    {
        Debug.Log("[SceneRouterDebug] GoToMenu called — the button IS reaching the script. " +
                  $"About to load '{menuScene}'.");

        // Report whether a fader is intercepting.
        if (SceneFader.Instance != null)
        {
            Debug.Log("[SceneRouterDebug] A SceneFader exists — loading THROUGH the fader.");
        }
        else
        {
            Debug.Log("[SceneRouterDebug] No SceneFader — loading directly.");
        }

        // Check the scene can actually be loaded before trying.
        if (Application.CanStreamedLevelBeLoaded(menuScene))
        {
            Debug.Log($"[SceneRouterDebug] '{menuScene}' IS in Build Settings. Loading now.");
        }
        else
        {
            Debug.LogError($"[SceneRouterDebug] '{menuScene}' is NOT in Build Settings (or the " +
                           "name is misspelled). THIS is why nothing happens. " +
                           "Fix: File > Build Settings > Add Open Scenes, and check the exact name.");
            return;
        }

        if (SceneFader.Instance != null)
            SceneFader.LoadScene(menuScene);
        else
            SceneManager.LoadScene(menuScene);
    }
}