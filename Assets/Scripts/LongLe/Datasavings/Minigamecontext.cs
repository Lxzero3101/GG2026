using UnityEngine;

/// <summary>
/// Tiny cross-scene note-passing helper. When the player clicks an Office entry,
/// MiniGameEntry writes the chosen minigame number here; the minigame scene's
/// win/lose handler reads it back so it knows which minigame to credit in
/// GameData — without every minigame scene hard-coding its own number.
///
/// It's a plain static (not a MonoBehaviour) so it survives scene loads with
/// zero setup and nothing to place in any scene. Only one value is ever live
/// at a time, which is exactly the case here (you can only be in one minigame).
/// </summary>
public static class MiniGameContext
{
    /// <summary>
    /// The minigame (1..4) the player is currently in. Set by MiniGameEntry on
    /// click; read by MiniGameResult when reporting a win/loss. 0 = none/unknown.
    /// </summary>
    public static int CurrentMiniGame { get; set; }
}