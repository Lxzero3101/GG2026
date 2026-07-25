using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerClickInput : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        // Frozen during the intro countdown (see PlayerMovement.IsLocked).
        if (PlayerMovement.Instance != null && PlayerMovement.Instance.IsLocked)
        {
            return;
        }

        // Check for click/tap using New Input System
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 mousePosition = Pointer.current.position.ReadValue();
            Vector2 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

            if (hit.collider != null)
            {
                // Trigger click on item if found
                hit.collider.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}