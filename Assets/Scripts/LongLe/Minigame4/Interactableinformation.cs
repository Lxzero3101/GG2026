using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Variant of <see cref="InteractableItem"/> that also shows a readable text
/// popup when clicked. While the text is visible, the boss's patience drain
/// is paused (only the automatic drain — if patience was already low, the
/// idle shake / angry expression keeps showing exactly as it was, since we
/// only stop it from getting worse, we don't reset it). Draining resumes the
/// moment the text disappears.
///
/// Everything from the base class (money/attempt bookkeeping, low-value boss
/// warning, collider disable + sorting order bump, zoom/shake/disappear
/// animation) still runs as normal — this just adds the info popup alongside it.
/// </summary>
public class InteractableInformation : InteractableItem
{
    [Header("Info Popup Settings")]
    [Tooltip("Text element used to display the info message. Assign a TextMeshPro text in the scene (e.g. under a Canvas).")]
    [SerializeField] private TMP_Text infoText;

    [TextArea]
    [SerializeField] private string infoMessage = "This item does something interesting!";

    [Tooltip("How long the text stays on screen before disappearing and patience resumes draining.")]
    [SerializeField] private float displayDuration = 3f;

    private void Start()
    {
        // Make sure it starts hidden even if left active in the Inspector.
        if (infoText != null)
        {
            infoText.gameObject.SetActive(false);
        }
    }

    protected override void CollectItem()
    {
        base.CollectItem();

        // IMPORTANT: this item deactivates itself partway through its own
        // zoom/shake animation (see InteractableItem.CollectAnimationRoutine),
        // and Unity kills every coroutine running on a GameObject the instant
        // it's deactivated. Starting InfoDisplayRoutine() on "this" would get
        // it cut off mid-wait — patience would pause and never resume, and the
        // text would never hide. Running it on GameUI (which stays alive for
        // the whole scene) avoids that.
        if (GameUI.Instance != null)
        {
            GameUI.Instance.StartCoroutine(InfoDisplayRoutine());
        }
        else
        {
            Debug.LogWarning("[InteractableInformation] GameUI.Instance is missing — falling back to a local coroutine, which may get cut short by this item's own animation.");
            StartCoroutine(InfoDisplayRoutine());
        }
    }

    private IEnumerator InfoDisplayRoutine()
    {
        GameUI.Instance?.PausePatience();

        if (infoText != null)
        {
            infoText.text = infoMessage;
            infoText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(displayDuration);

        if (infoText != null)
        {
            infoText.gameObject.SetActive(false);
        }

        GameUI.Instance?.ResumePatience();
    }
}