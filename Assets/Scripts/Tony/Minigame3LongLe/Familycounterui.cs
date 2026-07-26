using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays "Family member X/Y" progress. Self-subscribes to WinManager's
/// OnProgress(int,int) UnityEvent in code — no manual Inspector event wiring
/// needed (and no risk of accidentally picking the "Static Parameters" bind
/// instead of "Dynamic int, int", which silently locks the numbers at
/// whatever was typed in the Inspector).
///
/// SETUP:
/// 1. Add this script to a UI Text GameObject.
/// 2. Assign "Counter Text" to your TextMeshProUGUI.
/// 3. That's it — do NOT also add a listener on WinManager's On Progress
///    event pointing at this script; it would fire UpdateCount twice per hit.
/// </summary>
public class FamilyCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;

    [Tooltip("Use {0} for the current count and {1} for the required count.")]
    [SerializeField] private string format = "Family member {0}/{1}";

    private void Start()
    {
        // Wait one frame so every other Start() in the scene — including
        // WinManager's own Start(), which auto-detects requiredCount — has
        // definitely finished before we read it or subscribe.
        StartCoroutine(InitializeNextFrame());
    }

    private IEnumerator InitializeNextFrame()
    {
        yield return null;

        if (WinManager.Instance == null)
        {
            Debug.LogWarning("[FamilyCounterUI] No WinManager found in scene — counter will not update.");
            yield break;
        }

        WinManager.Instance.onProgress.AddListener(UpdateCount);
        UpdateCount(0, WinManager.Instance.requiredCount);
    }

    private void OnDestroy()
    {
        if (WinManager.Instance != null)
        {
            WinManager.Instance.onProgress.RemoveListener(UpdateCount);
        }
    }

    /// <summary>Called automatically whenever WinManager registers a new find.</summary>
    public void UpdateCount(int current, int required)
    {
        if (counterText != null)
        {
            counterText.text = string.Format(format, current, required);
        }
    }
}