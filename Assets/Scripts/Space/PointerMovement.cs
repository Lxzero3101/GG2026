using UnityEngine;

public class PointerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 200f;

    private RectTransform rectTransform;
    private bool movingRight = true;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (movingRight)
        {
            rectTransform.anchoredPosition += Vector2.right * moveSpeed * Time.deltaTime;
        }
    }
}