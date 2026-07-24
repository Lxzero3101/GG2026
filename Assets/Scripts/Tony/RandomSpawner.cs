using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Randomly spawns the player, a set of regular interactable objects, and a
/// special "win object" onto valid floor tiles. Guarantees:
///   - The player doesn't spawn too close to any regular object.
///   - The player and the win object spawn FAR apart from each other
///     (using a separate, usually larger, minimum distance).
///
/// SETUP:
/// 1. Put this on an empty GameObject in the scene.
/// 2. Assign "Ground Tilemap" (walkable floor tiles) and, optionally,
///    "Obstacle Tilemap" (wall tiles that should never be spawned on).
/// 3. Assign the Player prefab and the Win Object prefab.
/// 4. Add entries to "Objects To Spawn" for your regular objects
///    (boxes, cupboards, trash cans, vases, etc) with counts.
/// 5. Tune the distance settings to taste.
/// </summary>
public class RandomSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public string label = "Object";
        public GameObject prefab;
        [Min(0)] public int count = 1;
    }

    [Header("Tilemap Settings")]
    [Tooltip("Tiles the player and objects are allowed to spawn on.")]
    public Tilemap groundTilemap;

    [Tooltip("Optional. Tiles here are treated as blocked (walls, water, etc).")]
    public Tilemap obstacleTilemap;

    [Header("Player")]
    public GameObject playerPrefab;

    [Tooltip("Drag your Main Camera here so it automatically follows the spawned player.")]
    public CameraFollow cameraToFollowPlayer;

    [Tooltip("Drag your VisionDarkness component here so the dark-vision circle automatically follows the spawned player.")]
    public VisionDarkness visionDarkness;

    [Tooltip("Minimum distance the player must be from every regular spawned object.")]
    public float minDistancePlayerToObjects = 4f;

    [Header("Win Objects")]
    [Tooltip("Win-triggering objects (e.g. multiple decoys that could be 'the' win object, or several win points).")]
    public List<SpawnEntry> winObjectsToSpawn = new List<SpawnEntry>();

    [Tooltip("Minimum distance between the player and EACH win object. Make this large so they don't spawn near the player.")]
    public float minDistancePlayerToWinObject = 10f;

    [Tooltip("Minimum distance a win object must be from every regular spawned object and from other win objects.")]
    public float minDistanceWinObjectToObjects = 1.5f;

    [Header("Regular Interactable Objects")]
    public List<SpawnEntry> objectsToSpawn = new List<SpawnEntry>();

    [Tooltip("Minimum distance kept between two regular spawned objects.")]
    public float minDistanceBetweenObjects = 1.25f;

    [Header("Debug")]
    public bool spawnOnStart = true;

    private readonly List<Vector3> occupiedByObjects = new List<Vector3>();
    private Vector3? playerPosition;

    void Start()
    {
        if (spawnOnStart)
            SpawnAll();
    }

    [ContextMenu("Spawn All")]
    public void SpawnAll()
    {
        occupiedByObjects.Clear();
        playerPosition = null;

        if (groundTilemap == null)
        {
            Debug.LogError("RandomSpawner: Ground Tilemap is not assigned.");
            return;
        }

        List<Vector3Int> validCells = GetValidCells();
        if (validCells.Count == 0)
        {
            Debug.LogError("RandomSpawner: No valid tiles found to spawn on.");
            return;
        }

        // 1. Spawn regular objects first, spaced apart from each other.
        foreach (var entry in objectsToSpawn)
        {
            if (entry.prefab == null) continue;

            for (int i = 0; i < entry.count; i++)
            {
                Vector3? worldPos = FindPosition(validCells, occupiedByObjects, minDistanceBetweenObjects);

                if (worldPos.HasValue)
                {
                    Instantiate(entry.prefab, worldPos.Value, Quaternion.identity);
                    occupiedByObjects.Add(worldPos.Value);
                }
                else
                {
                    Debug.LogWarning($"RandomSpawner: Could not find a valid spawn position for " +
                                      $"{entry.label} (#{i + 1}).");
                }
            }
        }

        // 2. Spawn the player, far from every regular object.
        if (playerPrefab != null)
        {
            Vector3? pos = FindPosition(validCells, occupiedByObjects, minDistancePlayerToObjects);

            if (pos.HasValue)
            {
                GameObject spawnedPlayer = Instantiate(playerPrefab, pos.Value, Quaternion.identity);
                playerPosition = pos.Value;

                if (cameraToFollowPlayer != null)
                    cameraToFollowPlayer.target = spawnedPlayer.transform;

                if (visionDarkness != null)
                    visionDarkness.player = spawnedPlayer.transform;
            }
            else
            {
                Debug.LogWarning("RandomSpawner: Could not find a valid spawn position for the player.");
            }
        }

        // 3. Spawn win objects last, each far from the player AND spaced from
        //    regular objects and from each other.
        foreach (var entry in winObjectsToSpawn)
        {
            if (entry.prefab == null) continue;

            for (int i = 0; i < entry.count; i++)
            {
                Vector3? pos = FindWinObjectPosition(validCells);

                if (pos.HasValue)
                {
                    Instantiate(entry.prefab, pos.Value, Quaternion.identity);
                    // Win objects also count as "occupied" so later win objects
                    // (and regular objects, if spawned after — they aren't here,
                    // but this keeps things consistent) don't overlap them.
                    occupiedByObjects.Add(pos.Value);
                }
                else
                {
                    Debug.LogWarning($"RandomSpawner: Could not find a valid spawn position for " +
                                      $"win object {entry.label} (#{i + 1}) far enough from the player. " +
                                      $"Try lowering minDistancePlayerToWinObject.");
                }
            }
        }
    }

    /// <summary>Collects every tilemap cell that is walkable and not blocked by an obstacle tile.</summary>
    private List<Vector3Int> GetValidCells()
    {
        List<Vector3Int> cells = new List<Vector3Int>();
        BoundsInt bounds = groundTilemap.cellBounds;

        foreach (var pos in bounds.allPositionsWithin)
        {
            if (!groundTilemap.HasTile(pos)) continue;
            if (obstacleTilemap != null && obstacleTilemap.HasTile(pos)) continue;

            cells.Add(pos);
        }

        return cells;
    }

    /// <summary>Picks a random valid cell at least minDist away from every position in occupied.</summary>
    private Vector3? FindPosition(List<Vector3Int> cells, List<Vector3> occupied, float minDist)
    {
        List<Vector3Int> shuffled = new List<Vector3Int>(cells);
        Shuffle(shuffled);

        foreach (var cell in shuffled)
        {
            Vector3 worldPos = groundTilemap.GetCellCenterWorld(cell);

            bool tooClose = false;
            foreach (var occ in occupied)
            {
                if (Vector3.Distance(worldPos, occ) < minDist)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
                return worldPos;
        }

        return null;
    }

    /// <summary>Picks a position for the win object satisfying BOTH distance rules at once.</summary>
    private Vector3? FindWinObjectPosition(List<Vector3Int> cells)
    {
        List<Vector3Int> shuffled = new List<Vector3Int>(cells);
        Shuffle(shuffled);

        foreach (var cell in shuffled)
        {
            Vector3 worldPos = groundTilemap.GetCellCenterWorld(cell);

            if (playerPosition.HasValue &&
                Vector3.Distance(worldPos, playerPosition.Value) < minDistancePlayerToWinObject)
            {
                continue;
            }

            bool tooCloseToObjects = false;
            foreach (var occ in occupiedByObjects)
            {
                if (Vector3.Distance(worldPos, occ) < minDistanceWinObjectToObjects)
                {
                    tooCloseToObjects = true;
                    break;
                }
            }

            if (!tooCloseToObjects)
                return worldPos;
        }

        return null;
    }

    private void Shuffle(List<Vector3Int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
