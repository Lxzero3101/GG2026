using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Điều khiển bong bóng chat của Player và Debtor trong minigame kéo co.
/// Sau vài giây đầu round, cứ cách 1 khoảng thời gian ngẫu nhiên, sẽ hiện bong bóng
/// chat của MỘT trong hai người (luân phiên) — không bao giờ hiện cùng lúc.
/// Sau khi bong bóng của người này biến mất, tới lượt bong bóng của người kia.
///
/// CÁCH GẮN VÀO SCENE:
/// 1. Chọn GameObject "Player" trong Hierarchy (nhân vật áo xanh).
///    - Chuột phải trên Player -> UI -> Canvas. Unity sẽ tạo 1 Canvas con.
///    - Đổi Render Mode của Canvas đó thành "World Space".
///    - Kéo thu nhỏ Scale của Canvas đó xuống khoảng (0.01, 0.01, 0.01) để vừa kích thước.
///    - Đặt Position của Canvas nhô lên trên đầu nhân vật (ví dụ Y = 1.5).
///    - Đổi tên Canvas đó thành "PlayerChatBubble".
///    - Bên trong Canvas, thêm 1 Image (làm nền bong bóng) + 1 TextMeshPro - Text (UI) (chữ thoại).
///    - Tắt (uncheck) GameObject "PlayerChatBubble" để mặc định ẩn.
/// 2. Làm tương tự y hệt cho "Debtor" (nhân vật áo vàng) -> đặt tên "DebtorChatBubble".
/// 3. Tạo 1 GameObject rỗng tên "ChatBubbleManager", add script này vào.
/// 4. Kéo PowerBarController vào ô "Power Bar Controller".
/// 5. Kéo "PlayerChatBubble" vào ô "Player Bubble Object", kéo Text bên trong nó vào "Player Bubble Text".
/// 6. Kéo "DebtorChatBubble" vào ô "Debtor Bubble Object", kéo Text bên trong nó vào "Debtor Bubble Text".
/// 7. (Tuỳ chọn) Sửa lại các câu thoại mẫu trong Inspector nếu muốn.
/// </summary>
public class TugOfWarChatBubbleController : MonoBehaviour
{
    [Header("Liên kết Round (để biết khi nào bắt đầu / kết thúc)")]
    [SerializeField] private PowerBarController powerBarController;

