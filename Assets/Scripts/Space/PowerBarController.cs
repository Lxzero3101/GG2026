using UnityEngine;
using UnityEngine.UI;

public class PowerBarController : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform barBackground;
    public RectTransform indicator;
    public RectTransform targetZone;
    public Slider survivalSlider;

    [Header("Tug of War GameObjects (Cụm Giằng Co)")]
    public Transform playerTransform;   // Ông áo xanh (Player)
    public Transform debtorTransform;   // Ông áo vàng (Con nợ)
    public Transform moneyBagTransform; // Túi tiền
    public float tugSpeed = 1.5f;       // Tốc độ nhích qua nhích lại của 2 người

    [Header("Physics Settings (Dòng Lũ)")]
    public float floodStrength = 220f;
    public float swimStrength = 450f;

    [Header("Survival Settings (Thời Gian)")]
    public float maxSurvivalTime = 10f;
    public float currentSurvivalTime = 0f;

    [Header("Penalty Settings")]
    public float timeDrainRate = 2.5f;
    public float rewardMultiplier = 1f;

    private float minX;
    private float maxX;
    private float targetMinX;
    private float targetMaxX;
    private bool isGameOver = false;

    void Start()
    {
        float barWidth = barBackground.rect.width;
        minX = -barWidth / 2f;
        maxX = barWidth / 2f;

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

        // --- A. VẬT LÝ DI CHUYỂN KIM ---
        float currentSpeed = -floodStrength;
        if (Input.GetKey(KeyCode.Space))
        {
            currentSpeed += swimStrength;
        }
        MoveIndicator(currentSpeed);

        // --- B. XỬ LÝ TRONG / NGOÀI VÙNG XANH & GIẰNG CO ---
        bool isInTargetZone = CheckIsInTargetZone();

        if (isInTargetZone)
        {
            // Trong vùng xanh: Cộng thời gian
            currentSurvivalTime += Time.deltaTime * rewardMultiplier;
            currentSurvivalTime = Mathf.Min(currentSurvivalTime, maxSurvivalTime);

            // GIẰNG CO: Kéo cả cụm nhích sang TRÁI (về phía ông áo xanh)
            MoveTugOfWarGroup(Vector3.left);
        }
        else
        {
            // Ngoài vùng xanh: Trừ thời gian
            currentSurvivalTime -= timeDrainRate * Time.deltaTime;

            // GIẰNG CO: Kéo cả cụm nhích sang PHẢI (về phía ông áo vàng)
            MoveTugOfWarGroup(Vector3.right);
        }

        currentSurvivalTime = Mathf.Max(0f, currentSurvivalTime);

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

    [Header("Screen Boundaries (Giới Hạn Màn Hình)")]
    public float minScreenX = -7f; // Mép trái màn hình (tùy thuộc vào Camera của bạn)
    public float maxScreenX = 7f;  // Mép phải màn hình

    // Hàm di chuyển cả 2 nhân vật và túi tiền có GIỚI HẠN
    void MoveTugOfWarGroup(Vector3 direction)
    {
        Vector3 shift = direction * tugSpeed * Time.deltaTime;

        // Tính vị trí X mới thử nghiệm của ông áo xanh
        if (playerTransform != null)
        {
            float nextPlayerX = playerTransform.position.x + shift.x;

            // Chỉ cho phép di chuyển nếu còn nằm trong khoảng minScreenX và maxScreenX
            if (nextPlayerX >= minScreenX && nextPlayerX <= maxScreenX)
            {
                playerTransform.position += shift;
                if (debtorTransform != null) debtorTransform.position += shift;
                if (moneyBagTransform != null) moneyBagTransform.position += shift;
            }
        }
    }
    void CheckWinLossConditions()
    {
        if (currentSurvivalTime >= maxSurvivalTime)
        {
            isGameOver = true;
            Debug.Log("🎉 THẮNG MINIGAME!");
        }

        if (currentSurvivalTime <= 0f && maxSurvivalTime > 0)
        {
            isGameOver = true;
            Debug.LogError("💀 THUA! Hết thời gian sống sót!");
        }

        float indicatorX = indicator.anchoredPosition.x;
        if (indicatorX <= minX || indicatorX >= maxX)
        {
            isGameOver = true;
            Debug.LogError("💀 THUA NGAY! Chạm vạch đỏ!");
        }
    }
}