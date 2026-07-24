using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Attach this to the GameObject holding your "Floor" Tilemap. The floor
/// tilemap needs NO collider — it's purely walkable ground, so the player
/// moves freely across it. This script's job is just to answer "is this
/// world position walkable?" for anything that needs to query it (the
/// RandomSpawner, an NPC's pathing, item drops, etc), taking the wall
/// tilemap into account too.
///
/// Setup:
/// 1. Create a Tilemap GameObject called "Floor", paint your ground tiles.
/// 2. Add this script to it.
/// 3. Optionally drag your "Walls" tilemap into the Wall Tilemap slot so
///    IsWalkable() correctly excludes tiles that have a wall on top of them.
/// 4. Do NOT add a Collider2D to this tilemap — that would block movement.
/// </summary>
[RequireComponent(typeof(Tilemap))]
public class FloorTilemap : MonoBehaviour
{
    [Tooltip("This floor tilemap (auto-filled from this GameObject).")]
    public Tilemap floorTilemap;

    [Tooltip("Optional. Tiles here are excluded from walkability even if a floor tile exists underneath.")]
    public Tilemap wallTilemap;

    void Reset()
    {
        floorTilemap = GetComponent<Tilemap>();
    }

    void Awake()
    {
        if (floorTilemap == null)
            floorTilemap = GetComponent<Tilemap>();
    }

    /// <summary>Returns true if the given world position sits on floor and is not blocked by a wall.</summary>
    public bool IsWalkable(Vector3 worldPosition)
    {
        Vector3Int cell = floorTilemap.WorldToCell(worldPosition);

        bool hasFloor = floorTilemap.HasTile(cell);
        bool blockedByWall = wallTilemap != null && wallTilemap.HasTile(cell);

        return hasFloor && !blockedByWall;
    }

    /// <summary>Snaps a world position to the center of its containing floor cell (handy for grid-snapped movement).</summary>
    public Vector3 SnapToCellCenter(Vector3 worldPosition)
    {
        Vector3Int cell = floorTilemap.WorldToCell(worldPosition);
        return floorTilemap.GetCellCenterWorld(cell);
    }
}
