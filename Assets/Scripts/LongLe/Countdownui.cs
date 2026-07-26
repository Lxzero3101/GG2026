using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays a countdown at the center of the screen and notifies listeners when
/// it finishes. Purely presentational — has no knowledge of win/lose state or
/// scene flow; <see cref="GameManager"/> decides what happens after it finishes.
/// </summary>
public class CountdownUI : MonoBehaviour
{
    // ---- Added for the minigame polish pass -------------------------------
    // Same pattern as GameUI.Instance / PlayerMovement.Instance: lets scripts
    // living on PREFAB ASSETS (e.g. PlayerMovement on the Player prefab,
    // instantiated at runtime by RandomSpawner) find this scene object without
    // a serialized Inspector reference, which can't be wired on a prefab asset.
    public static CountdownUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    // -------------------------------------------------------------------

    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private int startValue = 5;
    [SerializeField] private float secondsPerCount = 1f;
    [Header("Instruction Text")]
    [Tooltip("Shown only during the countdown, hidden once it finishes.")]
    [SerializeField] private GameObject instructionTextObject;

    [Header("Audio")]
    [SerializeField] private AudioClip tickSfx;
    [SerializeField] private AudioClip goSfx;

    private Coroutine countdownCoroutine;

    /// <summary>Raised once the countdown reaches zero.</summary>
    public event Action OnCountdownFinished;

    /// <summary>Starts (or restarts) the countdown from <see cref="startValue"/>.</summary>
    public void StartCountdown()
    {
        gameObject.SetActive(true);

        if (instructionTextObject != null)
            instructionTextObject.SetActive(true); // ← thêm dòng này

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        countdownCoroutine = StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        int count = startValue;

        while (count > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = count.ToString();
            }

            AudioManager.Instance?.PlaySfx(tickSfx); // ← thêm dòng này

            yield return new WaitForSeconds(secondsPerCount);
            count--;
        }

        if (countdownText != null)
        {
            countdownText.text = string.Empty;
        }

        if (instructionTextObject != null)
            instructionTextObject.SetActive(false); // ← thêm dòng này

        AudioManager.Instance?.PlaySfx(goSfx);

        gameObject.SetActive(false);
        countdownCoroutine = null;
        OnCountdownFinished?.Invoke();
    }
}