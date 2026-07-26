using UnityEngine;

/// <summary>
/// Belt-and-suspenders progress wipe for the Lose scene. The moment this scene
/// loads, ALL minigame progress is reset (NoMP -> 0), regardless of how the
/// player got here. MiniGameResult.ReportLoss() already resets before loading
/// Lose, but putting this here means the Lose scene ITSELF is the source of
/// truth: if the player ever reaches Lose by any other route (a menu button, a
/// direct scene load during testing, a future code path), progress is still
/// wiped. Resetting twice is harmless because the reset is idempotent.
///
/// Story reason: you're a debt collector who only wins the big game by
/// collecting from every debtor (passing all 4 minigames). Losing any single
/// collection means starting the whole job over — so a loss wipes everything.
///
/// SETUP:
/// 1. Put an empty GameObject in the Lose scene named "LoseReset".
/// 2. Attach this script. Nothing to configure.
/// </summary>
public class LoseSceneReset : MonoBehaviour
{
    private void Awake()
    {
        // Awake (not Start) so the wipe happens as early as possible — before
        // any other Lose-scene script's Start() could read NoMP.
        GameData.Instance_OrCreate.ResetAll();
        Debug.Log("[LoseSceneReset] Reached Lose scene — all progress wiped, NoMP = 0.");
    }
}