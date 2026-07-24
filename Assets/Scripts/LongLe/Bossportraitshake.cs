using System.Collections;
using UnityEngine;

/// <summary>
/// Applies a positional shake effect to the boss portrait's RectTransform.
/// Supports a one-off timed shake and a continuous idle shake toggle. This
/// script is purely a visual effect — it has no knowledge of patience values
/// or expressions, so it can be reused on any UI element later.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class BossPortraitShake : MonoBehaviour
{
    [SerializeField] private float shakeMagnitude = 8f;
    [SerializeField] private float shakeFrequency = 25f;

    private RectTransform rectTransform;
    private Vector2 originalAnchoredPosition;
    private Coroutine oneOffShakeCoroutine;
    private Coroutine idleShakeCoroutine;
    private bool isIdleShaking;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
            originalAnchoredPosition = rectTransform.anchoredPosition;
        }
    }

    /// <summary>Plays a single timed shake for the given duration, then settles back to rest.</summary>
    /// <param name="duration">How long the shake lasts, in seconds.</param>
    public void Shake(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        EnsureInitialized();

        if (oneOffShakeCoroutine != null)
        {
            StopCoroutine(oneOffShakeCoroutine);
        }

        oneOffShakeCoroutine = StartCoroutine(OneOffShakeRoutine(duration));
    }

    /// <summary>Enables or disables a continuous low-intensity idle shake (e.g. while patience is critically low).</summary>
    public void SetIdleShaking(bool active)
    {
        EnsureInitialized();

        if (isIdleShaking == active)
        {
            return;
        }

        isIdleShaking = active;

        if (active)
        {
            if (idleShakeCoroutine == null)
            {
                idleShakeCoroutine = StartCoroutine(IdleShakeRoutine());
            }
        }
        else if (idleShakeCoroutine != null)
        {
            StopCoroutine(idleShakeCoroutine);
            idleShakeCoroutine = null;

            if (oneOffShakeCoroutine == null)
            {
                rectTransform.anchoredPosition = originalAnchoredPosition;
            }
        }
    }

    private IEnumerator OneOffShakeRoutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            ApplyShakeOffset();
            elapsed += Time.deltaTime;
            yield return null;
        }

        oneOffShakeCoroutine = null;

        if (!isIdleShaking)
        {
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }
    }

    private IEnumerator IdleShakeRoutine()
    {
        while (true)
        {
            ApplyShakeOffset();
            yield return null;
        }
    }

    private void ApplyShakeOffset()
    {
        float offsetX = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * 2f * shakeMagnitude;
        float offsetY = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * 2f * shakeMagnitude;
        rectTransform.anchoredPosition = originalAnchoredPosition + new Vector2(offsetX, offsetY);
    }
}