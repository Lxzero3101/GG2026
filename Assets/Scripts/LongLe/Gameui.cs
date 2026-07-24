using UnityEngine;

/// <summary>
/// Central bridge between gameplay logic and the UI subsystems. Gameplay code
/// (via GameManager) should talk exclusively to this class and never reference
/// <see cref="BossPortraitUI"/> or <see cref="PatienceBarUI"/> directly. This
/// keeps UI implementation details free to change without breaking gameplay code.
/// </summary>
public class GameUI : MonoBehaviour
{
    [SerializeField] private BossPortraitUI bossPortraitUI;
    [SerializeField] private PatienceBarUI patienceBarUI;

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

    /// <summary>Increases the patience bar's current value.</summary>
    public void IncreasePatience(float amount)
    {
        if (patienceBarUI == null)
        {
            Debug.LogWarning("[GameUI] PatienceBarUI reference is missing.");
            return;
        }

        patienceBarUI.Increase(amount);
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