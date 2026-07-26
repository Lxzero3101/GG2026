using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Điều khiển bong bóng chat của Player và Debtor trong minigame kéo co.
///
/// PHIÊN BẢN MỚI (không dùng Canvas World Space):
/// Bong bóng chat giờ là object 3D thuần túy, nằm ngay trong Hierarchy của
/// Player/Debtor, dùng SpriteRenderer (nền bong bóng) + TextMeshPro 3D (chữ
/// thoại) — không còn Canvas, không còn Image, không còn TextMeshProUGUI,
/// không còn auto-fit scale theo Canvas nữa.
///
/// Sau vài giây đầu round, cứ cách một khoảng thời gian ngẫu nhiên, bong bóng
/// của MỘT trong hai người sẽ hiện lên (luân phiên Player -> Debtor -> Player...),
/// không bao giờ hiện cùng lúc.
///
/// CÁCH GẮN VÀO SCENE:
/// 1. Trong Hierarchy, dưới "Player": tạo GameObject con tên "BubbleRoot".
///    - Trong "BubbleRoot", tạo 1 GameObject con có SpriteRenderer (nền bong
///      bóng) và 1 GameObject con khác có component "TextMeshPro" (3D, KHÔNG
///      PHẢI TextMeshProUGUI) để chứa chữ thoại.
///    - Đặt "BubbleRoot" nhô lên trên đầu nhân vật (ví dụ Y = 1.5).
/// 2. Làm tương tự y hệt cho "Debtor".
/// 3. Tạo 1 GameObject rỗng tên "ChatBubbleManager", add script này vào.
/// 4. Kéo PowerBarController vào ô "Power Bar Controller".
/// 5. Kéo "Player/BubbleRoot" vào "Player Bubble Root", kéo SpriteRenderer bên
///    trong vào "Player Bubble Sprite", kéo TextMeshPro (3D) bên trong vào
///    "Player Bubble Text".
/// 6. Kéo "Debtor/BubbleRoot" vào "Debtor Bubble Root" và làm tương tự cho
///    "Debtor Bubble Sprite" / "Debtor Bubble Text".
/// 7. (Tuỳ chọn) Sửa lại các câu thoại mẫu trong Inspector nếu muốn.
///
/// Bong bóng được ẩn bằng SetActive(false)/(true) trên BubbleRoot — không có
/// Canvas nào để bật/tắt, và pop animation chỉ đơn giản là Lerp localScale.
/// </summary>
public class TugOfWarChatBubbleController : MonoBehaviour
{
    [Header("Liên kết Round (để biết khi nào bắt đầu / kết thúc)")]
    [SerializeField] private PowerBarController powerBarController;

    [Header("Pop Animation")]
    [Tooltip("Thời gian animation phóng to khi bong bóng xuất hiện")]
    [SerializeField] private float popDuration = 0.25f;
    [Tooltip("Scale phóng to nhất (overshoot) trước khi settle về 1")]
    [SerializeField] private float popOvershoot = 1.15f;

    [Header("Bong bóng chat - Player (áo xanh)")]
    [SerializeField] private Transform playerBubbleRoot;
    [SerializeField] private SpriteRenderer playerBubbleSprite;
    [SerializeField] private TextMeshPro playerBubbleText;

    [Header("Bong bóng chat - Debtor (áo vàng)")]
    [SerializeField] private Transform debtorBubbleRoot;
    [SerializeField] private SpriteRenderer debtorBubbleSprite;
    [SerializeField] private TextMeshPro debtorBubbleText;

    [Header("Timing")]
    [Tooltip("Chờ bao lâu sau khi round bắt đầu thì mới bắt đầu random bong bóng chat đầu tiên")]
    [SerializeField] private float initialDelayMin = 2f;
    [SerializeField] private float initialDelayMax = 3f;

    [Tooltip("Khoảng thời gian NGẪU NHIÊN giữa 2 lần xuất hiện bong bóng chat")]
    [SerializeField] private float intervalMin = 3f;
    [SerializeField] private float intervalMax = 6f;

    [Tooltip("Bong bóng chat hiện trên màn hình bao lâu trước khi biến mất")]
    [SerializeField] private float bubbleDisplayDuration = 2.5f;

    [Header("Thoại mẫu - Player (người đòi nợ)")]
    [TextArea(1, 2)]
    [SerializeField]
    private string[] playerLines = new string[]
    {
        "Pay up now!",
        "You can't run from this!",
        "I'm not letting go!",
        "Where's my money?!",
        "You've dodged me long enough!",
        "Hand it over!",
        "This ends today!",
        "I know where you live!"
    };

    [Header("Thoại mẫu - Debtor (con nợ)")]
    [TextArea(1, 2)]
    [SerializeField]
    private string[] debtorLines = new string[]
    {
        "Please, just a little more time!",
        "I don't have it right now!",
        "Let go of me!",
        "I swear I'll pay you back!",
        "This isn't fair!",
        "I need that money too!",
        "Stop pulling so hard!",
        "You're breaking my arm!"
    };

    // Scale gốc của mỗi BubbleRoot (đọc từ Inspector lúc Awake), dùng làm scale
    // "đích" khi Pop Animation settle về, và khi Hide/Show không animation.
    private Vector3 playerBaseScale;
    private Vector3 debtorBaseScale;

    // Ai sẽ nói lượt tiếp theo. true = Player nói trước.
    private bool playerTurn = true;

    private Coroutine chatRoutine;
    private Coroutine playerPopRoutine;
    private Coroutine debtorPopRoutine;

