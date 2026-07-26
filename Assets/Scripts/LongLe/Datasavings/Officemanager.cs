using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Drives the Office scene. There are 4 fixed slots, one per minigame. At each
/// slot this spawns EITHER:
///   - the clickable MiniGameEntry prefab (if that minigame isn't passed yet), OR
///   - the green-tick decoration prefab (if it's already passed).
///
/// Because each slot just asks GameData "is my minigame passed?", the behaviour
/// you described falls out for free:
///   - Fresh start (NoMP 0): all 4 slots clickable.
///   - After passing 3: 3 slots show the tick, 1 stays clickable.
///   - After any loss: GameData resets to 0, so all 4 are clickable again.
///
/// When all 4 are passed (NoMP == 4) it spawns 4 ticks, plays the closing
/// dialogue, and auto-loads the Finish scene when the dialogue ends.
///
/// SETUP:
/// 1. Put this on an empty GameObject in the Office scene.
/// 2. Assign the clickable prefab (with MiniGameEntry) and the decoration
///    (green tick) prefab.
/// 3. Fill the 4 Slots: for each, set the spawn Transform (an empty child
///    placed where you want it), the minigame Number (1..4), and the exact
///    Scene Name to load.
/// 4. Optional: assign a DialogueManager for the 4/4 ending. If left empty,
///    a 4/4 state loads Finish after finishDelayIfNoDialogue seconds instead.
/// </summary>
public class OfficeManager : MonoBehaviour
{
    [System.Serializable]
    public class MiniGameSlot
    {
        [Tooltip("Where this slot's prefab spawns. Use an empty child GameObject placed in the scene.")]
        public Transform spawnPoint;

        [Tooltip("Which minigame this slot represents (1..4).")]
        public int miniGameNumber = 1;

        [Tooltip("Exact scene name this slot's clickable prefab loads (must be in Build Settings).")]
        public string sceneName = "Minigame1";
    }

    [Header("Prefabs")]
    [Tooltip("Clickable prefab (must have a MiniGameEntry component). Spawned for UNPASSED minigames.")]
    [SerializeField] private MiniGameEntry clickablePrefab;

    [Tooltip("Green-tick decoration prefab. Spawned for PASSED minigames. No script required.")]
    [SerializeField] private GameObject decorationPrefab;

    [Header("Slots (one per minigame)")]
    [SerializeField] private MiniGameSlot[] slots = new MiniGameSlot[GameData.TotalMiniGames];

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

        // All done → ending flow instead of spawning entry points.
        if (data.AllPassed)
        {
            SpawnAllAsPassed();
            BeginFinishSequence();
            return;
        }

        SpawnSlots(data);
    }

    private void SpawnSlots(GameData data)
    {
        foreach (MiniGameSlot slot in slots)
        {
            if (slot == null || slot.spawnPoint == null)
            {
                continue;
            }

            if (data.IsPassed(slot.miniGameNumber))
            {
                SpawnDecoration(slot);
            }
            else
            {
                SpawnClickable(slot);
            }
        }
    }

    private void SpawnClickable(MiniGameSlot slot)
    {
        if (clickablePrefab == null)
        {
            Debug.LogWarning("[OfficeManager] Clickable prefab is not assigned.");
            return;
        }

        MiniGameEntry entry = Instantiate(
            clickablePrefab, slot.spawnPoint.position, slot.spawnPoint.rotation);
        entry.Configure(slot.miniGameNumber, slot.sceneName);
    }

    private void SpawnDecoration(MiniGameSlot slot)
    {
        if (decorationPrefab == null)
        {
            Debug.LogWarning("[OfficeManager] Decoration prefab is not assigned.");
            return;
        }

        Instantiate(decorationPrefab, slot.spawnPoint.position, slot.spawnPoint.rotation);
    }

    private void SpawnAllAsPassed()
    {
        foreach (MiniGameSlot slot in slots)
        {
            if (slot != null && slot.spawnPoint != null)
            {
                SpawnDecoration(slot);
            }
        }
    }

    private void BeginFinishSequence()
    {
        if (finishDialogue != null)
        {
            // Load Finish when the dialogue finishes. StartDialogue's own
            // scene-load can also do this, but wiring it here keeps the
            // Finish scene name owned by OfficeManager.
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