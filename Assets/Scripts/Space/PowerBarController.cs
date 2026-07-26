using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the tug-of-war minigame: an indicator drifts under "flood" pressure and
/// the player fights it back by holding Space, trying to keep the indicator inside
/// the TargetZone long enough to accumulate survival time and win.
///
/// This script owns its own win/lose visuals (sprite swaps) but does NOT decide
/// scene flow itself, and does NOT know about GameUI/BossExpressionController at
/// all — it only reports outcomes and TargetZone status via events. A scene-specific
/// orchestrator (e.g. MiniGameFlowController) listens to these and decides what to do.
/// </summary>
public class PowerBarController : MonoBehaviour
{
    [Header("UI Elements")]
    [Header("Target Zone Randomization")]
    [SerializeField] private float zoneRandomInterval = 5f; // Mỗi bao nhiêu giây thì đổi vị trí vùng xanh
    [SerializeField] private float zoneRandomTimer = 0f;
    [SerializeField] private RectTransform barBackground;
    [SerializeField] private RectTransform indicator;
    [SerializeField] private RectTransform targetZone;
    [SerializeField] private Slider survivalSlider;

    [Header("Tug of War GameObjects")]
    [SerializeField] private Transform playerTransform;   // Ông áo xanh
    [SerializeField] private Transform debtorTransform;   // Ông áo vàng
    [SerializeField] private Transform moneyBagTransform; // Túi tiền
    [SerializeField] private float tugSpeed = 1.5f;       // Tốc độ giằng co

    [Header("Screen Boundaries (Giới Hạn Màn Hình)")]
    [SerializeField] private float minScreenX = -6.5f;    // Mép trái màn hình
    [SerializeField] private float maxScreenX = 6.5f;     // Mép phải màn hình

    [Header("Sprites & Victory Visuals")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer; // SpriteRenderer của Player
    [SerializeField] private SpriteRenderer debtorSpriteRenderer; // SpriteRenderer của Con Nợ

    [Space(5)]
    [SerializeField] private Sprite playerVictorySprite;          // Ảnh Player giơ 2 tay ăn mừng
    [SerializeField] private Sprite debtorVictorySprite;          // Ảnh Con Nợ giơ 2 tay ăn mừng (nếu thắng)
    [SerializeField] private Sprite playerFallenSprite;           // Ảnh Player bị ngã gục
    [SerializeField] private Sprite debtorFallenSprite;           // Ảnh Con Nợ bị ngã gục

    [Space(5)]
    [SerializeField] private float moneyBagHeightOffset = 1.2f;   // Độ cao túi tiền bay lên đầu

    [Header("Physics Settings (Dòng Lũ)")]
    [SerializeField] private float floodStrength = 220f;  // Tốc độ dòng lũ kéo trái (vận tốc, giây)
    [SerializeField] private float swimStrength = 20f;    // Khoảng cách CỐ ĐỊNH nhích sang phải mỗi lần BẤM Space
    [SerializeField] private float maxVelocity = 40f;     // Giới hạn tốc độ tối đa của dòng lũ

    [Header("Survival Settings (Thời Gian)")]
    [SerializeField] private float maxSurvivalTime = 10f; // Thời gian cần đạt để THẮNG
    [SerializeField] private float currentSurvivalTime = 0f;
    [SerializeField] private float startGracePeriod = 2f; // Số giây đầu round KHÔNG bị thua

    [Header("Penalty Settings (TụT Thời Gian)")]
    [SerializeField] private float timeDrainRate = 2.5f;  // Tốc độ trừ thời gian/giây khi ở ngoài vùng xanh
    [SerializeField] private float rewardMultiplier = 1f; // Tốc độ cộng thời gian khi ở trong vùng xanh

    [Header("Round End Transition")]
    [Tooltip("How far (and which direction) BarBackground is instantly moved, relative to its start position, when the round ends.")]
    [SerializeField] private Vector2 offScreenOffset = new Vector2(3000f, 0f);

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip winClip;      // Phát khi thắng
    [SerializeField] private AudioClip loseClip;     // Phát khi thua
    [Header("Tug Loop Audio")]
    [SerializeField] private AudioSource tugLoopSource;   // AudioSource riêng để loop
    [SerializeField] private AudioClip tugLoopClip;       // Tiếng kéo dây liên tục

    /// <summary>Raised once when the player reaches <see cref="maxSurvivalTime"/>.</summary>
    public event Action OnMiniGameWon;

    /// <summary>Raised once when the player runs out of survival time or the indicator hits an edge.</summary>
    public event Action OnMiniGameLost;

    /// <summary>Raised whenever the indicator crosses the TargetZone boundary. True = now outside the zone.</summary>
    public event Action<bool> OnTargetZoneStatusChanged;

    private float minX;
    private float maxX;
    private float targetMinX;
    private float targetMaxX;
    private bool isGameOver = false;

    // The round doesn't actually run (indicator doesn't move, no win/lose checks)
    // until GameManager calls BeginRound() once the intro countdown finishes.
    private bool isRoundActive = false;
    private float roundElapsedTime = 0f;

    private bool wasInTargetZone = true;
    private float indicatorVelocity = 0f;
    private Vector2 barBackgroundOriginalPosition;
    private RectTransform survivalSliderRect;
    private Vector2 survivalSliderOriginalPosition;

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

        barBackgroundOriginalPosition = barBackground != null ? barBackground.anchoredPosition : Vector2.zero;

        if (survivalSlider != null)
        {
            survivalSliderRect = survivalSlider.GetComponent<RectTransform>();
            survivalSliderOriginalPosition = survivalSliderRect != null ? survivalSliderRect.anchoredPosition : Vector2.zero;
        }

        wasInTargetZone = CheckIsInTargetZone();
    }

