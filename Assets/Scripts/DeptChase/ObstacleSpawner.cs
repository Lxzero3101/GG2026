using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject[] obstaclePrefabs; // kéo 4 prefab vào đây

    [Header("Lane Settings")]
    public float[] lanePositions = { -2f, -3f, -4f }; // Line 1, Line 2, Line 3 (bottom)
    public float spawnX = 10f;

    [Header("Spawn Timing")]
    public float minInterval = 1f;
    public float maxInterval = 2f;
    public bool IsSpawning { get; set; } = false;
    [Header("Audio")]
    [SerializeField] private AudioClip spawnSfx;

    [Header("Sorting Order Fix")]
    [Tooltip("Lane index that should render in front of the player (0-based). With the default lanePositions, index 2 = Line 3 / bottom lane.")]
    [SerializeField] private int frontLaneIndex = 2;

    [Tooltip("Sorting order applied to obstacles spawned in frontLaneIndex. Must be HIGHER than the player's Sprite Renderer sorting order.")]
    [SerializeField] private int frontLaneSortingOrder = 7;

    [Tooltip("Sorting order applied to obstacles on every OTHER lane. Must be LOWER than the player's Sprite Renderer sorting order. Explicitly set on every spawn so this no longer depends on whatever default value the obstacle prefab happens to have.")]
    [SerializeField] private int backLaneSortingOrder = 5;


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
        GameObject obstacle = Instantiate(obstaclePrefabs[index], spawnPos, Quaternion.identity);
        AudioManager.Instance?.PlaySfx(spawnSfx, 0.4f);

        // Explicitly set sortingOrder on EVERY spawn, for every lane — not just
        // the front lane — so this never depends on whatever default value the
        // prefab happens to have (which is what caused lanes 1/2 to start
        // covering the player after the player's own order changed).
        SpriteRenderer sr = obstacle.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = (lane == frontLaneIndex) ? frontLaneSortingOrder : backLaneSortingOrder;
        else
            Debug.LogWarning($"[ObstacleSpawner] Spawned obstacle '{obstacle.name}' has no SpriteRenderer in itself or its children — can't fix sorting order.");
    }

    public void StartSpawning() => IsSpawning = true;
    public void StopSpawning() => IsSpawning = false;
}