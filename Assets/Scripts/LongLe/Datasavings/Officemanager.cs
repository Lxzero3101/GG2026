using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Drives the Office scene for a HAND-PLACED entry layout (Option B).
///
/// You place each clickable MiniGameEntry object in the scene yourself, wherever
/// you want it, with its own Mini Game Number set in the Inspector. On scene
/// load, this manager looks at every pre-placed MiniGameEntry and, for each one
/// whose minigame is already PASSED (per GameData), it:
///   - disables that entry (so it can't be clicked / can't relaunch the game), and
///   - spawns a green-tick decoration at that entry's position.
/// Entries whose minigame is NOT passed are left exactly as you placed them.
///
/// This fixes the "tick spawned on top of a still-clickable entry" bug: the
/// hand-placed entry is the single source of position AND is the thing that gets
/// hidden, so there is never a leftover clickable object behind the tick.
///
/// Behaviour still falls out for free:
///   - Fresh start (NoMP 0): every entry stays clickable, no ticks.
///   - After passing some: those entries become ticks, the rest stay clickable.
///   - After any loss: GameData resets to 0, so on the next Office load every
///     entry is clickable again (the ticks simply aren't spawned).
///   - All 4 passed: every entry becomes a tick, then the finish dialogue plays
///     and Finish loads.
///
/// SETUP:
/// 1. Put this on an empty GameObject in the Office scene.
/// 2. Place your clickable MiniGameEntry objects in the scene (as you already
///    do), each with its Mini Game Number + Scene Name set.
/// 3. Assign the Decoration Prefab (the green tick — no script needed).
/// 4. Optional: assign a DialogueManager for the 4/4 ending. If left empty, a
///    4/4 state loads Finish after finishDelayIfNoDialogue seconds instead.
///
/// NOTE: You no longer need the old "Slots" array or a "Clickable Prefab" — the
/// hand-placed entries replace both. Those fields are gone.
/// </summary>
public class OfficeManager : MonoBehaviour
{
    [Header("Decoration")]
    [Tooltip("Green-tick decoration prefab, spawned at a passed minigame's entry position. Purely visual — needs no script or collider.")]
    [SerializeField] private GameObject decorationPrefab;

    [Tooltip("Optional local offset applied to the spawned tick relative to the entry's position (e.g. nudge it up/forward).")]
    [SerializeField] private Vector3 decorationOffset = Vector3.zero;

    [Header("All-Passed Ending")]
    [Tooltip("Optional. Dialogue played when the player has passed all 4. When it completes, Finish loads.")]
    [SerializeField] private DialogueManager finishDialogue;

    [Tooltip("Exact name of the Finish scene (must be in Build Settings).")]
    [SerializeField] private string finishSceneName = "Finish";

    [Tooltip("If no finishDialogue is assigned, wait this many seconds at 4/4 before loading Finish.")]
    [SerializeField] private float finishDelayIfNoDialogue = 2f;

    private void Start()
    {
        GameData data = GameData.Instance_OrCreate;

        ApplyProgressToEntries(data);

        // All done → play the ending after converting every entry to a tick.
        if (data.AllPassed)
        {
            BeginFinishSequence();
        }
    }

    /// <summary>
    /// Finds every hand-placed MiniGameEntry in the scene and, for each passed
    /// minigame, hides the entry and drops a tick in its place. Unpassed entries
    /// are left untouched.
    /// </summary>
    private void ApplyProgressToEntries(GameData data)
    {
        // Include inactive just in case an entry starts disabled for some reason.
        MiniGameEntry[] entries = FindObjectsByType<MiniGameEntry>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (entries.Length == 0)
        {
            Debug.LogWarning("[OfficeManager] No MiniGameEntry objects found in the Office scene. " +
                             "Place your clickable entries in the scene (Option B setup).");
            return;
        }

        foreach (MiniGameEntry entry in entries)
        {
            if (entry == null)
            {
                continue;
            }

            if (data.IsPassed(entry.MiniGameNumber))
            {
                ConvertEntryToDecoration(entry);
            }
            // else: leave the clickable entry exactly as placed.
        }
    }

    /// <summary>
    /// Hides a passed minigame's clickable entry and spawns a tick where it was.
    /// The entry is deactivated (not destroyed) so nothing else that might hold a
    /// reference to it breaks; either way it can no longer be clicked.
    /// </summary>
    private void ConvertEntryToDecoration(MiniGameEntry entry)
    {
        Vector3 position = entry.transform.position + decorationOffset;
        Quaternion rotation = entry.transform.rotation;

        // Hide the clickable entry so it can't be clicked or relaunch the game.
        entry.gameObject.SetActive(false);

        if (decorationPrefab == null)
        {
            Debug.LogWarning("[OfficeManager] Decoration prefab is not assigned — " +
                             $"Minigame {entry.MiniGameNumber} entry hidden but no tick shown.");
            return;
        }

        GameObject deco = Instantiate(decorationPrefab, position, rotation);

        // SAFETY NET: a decoration must never be able to launch a minigame. If the
        // tick prefab was accidentally built from the clickable prefab, strip any
        // launch behavior so clicking the tick can't send the player back in.
        MiniGameEntry stray = deco.GetComponentInChildren<MiniGameEntry>(true);
        if (stray != null)
        {
            Debug.LogWarning($"[OfficeManager] Decoration for Minigame {entry.MiniGameNumber} " +
                             "had a MiniGameEntry on it — removing it so it can't relaunch the minigame.");

            foreach (Collider2D col in deco.GetComponentsInChildren<Collider2D>(true))
            {
                col.enabled = false;
            }
            Destroy(stray);
        }
    }

    private void BeginFinishSequence()
    {
        if (finishDialogue != null)
        {
            finishDialogue.onDialogueComplete.AddListener(LoadFinish);
            finishDialogue.StartDialogue();
        }
        else
        {
            StartCoroutine(LoadFinishAfterDelay());
        }
    }

    private IEnumerator LoadFinishAfterDelay()
    {
        yield return new WaitForSeconds(finishDelayIfNoDialogue);
        LoadFinish();
    }

    private void LoadFinish()
    {
        if (string.IsNullOrEmpty(finishSceneName))
        {
            Debug.LogWarning("[OfficeManager] Finish scene name is empty.");
            return;
        }

        SceneManager.LoadScene(finishSceneName);
    }
}