    /// <summary>Called by GameManager once the intro countdown finishes. Until then, the indicator stays put.</summary>
    public void BeginRound()
    {
        isRoundActive = true;
        roundElapsedTime = 0f;
        zoneRandomTimer = 0f;

        if (tugLoopSource != null && tugLoopClip != null)
        {
            tugLoopSource.clip = tugLoopClip;
            tugLoopSource.loop = true;
            tugLoopSource.Play();
        }
    }

    /// <summary>Instantly moves BarBackground and SurvivalSlider off screen. Called by GameManager on win or loss.</summary>
    public void HideBar()
    {
        if (barBackground != null)
        {
            barBackground.anchoredPosition = barBackgroundOriginalPosition + offScreenOffset;
        }

        if (survivalSliderRect != null)
        {
            survivalSliderRect.anchoredPosition = survivalSliderOriginalPosition + offScreenOffset;
        }
    }

    void Update()
    {
        if (!isRoundActive || isGameOver) return;
        roundElapsedTime += Time.deltaTime;
        // --- ĐẾM GIỜ ĐỂ RANDOM VỊ TRÍ VÙNG XANH ---
        zoneRandomTimer += Time.deltaTime;
        if (zoneRandomTimer >= zoneRandomInterval)
        {
            zoneRandomTimer = 0f;
            RandomizeTargetZonePosition();
        }

        // --- A. VẬT LÝ DI CHUYỂN KIM ---
        // Dòng lũ luôn kéo về bên trái theo vận tốc (điều khiển bằng floodStrength)
        indicatorVelocity -= floodStrength * Time.deltaTime;
        indicatorVelocity = Mathf.Clamp(indicatorVelocity, -maxVelocity, 0f); // Dòng lũ chỉ kéo trái, không tự đẩy phải

        MoveIndicator(indicatorVelocity);

        // Mỗi lần BẤM Space, nhích trực tiếp 1 khoảng CỐ ĐỊNH sang phải (tách biệt khỏi vận tốc dòng lũ)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NudgeIndicator(swimStrength);
        }

