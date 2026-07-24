using UnityEngine;

public class PointerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 200f;
    [SerializeField] private float leftLimit = -200f;
    [SerializeField] private float rightLimit = 200f;

    private RectTransform rectTransform;
    private bool movingRight = true;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // Di chuyển sang phải
        if (movingRight)
        {
            rectTransform.anchoredPosition += Vector2.right * moveSpeed * Time.deltaTime;

            // Nếu chạm mép phải thì quay đầu
            if (rectTransform.anchoredPosition.x >= rightLimit)
            {
                movingRight = false;
            }
        }
        // Di chuyển sang trái
        else
        {
            rectTransform.anchoredPosition += Vector2.left * moveSpeed * Time.deltaTime;

            // Nếu chạm mép trái thì quay đầu
            if (rectTransform.anchoredPosition.x <= leftLimit)
            {
                movingRight = true;
            }
        }
    }
}