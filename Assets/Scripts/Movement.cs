using UnityEngine;

/// <summary>
/// Moves the attached object around an N x N grid using WASD or Arrow Keys.
/// Attach this to your Player GameObject.
///
/// Two modes are supported:
///  - Snap Movement (like classic grid games): player instantly steps one cell at a time,
///    with an optional smooth glide between cells.
///  - Free Movement (like Brotato): player moves freely and continuously with WASD,
///    but is clamped to stay within the bounds of the N x N grid.
///
/// Set "useFreeMovement" in the Inspector to choose which style you want.
/// </summary>
public class Movement : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("Size of the grid (N x N cells).")]
    public int gridSize = 10;

    [Tooltip("World size of a single grid cell.")]
    public float cellSize = 1f;

    [Tooltip("World position of the grid's bottom-left corner (cell 0,0).")]
    public Vector2 gridOrigin = Vector2.zero;

    [Header("Movement Mode")]
    [Tooltip("If true, moves freely like Brotato. If false, snaps one cell at a time.")]
    public bool useFreeMovement = true;

    [Header("Free Movement Settings")]
    [Tooltip("Movement speed in units per second (used only in Free Movement mode).")]
    public float moveSpeed = 5f;

    [Header("Snap Movement Settings")]
    [Tooltip("Time in seconds to glide between grid cells (used only in Snap mode).")]
    public float snapMoveDuration = 0.12f;

    // Internal state for snap movement
    private Vector2Int _currentCell;
    private Vector3 _snapTargetPosition;
    private bool _isSnapping;
    private float _snapTimer;
    private Vector3 _snapStartPosition;

    private void Start()
    {
        if (!useFreeMovement)
        {
            // Initialize the starting cell based on current position
            _currentCell = WorldToCell(transform.position);
            transform.position = CellToWorld(_currentCell);
            _snapTargetPosition = transform.position;
        }
    }

    private void Update()
    {
        if (useFreeMovement)
        {
            HandleFreeMovement();
        }
        else
        {
            HandleSnapMovement();
        }
    }

    // ---------------------------------------------------------
    // FREE MOVEMENT (Brotato-style continuous movement)
    // ---------------------------------------------------------
    private void HandleFreeMovement()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        Vector2 input = new Vector2(h, v);
        if (input.sqrMagnitude > 1f)
            input.Normalize();

        Vector3 movement = new Vector3(input.x, input.y, 0f) * moveSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position + movement;

        // Clamp position so the player stays within the N x N grid bounds
        float minX = gridOrigin.x;
        float minY = gridOrigin.y;
        float maxX = gridOrigin.x + (gridSize - 1) * cellSize;
        float maxY = gridOrigin.y + (gridSize - 1) * cellSize;

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        transform.position = newPosition;
    }

    // ---------------------------------------------------------
    // SNAP MOVEMENT (one grid cell at a time)
    // ---------------------------------------------------------
    private void HandleSnapMovement()
    {
        if (_isSnapping)
        {
            _snapTimer += Time.deltaTime;
            float t = snapMoveDuration <= 0f ? 1f : Mathf.Clamp01(_snapTimer / snapMoveDuration);
            transform.position = Vector3.Lerp(_snapStartPosition, _snapTargetPosition, t);

            if (t >= 1f)
            {
                _isSnapping = false;
                transform.position = _snapTargetPosition;
            }
            return; // ignore new input while mid-move
        }

        Vector2Int direction = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) direction = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) direction = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) direction = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) direction = Vector2Int.right;

        if (direction == Vector2Int.zero)
            return;

        Vector2Int targetCell = _currentCell + direction;

        // Clamp to grid bounds (0 to gridSize - 1)
        targetCell.x = Mathf.Clamp(targetCell.x, 0, gridSize - 1);
        targetCell.y = Mathf.Clamp(targetCell.y, 0, gridSize - 1);

        if (targetCell == _currentCell)
            return; // blocked by edge of grid, don't move

        _currentCell = targetCell;
        _snapStartPosition = transform.position;
        _snapTargetPosition = CellToWorld(_currentCell);
        _snapTimer = 0f;
        _isSnapping = snapMoveDuration > 0f;

        if (!_isSnapping)
            transform.position = _snapTargetPosition; // instant move if duration is 0
    }

    // ---------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------
    private Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(
            gridOrigin.x + cell.x * cellSize,
            gridOrigin.y + cell.y * cellSize,
            transform.position.z
        );
    }

    private Vector2Int WorldToCell(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - gridOrigin.x) / cellSize);
        int y = Mathf.RoundToInt((worldPos.y - gridOrigin.y) / cellSize);
        return new Vector2Int(
            Mathf.Clamp(x, 0, gridSize - 1),
            Mathf.Clamp(y, 0, gridSize - 1)
        );
    }

    // Draws the grid bounds in the Scene view for easy setup
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        float size = (gridSize - 1) * cellSize;
        Vector3 center = new Vector3(gridOrigin.x + size / 2f, gridOrigin.y + size / 2f, 0f);
        Vector3 boxSize = new Vector3(size + cellSize, size + cellSize, 0.1f);
        Gizmos.DrawWireCube(center, boxSize);
    }
}
