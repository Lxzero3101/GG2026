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

    private bool hasReacted;

    private void HandleLossReaction()
    {
        // Guard: the loss event can arrive while a loss is already being
        // processed elsewhere (MiniGameFlowController also listens to it). Run
        // this reaction exactly once to avoid re-entrant event storms.
        if (hasReacted)
        {
            return;
        }
        hasReacted = true;

        // NOTE: We deliberately do NOT call gameUI.DecreasePatience() here.
        // On a patience-depletion loss, patience is already 0, and pushing
        // another patience change re-fires OnPatienceChanged / OnPatienceDepleted
        // WHILE a loss is mid-flight — which re-enters the loss handlers and
        // caused Unity to crash (StackOverflow from the event loop). The boss's
        // furious face + shake below are all the feedback this needs; the
        // patience bar is about to leave with the scene anyway.

        // Đổi biểu cảm sếp sang Furious (Rất giận) và bật Shake
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