    [Header("Bong bóng chat - Player (áo xanh)")]
    [SerializeField] private GameObject playerBubbleObject;
    [SerializeField] private TMP_Text playerBubbleText;
    [Tooltip("SpriteRenderer của Player, dùng để TỰ TÍNH vị trí bong bóng ngay trên đầu (không cần tự đoán số Y)")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

    [Header("Bong bóng chat - Debtor (áo vàng)")]
    [SerializeField] private GameObject debtorBubbleObject;
    [SerializeField] private TMP_Text debtorBubbleText;
    [Tooltip("SpriteRenderer của Debtor, dùng để TỰ TÍNH vị trí bong bóng ngay trên đầu (không cần tự đoán số Y)")]
    [SerializeField] private SpriteRenderer debtorSpriteRenderer;

    [Header("Auto Scale/Position Bong Bóng (tự tính, không cần chỉnh tay)")]
    [Tooltip("Khoảng hở thêm phía trên đầu nhân vật (đơn vị Unity units), cộng thêm vào chiều cao sprite")]
    [SerializeField] private float bubbleVerticalPadding = 0.3f;
    [Tooltip("Chiều cao mong muốn của chữ trong bong bóng, tính theo world units (VD: 0.3 = chữ cao khoảng 0.3 unit). Scale của Canvas bong bóng sẽ được TỰ TÍNH từ số này chia cho Font Size đang đặt trên Text.")]
    [SerializeField] private float desiredBubbleTextWorldHeight = 0.3f;

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

    private Coroutine chatRoutine;
    private bool playerTurn = true; // ai đi trước, mặc định Player nói trước

    void Awake()
    {
        AutoFitBubble(playerBubbleObject, playerBubbleText, playerSpriteRenderer);
        AutoFitBubble(debtorBubbleObject, debtorBubbleText, debtorSpriteRenderer);
        HideBothBubbles();
    }

    // Tự tính SCALE và VỊ TRÍ của bong bóng chat dựa trên kích thước THẬT của sprite và font size THẬT,
    // thay vì phải tự đoán số tay trong Inspector.
    private void AutoFitBubble(GameObject bubbleObject, TMP_Text bubbleText, SpriteRenderer characterRenderer)
    {
        if (bubbleObject == null) return;

        // 1. Tính SCALE: sao cho chữ trong bong bóng cao đúng "desiredBubbleTextWorldHeight" world units,
        //    dựa trên Font Size đang đặt sẵn trên TMP Text (không quan tâm Font Size bạn để bao nhiêu).
        if (bubbleText != null && bubbleText.fontSize > 0f)
        {
            float scale = desiredBubbleTextWorldHeight / bubbleText.fontSize;
            bubbleObject.transform.localScale = new Vector3(scale, scale, scale);
        }

        // 2. Tính VỊ TRÍ: đặt bong bóng CĂN GIỮA (X=0, Z=0) ngay phía trên đỉnh đầu nhân vật (Y),
        //    dựa theo bounds.extents.y THẬT của SpriteRenderer (đã tính cả Transform Scale của nhân vật).
        //    LUÔN reset cả X/Z về 0 (kể cả khi thiếu SpriteRenderer) vì Unity có thể để lại toạ độ
        //    mặc định "rác" lúc tạo Canvas (ví dụ X=96, Y=54) khiến bong bóng bị lệch xa ra ngoài
        //    khung hình camera, tưởng như bong bóng "biến mất".
        if (bubbleObject.transform.parent != null)
        {
            float localHalfHeight;
            if (characterRenderer != null)
            {
                float worldHalfHeight = characterRenderer.bounds.extents.y;
                float parentScaleY = bubbleObject.transform.parent.lossyScale.y;
                localHalfHeight = (parentScaleY != 0f) ? worldHalfHeight / parentScaleY : worldHalfHeight;
            }
            else
            {
                // Chưa gán SpriteRenderer -> dùng khoảng cách mặc định để vẫn hiện được, thay vì để lệch
                localHalfHeight = 0.5f;
                Debug.LogWarning("[ChatBubble] Chưa gán SpriteRenderer cho " + bubbleObject.name + " -> dùng vị trí mặc định (Y=0.5 + padding). Kéo SpriteRenderer vào Inspector để tự tính chính xác.");
            }

            bubbleObject.transform.localPosition = new Vector3(0f, localHalfHeight + bubbleVerticalPadding, 0f);
        }
    }

    void OnEnable()
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

    void OnDisable()
    {
        if (powerBarController != null)
        {
            powerBarController.OnRoundBegan -= HandleRoundBegan;
            powerBarController.OnMiniGameWon -= HandleRoundEnded;
            powerBarController.OnMiniGameLost -= HandleRoundEnded;
        }
    }

    private void HandleRoundBegan()
    {
        Debug.Log("[ChatBubble] Round bắt đầu -> bắt đầu đếm giờ để hiện bong bóng chat.");
        if (chatRoutine != null) StopCoroutine(chatRoutine);
        chatRoutine = StartCoroutine(ChatLoop());
    }

    private void HandleRoundEnded()
    {
        if (chatRoutine != null) StopCoroutine(chatRoutine);
        HideBothBubbles();
    }

    private IEnumerator ChatLoop()
    {
        // Chờ 2-3 giây đầu round trước khi bắt đầu
        yield return new WaitForSeconds(Random.Range(initialDelayMin, initialDelayMax));

        while (true)
        {
            ShowBubble(playerTurn);

            yield return new WaitForSeconds(bubbleDisplayDuration);

            HideBubble(playerTurn);

            // Đổi lượt cho người còn lại
            playerTurn = !playerTurn;

            // Chờ 1 khoảng ngẫu nhiên trước khi tới lượt tiếp theo
            yield return new WaitForSeconds(Random.Range(intervalMin, intervalMax));
        }
    }

    private void ShowBubble(bool isPlayer)
    {
        if (isPlayer)
        {
            if (playerLines.Length == 0 || playerBubbleObject == null)
            {
                Debug.LogWarning("[ChatBubble] Không hiện được bong bóng Player: thiếu 'Player Bubble Object' hoặc mảng 'Player Lines' rỗng.");
                return;
            }
            string line = playerLines[Random.Range(0, playerLines.Length)];
            if (playerBubbleText != null) playerBubbleText.text = line;
            playerBubbleObject.SetActive(true);
            Debug.Log("[ChatBubble] Hiện bong bóng PLAYER: " + line);
        }
        else
        {
            if (debtorLines.Length == 0 || debtorBubbleObject == null)
            {
                Debug.LogWarning("[ChatBubble] Không hiện được bong bóng Debtor: thiếu 'Debtor Bubble Object' hoặc mảng 'Debtor Lines' rỗng.");
                return;
            }
            string line = debtorLines[Random.Range(0, debtorLines.Length)];
            if (debtorBubbleText != null) debtorBubbleText.text = line;
            debtorBubbleObject.SetActive(true);
            Debug.Log("[ChatBubble] Hiện bong bóng DEBTOR: " + line);
        }
    }

    private void HideBubble(bool isPlayer)
    {
        if (isPlayer)
        {
            if (playerBubbleObject != null) playerBubbleObject.SetActive(false);
        }
        else
        {
            if (debtorBubbleObject != null) debtorBubbleObject.SetActive(false);
        }
    }

    private void HideBothBubbles()
    {
        if (playerBubbleObject != null) playerBubbleObject.SetActive(false);
        if (debtorBubbleObject != null) debtorBubbleObject.SetActive(false);
    }
}