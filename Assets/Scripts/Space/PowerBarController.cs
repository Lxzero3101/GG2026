using UnityEngine;
using UnityEngine.UI;

public class PowerBarController : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform barBackground;
    public RectTransform indicator;
    public RectTransform targetZone;
    public Slider survivalSlider;

    [Header("Tug of War GameObjects")]
    public Transform playerTransform;   // Ông áo xanh
    public Transform debtorTransform;   // Ông áo vàng
    public Transform moneyBagTransform; // Túi tiền
    public float tugSpeed = 1.5f;       // Tốc độ giằng co

    [Header("Screen Boundaries (Giới Hạn Màn Hình)")]
    public float minScreenX = -6.5f;    // Mép trái màn hình
    public float maxScreenX = 6.5f;     // Mép phải màn hình

    [Header("Sprites & Victory Visuals")]
    public SpriteRenderer playerSpriteRenderer; // SpriteRenderer của Player
    public SpriteRenderer debtorSpriteRenderer; // SpriteRenderer của Con Nợ

    [Space(5)]
    public Sprite playerVictorySprite;          // Ảnh Player giơ 2 tay ăn mừng
    public Sprite debtorVictorySprite;          // Ảnh Con Nợ giơ 2 tay ăn mừng (nếu thắng)
    public Sprite playerFallenSprite;           // Ảnh Player bị ngã gục
    public Sprite debtorFallenSprite;           // Ảnh Con Nợ bị ngã gục

    [Space(5)]
    public float moneyBagHeightOffset = 1.2f;   // Độ cao túi tiền bay lên đầu

    [Header("Physics Settings (Dòng Lũ)")]
    public float floodStrength = 220f;  // Lực nước đẩy trôi về bên trái
    public float swimStrength = 450f;   // Lực người chơi bấm Space bơi sang phải

    [Header("Survival Settings (Thời Gian)")]
    public float maxSurvivalTime = 10f; // Thời gian cần đạt để THẮNG
    public float currentSurvivalTime = 0f;

    [Header("Penalty Settings (TụT Thời Gian)")]
    public float timeDrainRate = 2.5f;  // Tốc độ trừ thời gian/giây khi ở ngoài vùng xanh
    public float rewardMultiplier = 1f; // Tốc độ cộng thời gian khi ở trong vùng xanh

    private float minX;
    private float maxX;
    private float targetMinX;
    private float targetMaxX;
    private bool isGameOver = false;

    void Start()
    {
        // 1. Tính toán giới hạn của thanh nền (2 Vạch Đỏ ở đầu)
        float barWidth = barBackground.rect.width;
        minX = -barWidth / 2f;
        maxX = barWidth / 2f;

        // 2. Tính toán vị trí Vùng Xanh (Target Zone)
        float targetWidth = targetZone.rect.width;
        float targetX = targetZone.anchoredPosition.x;
        targetMinX = targetX - (targetWidth / 2f);
        targetMaxX = targetX + (targetWidth / 2f);

        // Đặt kim ở vị trí chính giữa ban đầu
        indicator.anchoredPosition = new Vector2(0, indicator.anchoredPosition.y);

        // Thiết lập thanh Slider UI
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

        // --- B. GIẰNG CO & TÍNH THỜI GIAN ---
        bool isInTargetZone = CheckIsInTargetZone();

        if (isInTargetZone)
        {
            // Trong vùng xanh: Cộng thời gian
            currentSurvivalTime += Time.deltaTime * rewardMultiplier;
            currentSurvivalTime = Mathf.Min(currentSurvivalTime, maxSurvivalTime);

            // Kéo cả cụm nhích sang TRÁI (về phía ông áo xanh)
            MoveTugOfWarGroup(Vector3.left);
        }
        else
        {
            // Ra ngoài vùng xanh: Trừ thời gian liên tục
            currentSurvivalTime -= timeDrainRate * Time.deltaTime;

            // Kéo cả cụm nhích sang PHẢI (về phía ông áo vàng)
            MoveTugOfWarGroup(Vector3.right);
        }

        // Đảm bảo thời gian không bị âm
        currentSurvivalTime = Mathf.Max(0f, currentSurvivalTime);

        // Cập nhật giá trị lên Slider
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

    // Hàm di chuyển cả 2 nhân vật và túi tiền (Có kiểm tra giới hạn minScreenX và maxScreenX)
    void MoveTugOfWarGroup(Vector3 direction)
    {
        Vector3 shift = direction * tugSpeed * Time.deltaTime;

        if (playerTransform != null)
        {
            // Tính vị trí X tiếp theo của người chơi
            float nextPlayerX = playerTransform.position.x + shift.x;

            // Chỉ cho phép di chuyển nếu vẫn nằm trong khoảng giới hạn màn hình
            if (nextPlayerX >= minScreenX && nextPlayerX <= maxScreenX)
            {
                playerTransform.position += shift;

                if (debtorTransform != null)
                    debtorTransform.position += shift;

                if (moneyBagTransform != null)
                    moneyBagTransform.position += shift;
            }
        }
    }

    void CheckWinLossConditions()
    {
        // 1. THẮNG: Đạt đủ thời gian tích lũy
        if (currentSurvivalTime >= maxSurvivalTime)
        {
            isGameOver = true;
            Debug.Log("🎉 BẠN ĐÃ THẮNG MINIGAME!");
            HandleWinVisuals();
        }

        // 2. THUA: Hết thời gian sống sót HOẶC Chạm vạch đỏ ở 2 đầu
        float indicatorX = indicator.anchoredPosition.x;
        if ((currentSurvivalTime <= 0f && maxSurvivalTime > 0) || indicatorX <= minX || indicatorX >= maxX)
        {
            isGameOver = true;
            Debug.LogError("💀 BẠN ĐÃ THUA MINIGAME!");
            HandleLossVisuals();
        }
    }

    // Hiệu ứng khi NGƯỜI CHƠI THẮNG
    void HandleWinVisuals()
    {
        // 1. Người chơi giơ 2 tay ăn mừng
        if (playerSpriteRenderer != null && playerVictorySprite != null)
        {
            playerSpriteRenderer.sprite = playerVictorySprite;
        }

        // 2. Con Nợ ngã gục xuống
        if (debtorSpriteRenderer != null && debtorFallenSprite != null)
        {
            debtorSpriteRenderer.sprite = debtorFallenSprite;
        }

        // 3. Túi tiền bay lên đầu Người chơi
        if (moneyBagTransform != null && playerTransform != null)
        {
            moneyBagTransform.position = playerTransform.position + new Vector3(0, moneyBagHeightOffset, 0);
        }
    }

    // Hiệu ứng khi NGƯỜI CHƠI THUA (Con Nợ thắng)
    void HandleLossVisuals()
    {
        // 1. Con Nợ giơ 2 tay ăn mừng
        if (debtorSpriteRenderer != null && debtorVictorySprite != null)
        {
            debtorSpriteRenderer.sprite = debtorVictorySprite;
        }

        // 2. Người chơi ngã gục xuống
        if (playerSpriteRenderer != null && playerFallenSprite != null)
        {
            playerSpriteRenderer.sprite = playerFallenSprite;
        }

        // 3. Túi tiền bay lên đầu Con Nợ
        if (moneyBagTransform != null && debtorTransform != null)
        {
            moneyBagTransform.position = debtorTransform.position + new Vector3(0, moneyBagHeightOffset, 0);
        }
    }
}