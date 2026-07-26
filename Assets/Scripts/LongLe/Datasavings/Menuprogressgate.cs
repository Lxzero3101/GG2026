using UnityEngine;

/// <summary>
/// Decides what happens to saved progress when the player is on the Menu.
///
/// The rule you asked for:
///   - Normal case: entering the Office from the Menu should be a FRESH run,
///     so progress resets to NoMP = 0.
///   - Exception: if the player has already completed ALL 4 minigames (came
///     back via Finish -> Menu), progress must be PRESERVED so that re-entering
///     the Office shows all 4 decoration prefabs (a finished office).
///
/// So: this resets NoMP to 0 on the Menu UNLESS the player is at 4/4, in which
/// case it leaves everything intact. Because the reset happens here on the Menu
/// (not in the Office), the Office script stays a pure reader of GameData and
/// doesn't need to know any of this policy.
///
/// SETUP:
/// 1. Put an empty GameObject in the Menu scene named "MenuProgressGate".
/// 2. Attach this script. Nothing to configure.
///
/// EDGE CASE THIS HANDLES:
///   Finish -> Menu -> Office : NoMP was 4, stays 4, office shows all ticks. ✔
///   (fresh start) Menu -> Office : NoMP resets to 0, office shows 4 clickable. ✔
///   Lose -> Menu -> Office : LoseSceneReset already zeroed it; this leaves 0. ✔
/// </summary>
public class MenuProgressGate : MonoBehaviour
{
    private void Start()
    {
        GameData data = GameData.Instance_OrCreate;

        if (data.AllPassed)
        {
            // Player finished the whole game and came back to the Menu. Keep
            // their completed progress so the Office reads as fully done.
            Debug.Log("[MenuProgressGate] All 4 minigames complete — preserving progress on Menu.");
            return;
        }

        // Fresh playthrough (or a partial run being abandoned): start clean so
        // entering the Office spawns all 4 clickable entries.
        data.ResetAll();
        Debug.Log("[MenuProgressGate] Menu reached mid-run — progress reset to NoMP = 0.");
    }
}