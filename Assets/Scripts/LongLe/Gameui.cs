using System;
using UnityEngine;

/// <summary>
/// Central bridge between gameplay logic and the UI subsystems. Gameplay code
/// (via GameManager, or PowerBarController) should talk exclusively to this class
/// and never reference <see cref="BossPortraitUI"/>, <see cref="PatienceBarUI"/>,
/// or <see cref="BossExpressionController"/> directly. This keeps UI implementation
/// details free to change without breaking gameplay code.
/// </summary>
public class GameUI : MonoBehaviour
{
    /// <summary>
    /// Runtime-only lookup for scripts that can't hold a serialized reference to
    /// this scene object — e.g. components on prefab ASSETS (like obstacles),
    /// which Unity will not let you drag a scene object into via the Inspector.
    /// Those scripts fall back to <see cref="Instance"/> instead.
    /// </summary>
    public static GameUI Instance { get; private set; }

    [SerializeField] private BossPortraitUI bossPortraitUI;
    [SerializeField] private PatienceBarUI patienceBarUI;
    [SerializeField] private BossExpressionController bossExpressionController;

    [Tooltip("Fraction of MAX patience lost when the player hits an obstacle (0.1 = 10%).")]
    [SerializeField, Range(0f, 1f)] private float obstacleHitPercentage = 0.1f;

    /// <summary>Raised once when patience reaches zero — e.g. for GameManager to trigger a loss.</summary>
    public event Action OnPatienceDepleted;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (patienceBarUI != null)
        {
            patienceBarUI.OnPatienceDepleted += HandlePatienceDepleted;
        }
    }

    private void OnDisable()
    {
        if (patienceBarUI != null)
        {
            patienceBarUI.OnPatienceDepleted -= HandlePatienceDepleted;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void HandlePatienceDepleted()
    {
        OnPatienceDepleted?.Invoke();
    }

    /// <summary>
    /// Single entry point for obstacle-hit reactions. Applies the configured
    /// patience penalty and triggers the boss's "next stage" flash + shake.
    /// This is the ONLY GameUI method gameplay/obstacle scripts should call
    /// for this feature — it never exposes PatienceBarUI or BossPortraitUI directly.
    /// </summary>
    public void ApplyObstacleHit()
    {
        if (patienceBarUI == null)
        {
            Debug.LogWarning("[GameUI] PatienceBarUI reference is missing.");
            return;
        }

        // Flash first so it captures the pre-hit stage, then apply the drop.
        bossExpressionController?.FlashNextStage();

        float amount = patienceBarUI.MaxPatience * obstacleHitPercentage;
        patienceBarUI.Decrease(amount);
    }

    /// <summary>
    /// Called by PowerBarController when the tug-of-war indicator leaves (true) or
    /// re-enters (false) its TargetZone. Forwards to the boss expression system
    /// without exposing BossExpressionController to gameplay code directly.
    /// </summary>
    public void SetBossOutOfZoneReaction(bool active)
    {
        bossExpressionController?.SetOutOfZoneOverride(active);
    }

    /// <summary>Sets the boss portrait's current facial expression.</summary>
    public void SetBossExpression(BossExpression expression)
    {
        if (bossPortraitUI == null)
        {
            Debug.LogWarning("[GameUI] BossPortraitUI reference is missing.");
            return;
        }

        bossPortraitUI.SetExpression(expression);
    }

    /// <summary>Increases the patience bar's current value by an absolute amount.</summary>
    public void IncreasePatience(float amount)
    {
        if (patienceBarUI == null)
        {
            Debug.LogWarning("[GameUI] PatienceBarUI reference is missing.");
            return;
        }

        patienceBarUI.Increase(amount);
    }

    /// <summary>Increases patience by a fraction of the maximum (0.2 = +20% of max).</summary>
    public void IncreasePatiencePercent(float normalizedAmount)
    {
        if (patienceBarUI == null)
        {
            Debug.LogWarning("[GameUI] PatienceBarUI reference is missing.");
            return;
        }

        patienceBarUI.Increase(patienceBarUI.MaxPatience * normalizedAmount);
    }

    /// <summary>Decreases the patience bar's current value.</summary>
    public void DecreasePatience(float amount)
    {
        if (patienceBarUI == null)
        {
            Debug.LogWarning("[GameUI] PatienceBarUI reference is missing.");
            return;
        }

        patienceBarUI.Decrease(amount);
    }

    /// <summary>Sets the patience bar's automatic drain speed (units per second).</summary>
    public void SetDrainSpeed(float speed)
    {
        if (patienceBarUI == null)
        {
            Debug.LogWarning("[GameUI] PatienceBarUI reference is missing.");
            return;
        }

        patienceBarUI.SetDecreaseSpeed(speed);
    }

    /// <summary>Pauses the patience bar's automatic drain.</summary>
    public void PausePatience()
    {
        patienceBarUI?.PauseDrain();
    }

    /// <summary>Resumes the patience bar's automatic drain.</summary>
    public void ResumePatience()
    {
        patienceBarUI?.ResumeDrain();
    }

    /// <summary>Resets the patience bar to its maximum value.</summary>
    public void ResetPatience()
    {
        patienceBarUI?.Reset();
    }

    /// <summary>Gets the current patience as a 0–1 fraction, e.g. for gameplay threshold checks.</summary>
    public float GetNormalizedPatience()
    {
        return patienceBarUI != null ? patienceBarUI.NormalizedPatience : 0f;
    }
}