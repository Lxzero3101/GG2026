using System.Collections;
using UnityEngine;

/// <summary>
/// Bridges patience data with the boss portrait's display. Splits the patience
/// range into stages (one per entry in <see cref="stageExpressions"/>) and keeps
/// the portrait's expression in sync via <see cref="PatienceBarUI"/>'s change
/// event — no per-frame polling. Also supports a temporary "flash" preview of
/// the next angrier expression (used on obstacle hits) and a continuous idle
/// shake when patience is critically low.
/// </summary>
public class BossExpressionController : MonoBehaviour
{
    [SerializeField] private BossPortraitUI bossPortraitUI;
    [SerializeField] private PatienceBarUI patienceBarUI;
    [SerializeField] private BossPortraitShake portraitShake;

    [Tooltip("Expression shown for each patience stage, ordered from full patience (index 0) to empty (last index). Stage width = 100% / array length.")]
    [SerializeField]
    private BossExpression[] stageExpressions =
    {
        BossExpression.Neutral,
        BossExpression.Annoyed,
        BossExpression.Disappointed,
        BossExpression.Angry,
        BossExpression.Furious
    };

    [Tooltip("How long a 'next stage' flash preview (e.g. on obstacle hit) is shown before reverting.")]
    [SerializeField] private float flashDuration = 1f;

    [Tooltip("Normalized patience (0-1) at or below which the idle low-patience shake is active.")]
    [SerializeField, Range(0f, 1f)] private float lowPatienceShakeThreshold = 0.2f;

    private int currentStageIndex;
    private Coroutine flashCoroutine;

    /// <summary>The stage index of the currently displayed steady-state expression (0 = calmest).</summary>
    public int CurrentStageIndex => currentStageIndex;

    private void OnEnable()
    {
        if (patienceBarUI == null)
        {
            Debug.LogWarning("[BossExpressionController] PatienceBarUI reference is missing.");
            return;
        }

        patienceBarUI.OnPatienceChanged += HandlePatienceChanged;
        HandlePatienceChanged(patienceBarUI.NormalizedPatience);
    }

    private void OnDisable()
    {
        if (patienceBarUI != null)
        {
            patienceBarUI.OnPatienceChanged -= HandlePatienceChanged;
        }
    }

    private void HandlePatienceChanged(float normalizedValue)
    {
        currentStageIndex = ComputeStageIndex(normalizedValue);

        // Don't stomp on an in-progress flash preview; it will pick up the
        // latest currentStageIndex itself once it finishes.
        if (flashCoroutine == null)
        {
            ApplyExpressionForStage(currentStageIndex);
        }

        portraitShake?.SetIdleShaking(normalizedValue <= lowPatienceShakeThreshold);
    }

    private int ComputeStageIndex(float normalizedValue)
    {
        int stageCount = stageExpressions.Length;
        int index = Mathf.FloorToInt((1f - normalizedValue) * stageCount);
        return Mathf.Clamp(index, 0, stageCount - 1);
    }

    private void ApplyExpressionForStage(int stageIndex)
    {
        if (bossPortraitUI == null || stageExpressions.Length == 0)
        {
            return;
        }

        bossPortraitUI.SetExpression(stageExpressions[stageIndex]);
    }

    /// <summary>
    /// Temporarily previews the next, angrier expression stage (relative to the
    /// stage displayed right now) and triggers a shake. Reverts to whatever the
    /// actual current stage is once <see cref="flashDuration"/> elapses. Call this
    /// BEFORE applying the patience decrease so the preview is based on the
    /// pre-hit stage, as described in the design.
    /// </summary>
    public void FlashNextStage()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRoutine(currentStageIndex));
    }

    private IEnumerator FlashRoutine(int baseStage)
    {
        int flashStage = Mathf.Min(baseStage + 1, stageExpressions.Length - 1);
        ApplyExpressionForStage(flashStage);
        portraitShake?.Shake(flashDuration);

        yield return new WaitForSeconds(flashDuration);

        // currentStageIndex may have advanced further during the wait (e.g. from
        // continued passive drain) — reverting to it, not to baseStage, keeps the
        // portrait honest about the real current value.
        ApplyExpressionForStage(currentStageIndex);
        flashCoroutine = null;
    }
}