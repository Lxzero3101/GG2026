using UnityEngine;

/// <summary>
/// Attach to any obstacle GameObject that has a 2D trigger collider. When the
/// player enters the trigger, this notifies GameUI to apply the obstacle-hit
/// patience penalty and boss reaction. This script never touches patience or
/// boss UI directly — it only calls GameUI, per the project's UI architecture.
///
/// NOTE: This lives on a PREFAB ASSET, and GameUI lives in the scene. Unity
/// will not let you drag a scene object into a prefab asset's field (you'll
/// see "Type mismatch" in the Inspector if you try) — asset serialization
/// simply can't hold a scene reference. So the gameUI field is optional; if
/// left empty it's resolved automatically at runtime via GameUI.Instance.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ObstaclePatiencePenalty : MonoBehaviour
{
    [Tooltip("Optional — leave empty. Auto-resolved via GameUI.Instance at runtime because prefab assets can't reference scene objects.")]
    [SerializeField] private GameUI gameUI;
    [SerializeField] private string playerTag = "Player";
    [Tooltip("If true, this obstacle only triggers the penalty once, then disables itself.")]
    [SerializeField] private bool disableAfterHit = true;

    private bool hasTriggered;

    private void Awake()
    {
        if (gameUI == null)
        {
            gameUI = GameUI.Instance;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered || !other.CompareTag(playerTag))
        {
            return;
        }

        if (gameUI == null)
        {
            // Covers the case where this obstacle spawned before GameUI.Awake() ran.
            gameUI = GameUI.Instance;
        }

        if (gameUI == null)
        {
            Debug.LogWarning("[ObstaclePatiencePenalty] GameUI reference is missing.");
            return;
        }

        hasTriggered = true;
        gameUI.ApplyObstacleHit();

        if (disableAfterHit)
        {
            gameObject.SetActive(false);
        }
    }
}