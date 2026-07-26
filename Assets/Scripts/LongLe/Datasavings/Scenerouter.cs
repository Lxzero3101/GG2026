using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Button-friendly scene loader. Put ONE of these anywhere in a scene that has
/// navigation buttons (Menu, Settings, Credit, Lose, Finish...) and wire each
/// button's OnClick to the matching method below. Saves you writing a bespoke
/// script per button.
///
/// The scene names default to your spec's 11 scenes; override any in the
/// Inspector if yours differ. Methods are public and parameterless so they
/// show up cleanly in the Button OnClick dropdown.
///
/// Routing rules from the spec (wire buttons accordingly):
///   Menu    -> Credit, Settings, Office
///   Credit  -> Menu           Settings -> Menu
///   Lose    -> Menu           Finish   -> Menu
///   Office  -> Menu (or a Minigame, handled by MiniGameEntry, not here)
/// </summary>
public class SceneRouter : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string menuScene = "Menu";
    [SerializeField] private string creditScene = "Credit";
    [SerializeField] private string settingsScene = "Settings";
    [SerializeField] private string officeScene = "Office";
    [SerializeField] private string finishScene = "Finish";
    [SerializeField] private string loseScene = "Lose";

    public void GoToMenu() => Load(menuScene);
    public void GoToCredit() => Load(creditScene);
    public void GoToSettings() => Load(settingsScene);
    public void GoToOffice() => Load(officeScene);
    public void GoToFinish() => Load(finishScene);
    public void GoToLose() => Load(loseScene);

    /// <summary>
    /// Load any scene by exact name — for buttons whose destination isn't one
    /// of the presets above. Set the string on a second SceneRouter or via a
    /// UnityEvent if you need it from the Inspector.
    /// </summary>
    public void GoToScene(string sceneName) => Load(sceneName);

    /// <summary>Quits the application (ignored in the editor, works in a build).</summary>
    public void QuitGame()
    {
        Debug.Log("[SceneRouter] Quit requested.");
        Application.Quit();
    }

    private static void Load(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[SceneRouter] Scene name is empty — not loading.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}