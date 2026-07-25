using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class InteractableItem : MonoBehaviour
{
    [Header("Item Value")]
    [SerializeField] private int moneyValue = 10;

    [Header("Interaction Settings")]
    [Tooltip("Key the player presses to interact while standing near this item (proximity-based, replaces mouse click).")]
    [SerializeField] private Key interactKey = Key.F;

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

    // Shared across ALL InteractableItem instances. If the player is standing
    // inside two overlapping trigger zones, both items' Update() would otherwise
    // react to the same single keypress in the same frame and both get collected
    // at once. This lets only the first item to process the press in a given
    // frame claim it; every other item sees the frame already claimed and skips.
    private static int lastInteractFrame = -1;

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
        HandleInteractInput();
    }

    /// <summary>
    /// Proximity + key-press interaction, replacing the old click-based flow.
    /// Requires the player to be inside the trigger (<see cref="isPlayerNearby"/>)
    /// AND press <see cref="interactKey"/>. Also respects PlayerMovement.IsLocked,
    /// so this is automatically disabled during the intro countdown and after the
    /// round ends (win/lose freeze), same as before.
    /// </summary>
    private void HandleInteractInput()
    {
        if (isCollected || !isPlayerNearby)
        {
            return;
        }

        if (PlayerMovement.Instance != null && PlayerMovement.Instance.IsLocked)
        {
            return;
        }

        if (Keyboard.current == null || !Keyboard.current[interactKey].wasPressedThisFrame)
        {
            return;
        }

        if (lastInteractFrame == Time.frameCount)
        {
            // Another overlapping item already claimed this keypress this frame.
            return;
        }

        lastInteractFrame = Time.frameCount;
        CollectItem();
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

    // Click-based collection (OnMouseDown) removed — interaction is now
    // proximity + key press, handled by HandleInteractInput() above.
    // PlayerClickInput.cs is now unused and can be removed from the player object.

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