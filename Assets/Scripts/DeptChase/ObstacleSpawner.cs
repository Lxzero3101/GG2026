using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject[] obstaclePrefabs; // kéo 4 prefab vào đây

    [Header("Lane Settings")]
    public float[] lanePositions = { -2f, -3f, -4f };
    public float spawnX = 10f;

    [Header("Spawn Timing")]
    public float minInterval = 1f;
    public float maxInterval = 2f;
    public bool IsSpawning { get; set; } = false;

    private float timer;
    private int lastLane = -1;

    void Start()
    {
        timer = 1f;
    }

    void Update()
    {
        if (!IsSpawning) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnObstacle();
            timer = Random.Range(minInterval, maxInterval);
        }
    }

    void SpawnObstacle()
    {
        // Random lane (không trùng lane trước)
        int lane;
        do { lane = Random.Range(0, lanePositions.Length); }
        while (lane == lastLane && lanePositions.Length > 1);
        lastLane = lane;

        // Random obstacle
        int index = Random.Range(0, obstaclePrefabs.Length);
        Vector3 spawnPos = new Vector3(spawnX, lanePositions[lane], 0f);
        Instantiate(obstaclePrefabs[index], spawnPos, Quaternion.identity);
    }

    public void StartSpawning() => IsSpawning = true;
    public void StopSpawning() => IsSpawning = false;
}