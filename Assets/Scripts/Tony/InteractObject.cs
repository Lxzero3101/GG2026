/// <summary>
/// Implement this interface on any object in the map that the player
/// should be able to interact with (chests, doors, NPCs, pickups, levers, etc.).
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Called when the player presses the interact key while in range of this object.
    /// </summary>
    void Interact();

    /// <summary>
    /// Optional text shown to the player (e.g. "Press Z to open chest").
    /// Return an empty string if you don't want a prompt.
    /// </summary>
    string GetInteractionPrompt();
}
