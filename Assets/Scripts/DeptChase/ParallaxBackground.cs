using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 2f;        // tốc độ cuộn
    public float resetPositionX = 20f;    // khoảng rộng của sprite (đo trong scene)

    private Vector3 startPosition;
    private bool isPaused = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (isPaused) return;

        // Di chuyển sang trái
        transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

        // Khi ra khỏi màn hình thì reset về vị trí ban đầu (loop vô hạn)
        if (transform.position.x <= startPosition.x - resetPositionX)
            transform.position = startPosition;
    }

    /// <summary>Pauses or resumes the parallax scroll (e.g. during countdown or after win/lose).</summary>
    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }
}