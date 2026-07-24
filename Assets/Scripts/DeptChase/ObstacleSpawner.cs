using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject obstaclePrefab;

    [Header("Lane Settings")]
    public float[] lanePositions = { 2f, 0f, -2f };
    public float spawnX = 10f;

    [Header("Spawn Timing")]
    public float minInterval = 1f;
    public float maxInterval = 2f;
    public bool IsSpawning { get; set; } = false;

    private float timer;


    void Start()
    {
        timer = 1f; // delay nhỏ trước khi spawn cái đầu tiên
       
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
        int lane = Random.Range(0, lanePositions.Length);
        Vector3 spawnPos = new Vector3(spawnX, lanePositions[lane], 0f);
        Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
    }

    public void StartSpawning() => IsSpawning = true;
    public void StopSpawning() => IsSpawning = false;
}