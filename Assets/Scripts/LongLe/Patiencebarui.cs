using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the patience value (current/maximum), its automatic drain over time,
/// and the visual fill amount of its Image. This script owns the patience data
/// and exposes controlled methods for other systems to modify it; it does not
/// know about game rules, only about the bar itself.
/// </summary>
public class PatienceBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private float maxPatience = 100f;
    [SerializeField] private float decreasePerSecond = 5f;
    [SerializeField] private bool drainOnStart = true;

    private float currentPatience;
    private bool isDraining;

    /// <summary>Current patience value, clamped between 0 and <see cref="MaxPatience"/>.</summary>
    public float CurrentPatience => currentPatience;

    /// <summary>The maximum value patience can reach.</summary>
    public float MaxPatience => maxPatience;

    /// <summary>Current patience expressed as a 0–1 fraction of the maximum.</summary>
    public float NormalizedPatience => maxPatience > 0f ? currentPatience / maxPatience : 0f;

    /// <summary>Whether the bar is currently draining automatically over time.</summary>
    public bool IsDraining => isDraining;

    private void Awake()
    {
        currentPatience = maxPatience;
        isDraining = drainOnStart;
        UpdateFill();
    }

    private void Update()
    {
        if (!isDraining)
        {
            return;
        }

        Decrease(decreasePerSecond * Time.deltaTime);
    }

    /// <summary>Resets current patience back to the maximum value.</summary>
    public void Reset()
    {
        currentPatience = maxPatience;
        UpdateFill();
    }

    /// <summary>Increases current patience by <paramref name="amount"/>, clamped to the maximum.</summary>
    /// <param name="amount">Amount to add. Negative values are ignored.</param>
    public void Increase(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        currentPatience = Mathf.Clamp(currentPatience + amount, 0f, maxPatience);
        UpdateFill();
    }

    /// <summary>Decreases current patience by <paramref name="amount"/>, clamped to zero.</summary>
    /// <param name="amount">Amount to subtract. Negative values are ignored.</param>
    public void Decrease(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        currentPatience = Mathf.Clamp(currentPatience - amount, 0f, maxPatience);
        UpdateFill();
    }

    /// <summary>Sets the automatic drain rate, in patience units per second.</summary>
    /// <param name="speed">New drain speed. Negative values are clamped to zero.</param>
    public void SetDecreaseSpeed(float speed)
    {
        decreasePerSecond = Mathf.Max(0f, speed);
    }

    /// <summary>Pauses automatic drain over time. Manual Increase/Decrease calls still work.</summary>
    public void PauseDrain()
    {
        isDraining = false;
    }

    /// <summary>Resumes automatic drain over time.</summary>
    public void ResumeDrain()
    {
        isDraining = true;
    }

    private void UpdateFill()
    {
        if (fillImage == null)
        {
            return;
        }

        fillImage.fillAmount = NormalizedPatience;
    }
}