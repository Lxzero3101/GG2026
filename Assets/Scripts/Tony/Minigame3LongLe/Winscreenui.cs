using TMPro;
using UnityEngine;

/// <summary>
/// Shows an inspector-adjustable win message in the middle of the screen and
/// freezes the player once the mini-game is won. Self-subscribes to
/// WinManager's OnWin UnityEvent in code — no manual Inspector event wiring
/// needed for THIS script (you still need to separately wire GameManager and
/// GameUI on WinManager's On Win event — see below).
///
/// SETUP:
/// 1. Create a UI Text (TMP) object positioned center-screen. Leave it active
///    in the Hierarchy — this script disables it on Awake.
/// 2. Add this script anywhere in the scene, assign "Win Message Root" and
///    "Win Message Text", and write your message.
/// 3. That's it for this script. Do NOT also add a listener on WinManager's
///    On Win event pointing at ShowWinMessage(); it would fire twice.
///
/// STILL NEEDED IN THE INSPECTOR (these two are gameplay flow, not UI, and
/// stay code-untouched by design — see WinManager's On Win event list):
///   - GameManager.OnPlayerWin()            (pauses patience, shows win button)
///   - GameUI.ImproveBossExpression  → 2     (moves boss expression calmer)
/// </summary>
public class WinScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject winMessageRoot;
    [SerializeField] private TextMeshProUGUI winMessageText;

    [Tooltip("Text shown when the player wins. Adjustable per scene/level.")]
    [TextArea]
    [SerializeField] private string winMessage = "You found everyone!";

    private void Awake()
    {
        if (winMessageRoot != null)
        {
            winMessageRoot.SetActive(false);
        }
    }

    private void Start()
    {
        // Safe to subscribe immediately in Start(): Unity guarantees every
        // object's Awake() (where WinManager.Instance gets set) has already
        // run for the whole scene before any object's Start() runs.
        if (WinManager.Instance == null)
        {
            Debug.LogWarning("[WinScreenUI] No WinManager found in scene — win message will not show.");
            return;
        }

        WinManager.Instance.onWin.AddListener(ShowWinMessage);
    }

    private void OnDestroy()
    {
        if (WinManager.Instance != null)
        {
            WinManager.Instance.onWin.RemoveListener(ShowWinMessage);
        }
    }

    /// <summary>Called automatically when WinManager fires its win event.</summary>
    public void ShowWinMessage()
    {
        if (winMessageText != null)
        {
            winMessageText.text = winMessage;
        }

        if (winMessageRoot != null)
        {
            winMessageRoot.SetActive(true);
        }

        PlayerMovement.Instance?.SetLocked(true);
    }
}