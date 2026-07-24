using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class PullSystem : MonoBehaviour
{
    [Header("Power Bar References")]
    public RectTransform pointerRect;
    public RectTransform perfectZoneRect;

    [Header("Item & Anchors")]
    public Transform itemTransform;
    public Transform playerAnchor;
    public Transform enemyAnchor;

    [Header("Gameplay Settings")]
    public KeyCode grabKey = KeyCode.Space;
    public int requiredHits = 3;   // number of successful hits to win
    public int maxMisses = 3;      // number of misses to lose
    public float moveStepDuration = 0.25f;

    [Header("UI")]
    public GameObject winPanel;
    public Text resultText;

    [Header("Events (optional hooks)")]
    public UnityEvent onWin;
    public UnityEvent onLose;
    public UnityEvent onHit;
    public UnityEvent onMiss;

    private int currentHits = 0;
    private int currentMisses = 0;
    private float progress = 0f; // -1 = fully at enemy, 0 = start, 1 = fully at player
    private bool gameEnded = false;

    private Vector3 itemStartPos;
    private Coroutine moveRoutine;

    void Start()
    {
        if (itemTransform != null)
            itemStartPos = itemTransform.position;

        if (winPanel != null)
            winPanel.SetActive(false);

        if (resultText != null)
            resultText.text = "";
    }

    void Update()
    {
        if (gameEnded) return;
        if (Input.GetKeyDown(grabKey))
            TryGrab();

    }

    void TryGrab()
    {
        if (pointerRect == null || perfectZoneRect == null) return;

        bool isHit = CheckOverlap();

        if (isHit)
        {
            currentHits++;
            progress += 1f / requiredHits;
            onHit?.Invoke();
        }
        else
        {
            currentMisses++;
            progress -= 1f / maxMisses;
            onMiss?.Invoke();
        }

        progress = Mathf.Clamp(progress, -1f, 1f);
        UpdateItemPosition();

        if (currentHits >= requiredHits)
        {
            EndGame(true);
        }
        else if (currentMisses >= maxMisses)
        {
            EndGame(false);
        }
    }

    bool CheckOverlap()
    {
        float pointerX = pointerRect.anchoredPosition.x;

        float zoneCenter = perfectZoneRect.anchoredPosition.x;
        float zoneHalfWidth = perfectZoneRect.rect.width * 0.5f;

        float zoneMin = zoneCenter - zoneHalfWidth;
        float zoneMax = zoneCenter + zoneHalfWidth;

        return pointerX >= zoneMin && pointerX <= zoneMax;
    }

    void UpdateItemPosition()
    {
        if (itemTransform == null) return;

        Vector3 targetPos;

        if (progress >= 0f)
            targetPos = Vector3.Lerp(itemStartPos, playerAnchor.position, progress);
        else
            targetPos = Vector3.Lerp(itemStartPos, enemyAnchor.position, -progress);

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveItemTo(targetPos));
    }

    IEnumerator MoveItemTo(Vector3 targetPos)
    {
        Vector3 startPos = itemTransform.position;
        float elapsed = 0f;

        while (elapsed < moveStepDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveStepDuration;
            itemTransform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        itemTransform.position = targetPos;
    }

    void EndGame(bool didWin)
    {
        gameEnded = true;

        if (winPanel != null)
            winPanel.SetActive(true);

        if (resultText != null)
            resultText.text = didWin ? "Bạn đã giật được vật phẩm!" : "Kẻ thù đã cướp mất vật phẩm!";

        if (didWin)
            onWin?.Invoke();
        else
            onLose?.Invoke();
    }

    public void ResetGame()
    {
        gameEnded = false;
        currentHits = 0;
        currentMisses = 0;
        progress = 0f;

        if (itemTransform != null)
            itemTransform.position = itemStartPos;

        if (winPanel != null)
            winPanel.SetActive(false);

        if (resultText != null)
            resultText.text = "";
    }
}