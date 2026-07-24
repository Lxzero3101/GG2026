using UnityEngine;
using UnityEngine.UI;

public class PowerBarController : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform barBackground;
    public RectTransform indicator;
    public RectTransform targetZone;   // Vùng xanh lá
    public Slider survivalSlider;

    

    [Header("Physics Settings (Dòng Lũ)")]
    public float floodStrength = 220f;  // Tăng nhẹ lực nước đẩy
    public float swimStrength = 450f;   // Lực bơi của người chơi

    [Header("Survival Settings (Thời Gian)")]
    public float maxSurvivalTime = 10f;
    public float currentSurvivalTime = 0f;

    [Header("Penalty Settings (TụT Thời Gian)")]
    // Tốc độ trừ thời gian MỖI GIÂY khi đứng ngoài vùng xanh lá
    public float timeDrainRate = 2.5f;
    public float rewardMultiplier = 1f;

    private float minX;
    private float maxX;
    private float targetMinX;
    private float targetMaxX;
    private bool isGameOver = false;

    void Start()
    {
        // 1. Tính toán giới hạn thanh nền (Mép Đỏ 2 đầu)
        float barWidth = barBackground.rect.width;
        minX = -barWidth / 2f;
        maxX = barWidth / 2f;

        // 2. Tính toán vị trí Vùng Xanh (Target Zone đã thu nhỏ)
        float targetWidth = targetZone.rect.width;
        float targetX = targetZone.anchoredPosition.x;
        targetMinX = targetX - (targetWidth / 2f);
        targetMaxX = targetX + (targetWidth / 2f);

        indicator.anchoredPosition = new Vector2(0, indicator.anchoredPosition.y);

        if (survivalSlider != null)
        {
            survivalSlider.minValue = 0f;
            survivalSlider.maxValue = maxSurvivalTime;
            survivalSlider.value = currentSurvivalTime;
        }
    }

    void Update()
    {
        if (isGameOver) return;

        // --- A. VẬT LÝ DI CHUYỂN ---
        float currentSpeed = -floodStrength;
        if (Input.GetKey(KeyCode.Space))
        {
            currentSpeed += swimStrength;
        }
        MoveIndicator(currentSpeed);

        // --- B. TRỪ THỜI GIAN LIÊN TỤC KHI RA NGOÀI VÙNG XANH ---
        bool isInTargetZone = CheckIsInTargetZone();

        if (isInTargetZone)
        {
            // Ở trong vùng xanh: Cộng thời gian sống sót
            currentSurvivalTime += Time.deltaTime * rewardMultiplier;
            currentSurvivalTime = Mathf.Min(currentSurvivalTime, maxSurvivalTime);
        }
        else
        {
            // Ra ngoài vùng xanh: Trừ thời gian liên tục theo thời gian thực (Drain Rate)
            currentSurvivalTime -= timeDrainRate * Time.deltaTime;
        }

        // Đảm bảo không âm
        currentSurvivalTime = Mathf.Max(0f, currentSurvivalTime);

        // Cập nhật Slider UI
        if (survivalSlider != null)
        {
            survivalSlider.value = currentSurvivalTime;
        }

        // --- C. KIỂM TRA ĐIỀU KIỆN THẮNG / THUA ---
        CheckWinLossConditions();
    }

    void MoveIndicator(float speed)
    {
        float currentX = indicator.anchoredPosition.x;
        currentX += speed * Time.deltaTime;
        currentX = Mathf.Clamp(currentX, minX, maxX);
        indicator.anchoredPosition = new Vector2(currentX, indicator.anchoredPosition.y);
    }

    bool CheckIsInTargetZone()
    {
        float indicatorX = indicator.anchoredPosition.x;
        return (indicatorX >= targetMinX && indicatorX <= targetMaxX);
    }

    void CheckWinLossConditions()
    {
        // 1. THẮNG: Đạt đủ thời gian
        if (currentSurvivalTime >= maxSurvivalTime)
        {
            isGameOver = true;
            Debug.Log("🎉 BẠN ĐÃ THẮNG MINIGAME!");
        }

        // 2. THUA: Thời gian cạn sạch về 0
        if (currentSurvivalTime <= 0f && maxSurvivalTime > 0)
        {
            isGameOver = true;
            Debug.LogError("💀 THUA! Hết thời gian sống sót do ở ngoài vùng xanh quá lâu!");
        }

        // 3. THUA CHẾT NGAY: Chạm vào vùng đỏ 2 bên mép
        float indicatorX = indicator.anchoredPosition.x;
        if (indicatorX <= minX || indicatorX >= maxX)
        {
            isGameOver = true;
            Debug.LogError("💀 THUA NGAY! Chạm vạch đỏ ở 2 đầu thanh!");
        }
    }
}