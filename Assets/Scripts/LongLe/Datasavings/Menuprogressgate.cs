using UnityEngine;

/// <summary>
/// Resets saved progress whenever the player is on the Menu.
///
/// The rule:
///   - Entering the Office from the Menu is ALWAYS a FRESH run, so progress
///     resets to NoMP = 0 every time the Menu is reached — including after the
///     player has finished all 4 minigames. Pressing Play always starts over.
///
/// Because the reset happens here on the Menu (not in the Office), the Office
/// script stays a pure reader of GameData and doesn't need to know any policy.
///
/// SETUP:
/// 1. Put an empty GameObject in the Menu scene named "MenuProgressGate".
/// 2. Attach this script. Nothing to configure.
///
/// EDGE CASES:
///   Finish -> Menu -> Office : NoMP resets to 0, office shows 4 clickable. ✔
///   (fresh start) Menu -> Office : NoMP resets to 0, office shows 4 clickable. ✔
///   Lose -> Menu -> Office : already zeroed; this leaves 0. ✔
/// </summary>
public class MenuProgressGate : MonoBehaviour
{
    private void Start()
    {
        GameData data = GameData.Instance_OrCreate;

        // Always start clean when the Menu is reached, so entering the Office
        // spawns all 4 clickable entries — even after a completed 4/4 run.
        data.ResetAll();
        Debug.Log("[MenuProgressGate] Menu reached — progress reset to NoMP = 0.");
    }
}