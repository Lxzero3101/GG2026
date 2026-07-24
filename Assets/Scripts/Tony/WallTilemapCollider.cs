using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Attach this to the GameObject holding your "Wall" Tilemap to make every
/// painted wall tile solid (the player physically cannot walk through it).
///
/// This works together with PlayerController's Rigidbody2D + MovePosition:
/// Unity's physics step automatically stops a Dynamic Rigidbody2D from
/// overlapping a Static collider, so no wall-checking code is needed in the
/// player script — the wall tilemap just needs a proper collider, which this
/// script sets up automatically at runtime (and keeps working if you edit
/// the tilemap later, since CompositeCollider2D regenerates its shape).
///
/// Setup:
/// 1. Create a Tilemap GameObject called "Walls" (Grid > child Tilemap), paint
///    your wall tiles onto it.
/// 2. Add this script to that "Walls" GameObject.
/// 3. Make sure the Player has a non-trigger Collider2D (e.g. CircleCollider2D)
///    and a Dynamic Rigidbody2D (gravity scale 0) — PlayerController already
///    sets gravity/rotation correctly, just add a Collider2D component to the
///    player if it doesn't have one yet.
/// </summary>
[RequireComponent(typeof(Tilemap))]
public class WallTilemapCollider : MonoBehaviour
{
    [Tooltip("If true, the collider shape is rebuilt every time tiles change at runtime. " +
             "Leave off if your walls never change during play, for better performance.")]
    public bool rebuildOnTileChange = false;

    private TilemapCollider2D tilemapCollider;
    private CompositeCollider2D compositeCollider;
    private Rigidbody2D rb;

    void Awake()
    {
        SetupComponents();
    }

    void SetupComponents()
    {
        // Static Rigidbody2D is required for CompositeCollider2D to merge the wall shapes.
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        tilemapCollider = GetComponent<TilemapCollider2D>();
        if (tilemapCollider == null) tilemapCollider = gameObject.AddComponent<TilemapCollider2D>();

        compositeCollider = GetComponent<CompositeCollider2D>();
        if (compositeCollider == null) compositeCollider = gameObject.AddComponent<CompositeCollider2D>();

        // Merge all wall tile shapes into one efficient composite collider.
        tilemapCollider.usedByComposite = true;
        compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;

        // Make sure it's solid, not a trigger.
        compositeCollider.isTrigger = false;
    }

    void OnEnable()
    {
        if (rebuildOnTileChange)
        {
            Tilemap tm = GetComponent<Tilemap>();
            tm.RefreshAllTiles();
        }
    }

    /// <summary>Call this after modifying wall tiles at runtime to refresh collision shapes.</summary>
    public void RefreshCollider()
    {
        if (compositeCollider != null)
        {
            compositeCollider.GenerateGeometry();
        }
    }
}
