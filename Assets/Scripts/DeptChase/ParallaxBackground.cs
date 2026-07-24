using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 2f;        // tốc độ cuộn
    public float resetPositionX = 20f;    // khoảng rộng của sprite (đo trong scene)

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Di chuyển sang trái
        transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

        // Khi ra khỏi màn hình thì reset về vị trí ban đầu (loop vô hạn)
        if (transform.position.x <= startPosition.x - resetPositionX)
            transform.position = startPosition;
    }
}