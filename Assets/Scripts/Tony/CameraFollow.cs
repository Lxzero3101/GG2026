using UnityEngine;

/// <summary>
/// Smoothly follows a target (the Player) with the Camera. Attach to the
/// Main Camera.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Tooltip("The object the camera follows — drag your Player here.")]
    public Transform target;

    [Tooltip("Higher = camera catches up faster. ~5-10 feels smooth for top-down games.")]
    public float smoothSpeed = 8f;

    [Tooltip("Camera offset from the target (Z should stay negative for 2D so the camera is in front).")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Optional Bounds (leave size at 0 to disable)")]
    [Tooltip("Restrict the camera so it never shows outside the level. Set size to 0,0 to disable.")]
    public Vector2 minBounds;
    public Vector2 maxBounds;
    public bool useBounds = false;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}