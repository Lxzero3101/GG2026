using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The one call each minigame makes when it ends. It updates GameData (your
/// NoMP rules) and sends the player to the right scene — so no minigame scene
/// has to know the routing rules or scene names itself.
///
/// It figures out WHICH minigame reported by reading MiniGameContext (set when
/// the player clicked the Office entry). You can also pass an explicit number
/// if you prefer not to rely on that.
///
/// HOW TO CALL IT from your existing minigame code — just one line, added to
/// wherever the win / lose already happens:
///
///   MiniGameResult.ReportWin();     // on win  → +1 NoMP, back to Office (or Finish at 4/4)
///   MiniGameResult.ReportLoss();    // on lose → reset NoMP to 0, go to Lose scene
///
/// For example, in MiniGameFlowController.TriggerWin() add ReportWin() and in
/// TriggerLoss() add ReportLoss() (then let this handle the scene load instead
/// of the hard-coded SceneManager.LoadScene there). Same idea for
/// MiniGameManager / MiniGameManager4 / PowerBar handlers.
///
/// This is a static helper — nothing to place in any scene.
/// </summary>
public static class MiniGameResult
{
    /// <summary>Scene the player returns to after a win (unless all 4 are done).</summary>
    public static string OfficeSceneName = "Office";

    /// <summary>Scene loaded when the player loses.</summary>
    public static string LoseSceneName = "Lose";

    /// <summary>Scene loaded once all 4 minigames are passed.</summary>
    public static string FinishSceneName = "Finish";

    /// <summary>
    /// Report a win. Marks the minigame passed (+1 NoMP) and routes to the
    /// Office — or straight to Finish if that win completed all four.
    /// </summary>
    /// <param name="miniGameNumber">
    /// 1..4, or 0 to auto-detect from the Office click (MiniGameContext).
    /// </param>
    public static void ReportWin(int miniGameNumber = 0)
    {
        int number = Resolve(miniGameNumber);
        GameData data = GameData.Instance_OrCreate;

        data.RegisterWin(number);

        if (data.AllPassed)
        {
            Load(FinishSceneName);
        }
        else
        {
            Load(OfficeSceneName);
        }
    }

    /// <summary>
    /// Report a loss. Resets all progress (NoMP → 0, per the spec) and routes
    /// to the Lose scene.
    /// </summary>
    public static void ReportLoss(int miniGameNumber = 0)
    {
        int number = Resolve(miniGameNumber);
        GameData data = GameData.Instance_OrCreate;

        data.RegisterLoss(number);
        Load(LoseSceneName);
    }

    private static int Resolve(int miniGameNumber)
    {
        if (miniGameNumber >= 1 && miniGameNumber <= GameData.TotalMiniGames)
        {
            return miniGameNumber;
        }

        int fromContext = MiniGameContext.CurrentMiniGame;
        if (fromContext >= 1 && fromContext <= GameData.TotalMiniGames)
        {
            return fromContext;
        }

        Debug.LogWarning("[MiniGameResult] Could not resolve which minigame reported " +
                         "(no explicit number and MiniGameContext is unset). Defaulting to 1. " +
                         "Pass the number explicitly if you launch minigames without the Office click.");
        return 1;
    }

    private static void Load(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[MiniGameResult] Target scene name is empty — not loading.");
            return;
        }

        // Fade between scenes if a SceneFader exists; otherwise load directly.
        // Using the type name in a null-safe way means this file still compiles
        // and runs even if you haven't added SceneFader yet.
        if (SceneFader.Instance != null)
        {
            SceneFader.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}