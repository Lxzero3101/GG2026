using System.Collections;
using UnityEngine;

public class PlayerCrash : MonoBehaviour
{
    [Header("Stun Settings")]
    public float stunDuration = 1.5f;

    private PlayerLaneController laneController;
    private SpriteRenderer spriteRenderer;
    private bool isStunned = false;

    public System.Action OnCrash; // MiniGameManager lắng nghe event này


    private Color originalColor;

    // SFX
    [Header("Audio")]
    [SerializeField] private AudioClip crashSfx;

    void Awake()
    {
        laneController = GetComponent<PlayerLaneController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color; // lưu màu gốc
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle") && !isStunned)
        {
            Destroy(other.gameObject);
            StartCoroutine(StunRoutine());
            OnCrash?.Invoke();

            Debug.Log($"[PlayerCrash] AudioManager.Instance is {(AudioManager.Instance != null ? "OK" : "NULL")}, crashSfx is {(crashSfx != null ? crashSfx.name : "NULL")}");
            AudioManager.Instance?.PlaySfx(crashSfx);
        }
    }

    IEnumerator StunRoutine()
    {
        isStunned = true;
        laneController.InputLocked = true;

        // Flash đỏ để báo hiệu bị đâm
        float elapsed = 0f;
        while (elapsed < stunDuration)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.2f;
        }

        // Trả về màu gốc
        spriteRenderer.color = originalColor; // trả về màu gốc thay vì Color.blue
        laneController.InputLocked = false;
        isStunned = false;
    }
}