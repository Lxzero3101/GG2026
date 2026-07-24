using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Randomly spawns the player and a configurable set of interactable object
/// prefabs (boxes, cupboards, trash cans, vases, etc) onto valid tiles of a
/// Tilemap. Guarantees the player does not spawn near any spawned object.
///
/// Setup:
/// 1. Put this on an empty GameObject in the scene.
/// 2. Assign "Ground Tilemap" (the walkable floor tiles).
/// 3. Optionally assign "Obstacle Tilemap" (tiles that should never be spawned on,
///    e.g. walls/water) — leave empty if you don't have one.
/// 4. Assign the Player prefab.
/// 5. Add entries to "Objects To Spawn": prefab + count, e.g.
///       Box prefab, count 3
///       Cupboard prefab, count 2
///       TrashCan prefab, count 5
///       Vase prefab, count 3
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

    [Tooltip("Minimum distance the player must be from every spawned object.")]
    public float minDistanceFromObjects = 4f;

    [Header("Interactable Objects")]
    public List<SpawnEntry> objectsToSpawn = new List<SpawnEntry>();

    [Tooltip("Minimum distance kept between two spawned objects.")]
    public float minDistanceBetweenObjects = 1.25f;

    [Header("Debug")]
    public bool spawnOnStart = true;

    private readonly List<Vector3> occupiedPositions = new List<Vector3>();

    void Start()
    {
        if (spawnOnStart)
            SpawnAll();
    }

    [ContextMenu("Spawn All")]
    public void SpawnAll()
    {
        occupiedPositions.Clear();

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

        // Spawn objects first so we know where the player needs to avoid.
        foreach (var entry in objectsToSpawn)
        {
            if (entry.prefab == null) continue;

            for (int i = 0; i < entry.count; i++)
            {
                Vector3? worldPos = FindValidPosition(validCells, minDistanceBetweenObjects);

                if (worldPos.HasValue)
                {
                    Instantiate(entry.prefab, worldPos.Value, Quaternion.identity);
                    occupiedPositions.Add(worldPos.Value);
                }
                else
                {
                    Debug.LogWarning($"RandomSpawner: Could not find a valid spawn position for " +
                                      $"{entry.label} (#{i + 1}). Consider lowering minDistanceBetweenObjects " +
                                      $"or enlarging the tilemap.");
                }
            }
        }

        // Spawn the player somewhere far from every object spawned above.
        if (playerPrefab != null)
        {
            Vector3? playerPos = FindValidPosition(validCells, minDistanceFromObjects);

            if (playerPos.HasValue)
            {
                Instantiate(playerPrefab, playerPos.Value, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("RandomSpawner: Could not find a valid spawn position for the player " +
                                  "far enough from objects. Try lowering minDistanceFromObjects.");
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

    /// <summary>
    /// Picks a random valid cell whose world position is at least minDist away
    /// from every already-occupied position.
    /// </summary>
    private Vector3? FindValidPosition(List<Vector3Int> cells, float minDist)
    {
        // Shuffle a copy so each call explores cells in a fresh random order.
        List<Vector3Int> shuffled = new List<Vector3Int>(cells);
        Shuffle(shuffled);

        foreach (var cell in shuffled)
        {
            Vector3 worldPos = groundTilemap.GetCellCenterWorld(cell);

            bool tooClose = false;
            foreach (var occ in occupiedPositions)
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

    private void Shuffle(List<Vector3Int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
