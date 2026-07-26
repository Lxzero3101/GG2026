using UnityEngine;

/// <summary>
/// Script phụ trách xử lý riêng phản ứng xả Patience Bar và đổi mặt Sếp khi Thua.
/// Gắn script này lên cùng GameObject với MiniGameFlowController.
/// </summary>
public class MiniGameLossHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameUI gameUI;
    [SerializeField] private BossPortraitShake portraitShake;
    [SerializeField] private PowerBarController powerBarController;

    private void Awake()
    {
        // Tự động tìm tham số nếu chưa kéo thả vào Inspector
        if (gameUI == null) gameUI = FindFirstObjectByType<GameUI>();
        if (portraitShake == null) portraitShake = FindFirstObjectByType<BossPortraitShake>();
        if (powerBarController == null) powerBarController = FindFirstObjectByType<PowerBarController>();
    }

    private void OnEnable()
    {
        if (powerBarController != null)
        {
            powerBarController.OnMiniGameLost += HandleLossReaction;
        }
    }

    private void OnDisable()
    {
        if (powerBarController != null)
        {
            powerBarController.OnMiniGameLost -= HandleLossReaction;
        }
    }

    private void HandleLossReaction()
    {
        // 1. Xả sạch cây kiên nhẫn về 0
        if (gameUI != null)
        {
            gameUI.DecreasePatience(gameUI.GetNormalizedPatience() * 100f);
        }

        // 2. Đổi biểu cảm sếp sang Furious (Rất giận) và bật Shake
        if (gameUI != null)
        {
            gameUI.SetBossExpression(BossExpression.Furious);
        }

        if (portraitShake != null)
        {
            portraitShake.SetIdleShaking(true);
        }
    }
}