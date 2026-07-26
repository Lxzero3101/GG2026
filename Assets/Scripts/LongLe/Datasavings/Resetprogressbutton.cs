using UnityEngine;

/// <summary>
/// Wipes all saved player progress (NoMP -> 0) so the whole game can be played
/// again from scratch — including after finishing all 4 minigames. Wire this to
/// the Menu scene's "Reset" button.
///
/// After a reset, entering the Office spawns all 4 clickable entries again (no
/// decorations), because the Office reads GameData fresh on every load.
///
/// NOTE ON THE MENU'S AUTO-RESET: MenuProgressGate already resets progress when
/// the player is on the Menu on a partial/fresh run, but it deliberately
/// PRESERVES a completed 4/4 run (so Finish -> Menu -> Office shows a finished
/// office). This button is how the player breaks out of that finished state and
/// starts over — it resets regardless of whether they're at 4/4.
///
/// SETUP:
/// 1. Attach this script to your "Reset" button GameObject (or any object).
/// 2. On the button's OnClick(), drag in this object and choose
///    ResetProgressButton -> ResetProgress().
/// </summary>
public class ResetProgressButton : MonoBehaviour
{
    [Tooltip("If true, logs a message when progress is reset (handy while testing).")]
    [SerializeField] private bool logOnReset = true;

    /// <summary>
    /// Clears all minigame progress: NoMP goes to 0 and every minigame is marked
    /// not-passed. Hook this to the Reset button's OnClick().
    /// </summary>
    public void ResetProgress()
    {
        GameData.Instance_OrCreate.ResetAll();

        if (logOnReset)
        {
            Debug.Log("[ResetProgressButton] Progress reset — NoMP = 0. The game can be played again from the start.");
        }
    }
}