    private void Awake()
    {
        // Lưu lại scale gốc TRƯỚC khi ẩn, vì SetActive(false) không đổi scale
        // nhưng ta cần con số này để Pop Animation biết phải phóng to tới đâu.
        if (playerBubbleRoot != null)
            playerBaseScale = playerBubbleRoot.localScale;

        if (debtorBubbleRoot != null)
            debtorBaseScale = debtorBubbleRoot.localScale;

        HideBothBubbles();
    }

    private void OnEnable()
    {
        if (powerBarController != null)
        {
            powerBarController.OnRoundBegan += HandleRoundBegan;
            powerBarController.OnMiniGameWon += HandleRoundEnded;
            powerBarController.OnMiniGameLost += HandleRoundEnded;
        }
        else
        {
            Debug.LogWarning("[ChatBubble] Chưa gán 'Power Bar Controller' trong Inspector -> bong bóng chat sẽ KHÔNG BAO GIỜ chạy vì không nhận được sự kiện OnRoundBegan.");
        }
    }

    private void OnDisable()
    {
        if (powerBarController != null)
        {
            powerBarController.OnRoundBegan -= HandleRoundBegan;
            powerBarController.OnMiniGameWon -= HandleRoundEnded;
            powerBarController.OnMiniGameLost -= HandleRoundEnded;
        }

        // An toàn: dừng hết coroutine khi bị disable giữa chừng.
        StopAllChatCoroutines();
    }

    private void HandleRoundBegan()
    {
        if (chatRoutine != null) StopCoroutine(chatRoutine);
        chatRoutine = StartCoroutine(ChatLoop());
    }

    private void HandleRoundEnded()
    {
        StopAllChatCoroutines();
        HideBothBubbles();
    }

    private void StopAllChatCoroutines()
    {
        if (chatRoutine != null) { StopCoroutine(chatRoutine); chatRoutine = null; }
        if (playerPopRoutine != null) { StopCoroutine(playerPopRoutine); playerPopRoutine = null; }
        if (debtorPopRoutine != null) { StopCoroutine(debtorPopRoutine); debtorPopRoutine = null; }
    }

    /// <summary>
    /// Vòng lặp chính: chờ delay đầu round, sau đó luân phiên hiện bong bóng
    /// Player/Debtor với khoảng nghỉ ngẫu nhiên giữa mỗi lần.
    /// </summary>
    private IEnumerator ChatLoop()
    {
        yield return new WaitForSeconds(Random.Range(initialDelayMin, initialDelayMax));

        while (true)
        {
            ShowBubble(playerTurn);

            yield return new WaitForSeconds(bubbleDisplayDuration);

            HideBubble(playerTurn);

            // Đổi lượt cho người còn lại.
            playerTurn = !playerTurn;

            yield return new WaitForSeconds(Random.Range(intervalMin, intervalMax));
        }
    }

    private void ShowBubble(bool isPlayer)
    {
        Transform root = isPlayer ? playerBubbleRoot : debtorBubbleRoot;
        TextMeshPro text = isPlayer ? playerBubbleText : debtorBubbleText;
        string[] lines = isPlayer ? playerLines : debtorLines;
        string who = isPlayer ? "PLAYER" : "DEBTOR";

        if (root == null || lines.Length == 0)
        {
            Debug.LogWarning($"[ChatBubble] Không hiện được bong bóng {who}: thiếu 'Bubble Root' hoặc mảng thoại rỗng.");
            return;
        }

        string line = lines[Random.Range(0, lines.Length)];
        if (text != null) text.text = line;

        root.gameObject.SetActive(true);

        Vector3 targetScale = isPlayer ? playerBaseScale : debtorBaseScale;

        if (isPlayer)
        {
            if (playerPopRoutine != null) StopCoroutine(playerPopRoutine);
            playerPopRoutine = StartCoroutine(PopRoutine(root, targetScale));
        }
        else
        {
            if (debtorPopRoutine != null) StopCoroutine(debtorPopRoutine);
            debtorPopRoutine = StartCoroutine(PopRoutine(root, targetScale));
        }

        Debug.Log($"[ChatBubble] Hiện bong bóng {who}: {line}");
    }

    private void HideBubble(bool isPlayer)
    {
        Transform root = isPlayer ? playerBubbleRoot : debtorBubbleRoot;
        if (root != null) root.gameObject.SetActive(false);
    }

    private void HideBothBubbles()
    {
        if (playerBubbleRoot != null) playerBubbleRoot.gameObject.SetActive(false);
        if (debtorBubbleRoot != null) debtorBubbleRoot.gameObject.SetActive(false);
    }

    /// <summary>
    /// Animation "pop": phóng to vượt mức (overshoot) rồi settle về đúng scale gốc.
    /// Chạy hoàn toàn bằng Transform.localScale + Vector3.Lerp, không đụng Canvas.
    /// </summary>
    private IEnumerator PopRoutine(Transform bubbleRoot, Vector3 targetScale)
    {
        bubbleRoot.localScale = Vector3.zero;

        // Giai đoạn 1: phóng to vượt mức (overshoot).
        Vector3 overshootScale = targetScale * popOvershoot;
        float phase1Duration = popDuration * 0.6f;
        float elapsed = 0f;
        while (elapsed < phase1Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase1Duration;
            bubbleRoot.localScale = Vector3.Lerp(Vector3.zero, overshootScale, t);
            yield return null;
        }

        // Giai đoạn 2: settle về đúng scale gốc.
        elapsed = 0f;
        float phase2Duration = popDuration * 0.4f;
        while (elapsed < phase2Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase2Duration;
            bubbleRoot.localScale = Vector3.Lerp(overshootScale, targetScale, t);
            yield return null;
        }

        bubbleRoot.localScale = targetScale;
    }
}