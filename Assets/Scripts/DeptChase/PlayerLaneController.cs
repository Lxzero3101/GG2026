using UnityEngine;

public class PlayerLaneController : MonoBehaviour
{
    [Header("Lane Settings")]
    public float[] lanePositions = { 2f, 0f, -2f }; // Lane 0 = trên, 1 = giữa, 2 = dưới
    public float moveSpeed = 15f;

    private int currentLane = 1; // Bắt đầu ở lane giữa
    public bool InputLocked { get; set; } = false;
    void Start()
    {
        currentLane = 1; // ép về lane giữa, không tin default value
        transform.position = new Vector3(transform.position.x, lanePositions[currentLane], 0f);
    }

    void Update()
    {
        HandleInput();
        MoveToLane();
    }

    void HandleInput()
    {
        if (InputLocked) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            currentLane = Mathf.Max(0, currentLane - 1);

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            currentLane = Mathf.Min(lanePositions.Length - 1, currentLane + 1);
    }

    void MoveToLane()
    {
        float targetY = lanePositions[currentLane];
        Vector3 target = new Vector3(transform.position.x, targetY, 0f);
        transform.position = Vector3.Lerp(transform.position, target, moveSpeed * Time.deltaTime);
    }

    public int GetCurrentLane() => currentLane;
}
