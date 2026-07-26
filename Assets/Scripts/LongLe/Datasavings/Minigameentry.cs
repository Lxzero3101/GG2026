using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Put this on the CLICKABLE Office prefab (the one that sends the player into
/// a minigame). When clicked, it remembers which minigame is being entered
/// (so the loss/win handlers know who to credit) and loads that minigame scene.
///
/// Uses OnMouseDown, so the prefab needs a Collider2D (any type) and a camera
/// that can see it. No manual wiring beyond assigning the fields in the
/// Inspector — OfficeManager sets miniGameNumber/sceneName at spawn time, but
/// you can also assign them by hand for testing.
///
/// SETUP (prefab):
/// 1. Create the clickable object with a SpriteRenderer + a Collider2D.
/// 2. Attach this script.
/// 3. Leave the fields empty if OfficeManager will fill them; otherwise set
///    Mini Game Number (1..4) and Scene Name.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MiniGameEntry : MonoBehaviour
{
    [Tooltip("Which minigame this entry leads to (1..4). Set by OfficeManager at spawn, or by hand for testing.")]
    [SerializeField] private int miniGameNumber = 1;

    [Tooltip("Exact scene name to load for this minigame (must be in Build Settings).")]
    [SerializeField] private string sceneName = "Minigame1";

    /// <summary>The minigame number this entry represents (1..4).</summary>
    public int MiniGameNumber => miniGameNumber;

    /// <summary>
    /// Called by OfficeManager right after Instantiate so one prefab can serve
    /// any slot. Safe to skip if you hand-configure the prefab per copy.
    /// </summary>
    public void Configure(int number, string scene)
    {
        miniGameNumber = number;
        sceneName = scene;
    }

    private void OnMouseDown()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"[MiniGameEntry] No scene assigned for Minigame {miniGameNumber} — nothing to load.");
            return;
        }

        // Record who's being entered so the win/lose handler in that scene can
        // credit the right minigame without hard-coding a number in each scene.
        MiniGameContext.CurrentMiniGame = miniGameNumber;

        Debug.Log($"[MiniGameEntry] Entering Minigame {miniGameNumber} -> scene '{sceneName}'.");

        // Remove this clickable prefab immediately so it can't be clicked twice
        // (e.g. during a fade) and doesn't linger over the transition. The scene
        // is about to change anyway; on return, OfficeManager re-decides what to
        // spawn at this slot based on GameData.
        Destroy(gameObject);

        SceneManager.LoadScene(sceneName);
    }
}