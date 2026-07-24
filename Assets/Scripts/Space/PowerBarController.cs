using UnityEngine;

public class PowerBarController : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform barBackground; // Thanh nền
    public RectTransform indicator;     // Kim chỉ màu vàng/đỏ

    [Header("Settings")]
    public float speed = 500f;          // Tốc độ di chuyển của kim chỉ

    private float minX;
    private float maxX;
    private int direction = 1;          // 1: sang phải, -1: sang trái

    void Start()
    {
        // Tính toán giới hạn trái/phải dựa trên chiều rộng thanh nền
        float barWidth = barBackground.rect.width;
        minX = -barWidth / 2f;
        maxX = barWidth / 2f;
    }

    void Update()
    {
        // Chỉ chạy kim khi người chơi GIỮ phím Space
        if (Input.GetKey(KeyCode.Space))
        {
            MoveIndicator();
        }
    }

    void MoveIndicator()
    {
        // Tính toán vị trí X mới
        float currentX = indicator.anchoredPosition.x;
        currentX += speed * direction * Time.deltaTime;

        // Nếu chạm mép phải -> đổi hướng sang trái
        if (currentX >= maxX)
        {
            currentX = maxX;
            direction = -1;
        }
        // Nếu chạm mép trái -> đổi hướng sang phải
        else if (currentX <= minX)
        {
            currentX = minX;
            direction = 1;
        }

        // Cập nhật lại vị trí kim
        indicator.anchoredPosition = new Vector2(currentX, indicator.anchoredPosition.y);
    }
}