using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Temporary test harness for manually exercising <see cref="GameUI"/> via the keyboard.
/// Not part of the final architecture — attach to a test GameObject, delete before
/// wiring up real gameplay/GameManager logic.
/// </summary>
public class UITestController : MonoBehaviour
{
    [SerializeField] private GameUI gameUI;
    [SerializeField] private float testAdjustAmount = 10f;
    [SerializeField] private float testDrainSpeed = 15f;
    [SerializeField, Range(0f, 1f)] private float testIncreasePercentage = 0.2f;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || gameUI == null)
        {
            return;
        }

        // Number keys 1-5 swap boss expression.
        if (keyboard.digit1Key.wasPressedThisFrame) gameUI.SetBossExpression(BossExpression.Neutral);
        if (keyboard.digit2Key.wasPressedThisFrame) gameUI.SetBossExpression(BossExpression.Annoyed);
        if (keyboard.digit3Key.wasPressedThisFrame) gameUI.SetBossExpression(BossExpression.Disappointed);
        if (keyboard.digit4Key.wasPressedThisFrame) gameUI.SetBossExpression(BossExpression.Angry);
        if (keyboard.digit5Key.wasPressedThisFrame) gameUI.SetBossExpression(BossExpression.Furious);

        // Arrow keys adjust patience manually by a flat amount.
        if (keyboard.upArrowKey.wasPressedThisFrame) gameUI.IncreasePatience(testAdjustAmount);
        if (keyboard.downArrowKey.wasPressedThisFrame) gameUI.DecreasePatience(testAdjustAmount);

        // Space pauses drain, G resumes it, R resets, F speeds up drain.
        if (keyboard.spaceKey.wasPressedThisFrame) gameUI.PausePatience();
        if (keyboard.rKey.wasPressedThisFrame) gameUI.ResetPatience();
        if (keyboard.fKey.wasPressedThisFrame) gameUI.SetDrainSpeed(testDrainSpeed);
        if (keyboard.gKey.wasPressedThisFrame) gameUI.ResumePatience();

        // Simulates hitting an obstacle: -10% patience + boss flash/shake reaction.
        if (keyboard.hKey.wasPressedThisFrame) gameUI.ApplyObstacleHit();

        // U key mirrors the test button below, for keyboard-only testing.
        if (keyboard.uKey.wasPressedThisFrame) TestIncreasePatience();
    }

    /// <summary>
    /// Wire this to a UI Button's OnClick() to add a test amount of patience
    /// (default +20% of max) without needing the keyboard.
    /// </summary>
    public void TestIncreasePatience()
    {
        gameUI?.IncreasePatiencePercent(testIncreasePercentage);
    }
}