        // --- B. GIẰNG CO & TÍNH THỜI GIAN ---
        bool isInTargetZone = CheckIsInTargetZone();
        if (tugLoopSource != null && tugLoopSource.isPlaying)
        {
            tugLoopSource.pitch = isInTargetZone ? 1.1f : 0.9f;
        }

        if (isInTargetZone != wasInTargetZone)
        {
            wasInTargetZone = isInTargetZone;
            OnTargetZoneStatusChanged?.Invoke(!isInTargetZone);
        }

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

    void MoveIndicator(float velocity)
    {
        float currentX = indicator.anchoredPosition.x;
        currentX += velocity * Time.deltaTime;
        currentX = Mathf.Clamp(currentX, minX, maxX);
        indicator.anchoredPosition = new Vector2(currentX, indicator.anchoredPosition.y);
    }

    // Nhích kim ngay lập tức 1 khoảng cố định khi bấm Space (không phụ thuộc Time.deltaTime)
    void NudgeIndicator(float distance)
    {
        float currentX = indicator.anchoredPosition.x;
        currentX += distance;
        currentX = Mathf.Clamp(currentX, minX, maxX);
        indicator.anchoredPosition = new Vector2(currentX, indicator.anchoredPosition.y);
    }

    bool CheckIsInTargetZone()
    {
        float indicatorX = indicator.anchoredPosition.x;
        return (indicatorX >= targetMinX && indicatorX <= targetMaxX);
    }
    void RandomizeTargetZonePosition()
    {
        if (targetZone == null) return;

        float targetWidth = targetZone.rect.width;
        float halfWidth = targetWidth / 2f;

        // Random 1 vị trí X mới, đảm bảo vùng xanh không bị lòi ra ngoài 2 vạch đỏ
        float newX = UnityEngine.Random.Range(minX + halfWidth, maxX - halfWidth);

        targetZone.anchoredPosition = new Vector2(newX, targetZone.anchoredPosition.y);

        // Tính lại biên trái/phải của vùng xanh theo vị trí mới
        targetMinX = newX - halfWidth;
        targetMaxX = newX + halfWidth;
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
        // Trong thời gian grace period, không kiểm tra thua
        if (roundElapsedTime < startGracePeriod)
        {
            // 1. THẮNG: Đạt đủ thời gian tích lũy
            if (currentSurvivalTime >= maxSurvivalTime)
            {
                isGameOver = true;
                Debug.Log("BẠN ĐÃ THẮNG MINIGAME!");
                HandleWinVisuals();
                OnMiniGameWon?.Invoke();
            }
            return;
        }

        // 1. THẮNG: Đạt đủ thời gian tích lũy
        if (currentSurvivalTime >= maxSurvivalTime)
        {
            isGameOver = true;
            Debug.Log("BẠN ĐÃ THẮNG MINIGAME!");
            HandleWinVisuals();
            OnMiniGameWon?.Invoke();
            return;
        }

        // 2. THUA: Hết thời gian sống sót HOẶC Chạm vạch đỏ ở 2 đầu
        float indicatorX = indicator.anchoredPosition.x;
        if ((currentSurvivalTime <= 0f && maxSurvivalTime > 0) || indicatorX <= minX || indicatorX >= maxX)
        {
            isGameOver = true;
            Debug.LogError("BẠN ĐÃ THUA MINIGAME!");
            HandleLossVisuals();
            OnMiniGameLost?.Invoke();
        }
    }

    // Hiệu ứng khi NGƯỜI CHƠI THẮNG
    void HandleWinVisuals()
    {
        if (tugLoopSource != null) tugLoopSource.Stop();
        // Phát âm thanh thắng
        if (sfxSource != null && winClip != null)
        {
            sfxSource.PlayOneShot(winClip, 0.7f);
        }

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
        if (tugLoopSource != null) tugLoopSource.Stop();

        // Phát âm thanh thua
        if (sfxSource != null && loseClip != null)
        {
            sfxSource.PlayOneShot(loseClip, 0.7f);
        }

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