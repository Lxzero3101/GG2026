using TMPro;
using UnityEngine;

/// <summary>
/// Simple HUD readout for the minigame. Shows:
///  - "Goal: X"                — decorative label showing MiniGameManager4's win target.
///  - "Current value: X"       — live, updates every time the player picks up an item.
///  - "Vehicle capacity: X/Y"  — live, X = attempts used so far, Y = max allowed attempts.
///
/// Assign a TMP_Text per line. Leave any field empty to skip that line.
/// </summary>
public class MiniGameCounterUI : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text goalText;
    [SerializeField] private TMP_Text currentValueText;
    [SerializeField] private TMP_Text capacityText;

    [Header("Label Format")]
    [SerializeField] private string goalLabel = "Goal: {0}";
    [SerializeField] private string currentValueLabel = "Current value: {0}";
    [SerializeField] private string capacityLabel = "Vehicle capacity: {0}/{1}";

    private void Start()
    {
        if (MiniGameManager4.Instance != null)
        {
            if (goalText != null)
            {
                goalText.text = string.Format(goalLabel, MiniGameManager4.Instance.TargetMoneyRequirement);
            }

            MiniGameManager4.Instance.OnMoneyChanged += HandleMoneyChanged;
            HandleMoneyChanged(MiniGameManager4.Instance.CurrentTotalMoney);
        }
        else
        {
            Debug.LogWarning("[MiniGameCounterUI] MiniGameManager4.Instance is missing — Goal/Current value/Capacity won't update.");
        }
    }

    private void OnDestroy()
    {
        if (MiniGameManager4.Instance != null)
        {
            MiniGameManager4.Instance.OnMoneyChanged -= HandleMoneyChanged;
        }
    }

    private void HandleMoneyChanged(int newTotal)
    {
        if (currentValueText != null)
        {
            currentValueText.text = string.Format(currentValueLabel, newTotal);
        }

        // Attempts always change alongside money on every click, so this
        // event is a reliable trigger to refresh capacity too.
        if (capacityText != null && MiniGameManager4.Instance != null)
        {
            capacityText.text = string.Format(
                capacityLabel,
                MiniGameManager4.Instance.CurrentAttempts,
                MiniGameManager4.Instance.MaxAllowedAttempts);
        }
    }
}