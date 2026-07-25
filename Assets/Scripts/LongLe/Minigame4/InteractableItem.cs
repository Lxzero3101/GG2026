using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InteractableItem : MonoBehaviour
{
    [Header("Item Value")]
    [SerializeField] private int moneyValue = 10;

    [Header("Visual Effects Settings")]
    [SerializeField] private SpriteRenderer itemRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color sparkleColor = Color.yellow;
    [SerializeField] private float sparkleSpeed = 5f;

    [Header("Pickup Animation Settings")]
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float scaleMultiplier = 2f;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 0.1f;

    private bool isPlayerNearby = false;
    private bool isCollected = false;
    private Camera mainCamera;
    private Vector3 originalScale;

    public int MoneyValue => moneyValue;

    private void Awake()
    {
        if (itemRenderer == null)
        {
            itemRenderer = GetComponent<SpriteRenderer>();
        }
        mainCamera = Camera.main;
        originalScale = transform.localScale;
    }

    private void Update()
    {
        HandleSparkleEffect();
    }

    private void HandleSparkleEffect()
    {
        if (isPlayerNearby && !isCollected)
        {
            // Simple pulsing sparkle effect between normal and sparkle color
            float lerpVal = (Mathf.Sin(Time.time * sparkleSpeed) + 1f) / 2f;
            itemRenderer.color = Color.Lerp(normalColor, sparkleColor, lerpVal);
        }
        else if (!isCollected)
        {
            itemRenderer.color = normalColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    private void OnMouseDown()
    {
        // Triggers when clicked on via Physics 2D Raycaster / Collider.
        // Frozen during the intro countdown (see PlayerMovement.IsLocked) —
        // covers both the manual raycast in PlayerClickInput and Unity's
        // built-in OnMouseDown message.
        if (PlayerMovement.Instance != null && PlayerMovement.Instance.IsLocked)
        {
            return;
        }

        if (isPlayerNearby && !isCollected)
        {
            CollectItem();
        }
    }

    /// <summary>
    /// Handles the click reaction: money/attempt bookkeeping, the boss's
    /// low-value warning, freeing the item from physical collision so it can
    /// zoom to screen center without shoving the player around, and kicking
    /// off the pickup animation.
    ///
    /// Protected + virtual so variants (e.g. InteractableInformation) can layer
    /// extra behavior on top via <c>base.CollectItem()</c>.
    /// </summary>
    protected virtual void CollectItem()
    {
        isCollected = true;
        itemRenderer.color = normalColor; // Reset color tint

        // Stop this item from physically colliding with the player during the
        // zoom-to-center animation (this was the "clashes with player" issue).
        // Disabling rather than destroying keeps the component intact in case
        // anything else still wants to query it before the object deactivates.
        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }

        // Draw above everything else while it's zooming toward the center.
        if (itemRenderer != null)
        {
            itemRenderer.sortingOrder += 10;
        }

        // Notify GameManager / Counter
        MiniGameManager4.Instance?.ProcessItemClick(moneyValue);

        // Low-value pickups irritate the boss a little (flash + shake) but,
        // unlike an obstacle hit, don't cost any patience.
        if (MiniGameManager4.Instance != null && moneyValue < MiniGameManager4.Instance.LowValueThreshold)
        {
            GameUI.Instance?.FlashBossWarning();
        }

        StartCoroutine(CollectAnimationRoutine());
    }

    private IEnumerator CollectAnimationRoutine()
    {
        Vector3 startPosition = transform.position;
        Vector3 centerScreenWorldPos = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, mainCamera.nearClipPlane + 10f));
        centerScreenWorldPos.z = 0f; // Lock z for 2D

        Vector3 targetScale = originalScale * scaleMultiplier;
        float elapsedTime = 0f;

        // Step 1: Move to center and scale up
        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            transform.position = Vector3.Lerp(startPosition, centerScreenWorldPos, t);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = centerScreenWorldPos;
        transform.localScale = targetScale;

        // Step 2: Shake
        elapsedTime = 0f;
        Vector3 baseShakePos = transform.position;

        while (elapsedTime < shakeDuration)
        {
            Vector3 randomOffset = (Vector3)Random.insideUnitCircle * shakeMagnitude;
            transform.position = baseShakePos + randomOffset;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Step 3: Disappear
        gameObject.SetActive(false);
    }
}