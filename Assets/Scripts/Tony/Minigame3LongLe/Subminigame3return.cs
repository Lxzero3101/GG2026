using UnityEngine;

/// <summary>
/// Call this when the player FINISHES SubMinigame3 and should head back to the
/// Office. This is the moment Minigame 3 is truly "passed", so THIS is where
/// NoMP gets its +1 for minigame 3 — not back in WinManager. Routing to the
/// Office (or to Finish, if this was the 4th minigame) is handled for you.
///
/// TWO WAYS TO USE THIS:
///
/// A) Drop-in: put this script on a GameObject in the SubMinigame3 scene and,
///    wherever SubMinigame3 currently decides "done -> go to Office", call:
///        FindObjectOfType<SubMiniGame3Return>().ReturnToOffice();
///    or wire a UI button's OnClick straight to ReturnToOffice().
///
/// B) One-liner: if you'd rather keep your existing return script, DELETE this
///    file and just replace your "load Office" call with this single line:
///        MiniGameResult.ReportWin(3);
///    That does the exact same thing — credits minigame 3 and loads Office
///    (or Finish at 4/4).
///
/// Either way, do NOT also credit minigame 3 in WinManager — it now only hands
/// off to SubMinigame3, so crediting happens exactly once, right here.
/// </summary>
public class SubMiniGame3Return : MonoBehaviour
{
    /// <summary>
    /// Marks Minigame 3 as passed (+1 NoMP) and sends the player to the Office —
    /// or straight to Finish if this completed all four minigames.
    /// </summary>
    public void ReturnToOffice()
    {
        Debug.Log("[SubMiniGame3Return] SubMinigame3 finished — crediting Minigame 3 and returning to Office.");
        MiniGameResult.ReportWin(3);
    }
}