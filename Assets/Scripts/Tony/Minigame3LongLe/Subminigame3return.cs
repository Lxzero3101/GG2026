using UnityEngine;

/// <summary>
/// Ends SubMinigame3 by crediting Minigame 3 (+1 NoMP) and routing the player
/// back to the Office (or to Finish if that was the 4th). Minigame 3 is only
/// counted as "passed" HERE — after the SubMinigame3 dialogue finishes — not
/// back in WinManager, so quitting mid-SubMinigame3 never wrongly credits it.
///
/// HOW IT FIRES (two supported ways — you only need ONE):
///
///  1. AUTOMATIC (recommended): if this scene has a DialogueManager, this script
///     finds it on Start and hooks its onDialogueComplete event, so finishing
///     the dialogue automatically returns to the Office. It ALSO switches that
///     DialogueManager off its own scene-loading, so the dialogue won't load a
///     scene directly and skip the NoMP credit. No Inspector wiring needed.
///
///  2. MANUAL: call ReturnToOffice() yourself — from a button OnClick, a trigger,
///     or another script — if you don't use DialogueManager to end the scene.
///
/// SETUP:
/// 1. Put this on an empty GameObject in the SubMinigame3 scene
///    (you named it "SubMinigame3Manager").
/// 2. If SubMinigame3 ends with a DialogueManager, you're done — leave the rest
///    to automatic mode. Otherwise wire a button/trigger to ReturnToOffice().
/// </summary>
public class SubMiniGame3Return : MonoBehaviour
{
    [Tooltip("Optional. The dialogue whose completion returns to the Office. " +
             "Leave empty to auto-find one in the scene.")]
    [SerializeField] private DialogueManager endDialogue;

    [Tooltip("If true, when auto-hooking a DialogueManager this turns OFF that " +
             "dialogue's own 'Load Scene On Complete', so it won't load a scene " +
             "directly and skip crediting Minigame 3. Leave ON unless you know " +
             "you want the dialogue to load its own scene.")]
    [SerializeField] private bool takeOverDialogueSceneLoad = true;

    private bool hasReturned;

    private void Start()
    {
        if (endDialogue == null)
        {
            endDialogue = FindObjectOfType<DialogueManager>();
        }

        if (endDialogue != null)
        {
            // Stop the dialogue from loading a scene on its own — otherwise it
            // would jump to the Office WITHOUT crediting Minigame 3 (the bug).
            if (takeOverDialogueSceneLoad)
            {
                endDialogue.loadSceneOnComplete = false;
            }

            endDialogue.onDialogueComplete.AddListener(ReturnToOffice);
            Debug.Log("[SubMiniGame3Return] Hooked DialogueManager — will credit Minigame 3 and return to Office when the dialogue ends.");
        }
        else
        {
            Debug.Log("[SubMiniGame3Return] No DialogueManager found — waiting for a manual ReturnToOffice() call (e.g. a button).");
        }
    }

    private void OnDestroy()
    {
        if (endDialogue != null)
        {
            endDialogue.onDialogueComplete.RemoveListener(ReturnToOffice);
        }
    }

    /// <summary>
    /// Marks Minigame 3 as passed (+1 NoMP) and sends the player to the Office —
    /// or straight to Finish if this completed all four. Safe to call once; extra
    /// calls are ignored.
    /// </summary>
    public void ReturnToOffice()
    {
        if (hasReturned)
        {
            return;
        }
        hasReturned = true;

        Debug.Log("[SubMiniGame3Return] SubMinigame3 finished — crediting Minigame 3 and returning to Office.");
        MiniGameResult.ReportWin(3);
    }
}