using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton registry of every active InteractableObject in the scene.
/// Put this on an empty GameObject called "InteractableManager" that exists
/// before the player or any interactables call Register() (Script Execution
/// Order handles this automatically since Awake runs before Start).
/// </summary>
public class InteractableManager : MonoBehaviour
{
    public static InteractableManager Instance { get; private set; }

    private readonly List<InteractableObject> activeInteractables = new List<InteractableObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(InteractableObject obj)
    {
        if (!activeInteractables.Contains(obj))
            activeInteractables.Add(obj);
    }

    public void Unregister(InteractableObject obj)
    {
        activeInteractables.Remove(obj);
    }

    /// <summary>
    /// Returns the closest registered interactable within maxRange, or null if none.
    /// </summary>
    public InteractableObject GetNearestInRange(Vector3 position, float maxRange)
    {
        InteractableObject nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var obj in activeInteractables)
        {
            if (obj == null) continue;

            float dist = Vector2.Distance(position, obj.transform.position);
            if (dist <= maxRange && dist < nearestDist)
            {
                nearestDist = dist;
                nearest = obj;
            }
        }

        return nearest;
    }

    /// <summary>Useful for the spawner to check "is this point too close to any object".</summary>
    public IReadOnlyList<InteractableObject> All => activeInteractables;
}
