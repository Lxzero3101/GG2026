using System.Collections;
using UnityEngine;
using TMPro;

public class SpeechBubble : MonoBehaviour
{
    [Tooltip("The GameObject that gets shown/hidden (bubble background + text container).")]
    public GameObject bubbleRoot;

    [Tooltip("The text component that displays the message.")]
    public TMP_Text bubbleText;

    [Tooltip("How long the bubble stays visible before auto-hiding.")]
    public float displayDuration = 2.5f;

    private Coroutine activeRoutine;

    void Start()
    {
        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
    }

    public void Show(string message)
    {
        Show(message, displayDuration);
    }

    public void Show(string message, float duration)
    {
        if (bubbleRoot == null || bubbleText == null) return;

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