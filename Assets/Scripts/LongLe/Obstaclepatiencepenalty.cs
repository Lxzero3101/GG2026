using UnityEngine;

/// <summary>
/// Attach to any obstacle GameObject that has a 2D trigger collider. When the
/// player enters the trigger, this notifies GameUI to apply the obstacle-hit
/// patience penalty and boss reaction. This script never touches patience or
/// boss UI directly — it only calls GameUI, per the project's UI architecture.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ObstaclePatiencePenalty : MonoBehaviour
{
    [SerializeField] private GameUI gameUI;
    [SerializeField] private string playerTag = "Player";
    [Tooltip("If true, this obstacle only triggers the penalty once, then disables itself.")]
    [SerializeField] private bool disableAfterHit = true;

    private bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered || !other.CompareTag(playerTag))
        {
            return;
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