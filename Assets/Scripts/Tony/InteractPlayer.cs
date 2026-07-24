using UnityEngine;

/// <summary>
/// Attach this to the Player GameObject alongside your movement script.
/// It looks for nearby objects implementing IInteractable and lets the
/// player trigger them by pressing the interact key (default: Z).
///
/// Requirements for objects you want to interact with:
///  - They need a Collider2D (set as "Is Trigger" or not, either works with OverlapCircle)
///  - They need a script attached that implements IInteractable
///  - Optionally put them on a dedicated "Interactable" layer for performance/filtering
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Key used to trigger interaction.")]
    public KeyCode interactKey = KeyCode.Z;

    [Tooltip("How close the player needs to be to interact with an object.")]
    public float interactionRadius = 1f;

    [Tooltip("Optional: restrict detection to a specific layer. Leave as 'Everything' if you don't use layers.")]
    public LayerMask interactableLayer = ~0;

    [Header("Debug")]
    [Tooltip("Draws the interaction radius in the Scene view.")]
    public bool showGizmo = true;

    // The closest interactable currently in range (null if none)
    private IInteractable _currentTarget;

    private void Update()
    {
        _currentTarget = FindClosestInteractable();

        if (_currentTarget != null && Input.GetKeyDown(interactKey))
        {
            _currentTarget.Interact();
        }
    }

    /// <summary>
    /// Finds the closest IInteractable within interactionRadius of the player.
    /// </summary>
    private IInteractable FindClosestInteractable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius, interactableLayer);

        IInteractable closest = null;
        float closestDistSqr = float.MaxValue;

        foreach (var hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable == null)
                continue;

            float distSqr = (hit.transform.position - transform.position).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closest = interactable;
            }
        }

        return closest;
    }

    /// <summary>
    /// Exposes the current interaction prompt (e.g. for a UI popup like "Press Z to open").
    /// Returns null if nothing is in range.
    /// </summary>
    public string GetCurrentPrompt()
    {
        return _currentTarget?.GetInteractionPrompt();
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
