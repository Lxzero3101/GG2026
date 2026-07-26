using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent player-progress store. Survives scene loads (DontDestroyOnLoad)
/// and app restarts (PlayerPrefs). This is the single source of truth for
/// "which minigames has the player passed" — every other system reads it and
/// nobody duplicates the state.
///
/// The design keeps your NoMP rule from the spec ("+1 on win, reset to 0 on
/// lose") but, because minigames can be done in ANY order, it also records
/// WHICH minigames are done — so the Office spawner knows which specific
/// prefab to stop showing. NoMP is then simply "how many are marked done",
/// derived from that set, so the two can never disagree.
///
/// SETUP:
/// 1. Create an empty GameObject in your FIRST-loaded scene (Menu) named
///    "GameData" and attach this script. It persists itself from there on.
///    (Or just call GameData.Instance from anywhere — it self-creates.)
/// 2. There must be exactly ONE. The Awake guard destroys duplicates so you
///    can safely leave a copy in Menu without worrying about re-entry.
/// </summary>
public class GameData : MonoBehaviour
{
    public const int TotalMiniGames = 4;

    private const string PlayerPrefsKey = "PayOrDecay_PassedMask";

    public static GameData Instance { get; private set; }

    // Bitmask of passed minigames. Bit 0 = Minigame 1 ... bit 3 = Minigame 4.
    private int passedMask;

    /// <summary>
    /// Number of Minigames Passed (your "NoMP", 0..4). Derived from the passed
    /// set so it can never drift out of sync with which ones are actually done.
    /// </summary>
    public int NoMP
    {
        get
        {
            int count = 0;
            for (int i = 0; i < TotalMiniGames; i++)
            {
                if (IsPassed(i + 1)) count++;
            }
            return count;
        }
    }

    /// <summary>True once every minigame is passed (NoMP == 4).</summary>
    public bool AllPassed => NoMP >= TotalMiniGames;

    /// <summary>
    /// Lazily creates the singleton if it doesn't exist yet, so a minigame
    /// scene loaded directly (e.g. from the editor) still works during testing.
    /// </summary>
    public static GameData Instance_OrCreate
    {
        get
        {
            if (Instance == null)
            {
                GameObject go = new GameObject("GameData (auto-created)");
                go.AddComponent<GameData>();
            }
            return Instance;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // A GameData already persists from an earlier scene — kill the copy.
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    /// <summary>
    /// Marks the given minigame (1..4) as passed. This is your "+1 to NoMP":
    /// NoMP goes up because one more bit is now set. Idempotent — re-passing
    /// an already-passed minigame changes nothing.
    /// </summary>
    public void RegisterWin(int miniGameNumber)
    {
        if (!IsValid(miniGameNumber))
        {
            Debug.LogWarning($"[GameData] RegisterWin ignored — invalid minigame number {miniGameNumber}.");
            return;
        }

        passedMask |= (1 << (miniGameNumber - 1));
        Save();
        Debug.Log($"[GameData] Win registered for Minigame {miniGameNumber}. NoMP = {NoMP}/{TotalMiniGames}.");
    }

    /// <summary>
    /// Your "reset to 0 on lose": wipes ALL progress. The player starts the
    /// whole set over. (miniGameNumber is accepted only for symmetry/logging.)
    /// </summary>
    public void RegisterLoss(int miniGameNumber = 0)
    {
        passedMask = 0;
        Save();
        Debug.Log($"[GameData] Loss registered (Minigame {miniGameNumber}). Progress reset. NoMP = 0.");
    }

    /// <summary>True if the given minigame number (1..4) has been passed.</summary>
    public bool IsPassed(int miniGameNumber)
    {
        if (!IsValid(miniGameNumber)) return false;
        return (passedMask & (1 << (miniGameNumber - 1))) != 0;
    }

    /// <summary>
    /// The minigame numbers (1..4) NOT yet passed, in ascending order. The
    /// Office spawner uses this to decide which prefabs to place — its count
    /// is exactly (4 - NoMP), matching your spec.
    /// </summary>
    public List<int> GetUnpassedMiniGames()
    {
        List<int> result = new List<int>();
        for (int i = 1; i <= TotalMiniGames; i++)
        {
            if (!IsPassed(i)) result.Add(i);
        }
        return result;
    }

    /// <summary>Wipes all progress (e.g. a "New Game" button on the menu).</summary>
    public void ResetAll()
    {
        passedMask = 0;
        Save();
    }

    private static bool IsValid(int miniGameNumber)
        => miniGameNumber >= 1 && miniGameNumber <= TotalMiniGames;

    private void Save()
    {
        PlayerPrefs.SetInt(PlayerPrefsKey, passedMask);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        passedMask = PlayerPrefs.GetInt(PlayerPrefsKey, 0);
    }
}