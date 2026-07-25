using System.Collections;
using UnityEngine;
using TMPro;

public class SpeechBubble : MonoBehaviour
{
    public GameObject bubbleRoot;
    public TMP_Text bubbleText;
    public float displayDuration = 2.5f;

    [Tooltip("Check this ONLY on the Player's SpeechBubble — it auto-registers with WinManager on spawn.")]
    public bool isPlayerBubble = false;

    private Coroutine activeRoutine;

    void Awake()
    {
        // Register with WinManager the moment this object exists,
        // regardless of when/where it was spawned.
        if (isPlayerBubble && WinManager.Instance != null)
        {
            WinManager.Instance.SetPlayerSpeechBubble(this);
        }
    }

    void Start()
    {
        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
        else
            Debug.LogWarning($"SpeechBubble on '{name}': Bubble Root not assigned.");

        if (bubbleText == null)
            Debug.LogWarning($"SpeechBubble on '{name}': Bubble Text not assigned.");
    }

    public void Show(string message)
    {
        Show(message, displayDuration);
    }

    public void Show(string message, float duration)
    {
        if (bubbleRoot == null || bubbleText == null)
        {
            Debug.LogWarning($"SpeechBubble on '{name}': cannot show, missing Bubble Root or Bubble Text.");
            return;
        }

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(ShowRoutine(message, duration));
    }

    public void Hide()
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
    }

    private IEnumerator ShowRoutine(string message, float duration)
    {
        bubbleText.text = message;
        bubbleRoot.SetActive(true);

        yield return new WaitForSeconds(duration);

        bubbleRoot.SetActive(false);
        activeRoutine = null;
    }
}