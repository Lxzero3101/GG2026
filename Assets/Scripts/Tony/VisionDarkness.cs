using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the "Custom/VisionMask" shader to create a Feeding-Frenzy-style
/// dark screen with a soft circle of visibility around the player.
///
/// SETUP:
/// 1. Create the material: right-click in Project > Create > Material,
///    name it "VisionMaskMat", set its Shader to "Custom/VisionMask".
/// 2. Create a Canvas: GameObject > UI > Canvas (Screen Space - Overlay is fine).
/// 3. Inside that Canvas, add an Image: GameObject > UI > Image.
///    - Set its Source Image to None (so it's a plain rect, not a sprite).
///    - Stretch its RectTransform to fill the whole canvas (Anchor Presets:
///      hold Alt+Shift and click the bottom-right "stretch" option).
///    - Assign "VisionMaskMat" to its Material slot.
/// 4. Add this script to that same Image GameObject.
/// 5. Drag the Player into the "Player" field. Leave "Main Camera" empty to
///    auto-use Camera.main.
/// 6. Tune Radius / Softness in the Inspector to taste.
///
/// NOTE: This overlay darkens EVERYTHING behind it, including other UI.
/// If you have HUD elements (health bar, etc) that should stay visible
/// regardless of the light radius, put them on a SEPARATE Canvas with a
/// higher "Sort Order" so they render on top of this one.
/// </summary>
[RequireComponent(typeof(Image))]
public class VisionDarkness : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player — the visible circle follows this.")]
    public Transform player;

    [Tooltip("Leave empty to auto-use Camera.main.")]
    public Camera mainCamera;

    [Header("Vision Settings")]
    [Tooltip("Size of the fully-visible circle around the player (0-1, relative to screen height).")]
    [Range(0f, 1f)] public float radius = 0.18f;

    [Tooltip("Width of the soft fade-to-black edge around the radius.")]
    [Range(0.01f, 1f)] public float softness = 0.15f;

    [Tooltip("Color of the darkness (usually black, but you can tint it e.g. dark blue for a night feel).")]
    public Color darknessColor = Color.black;

    private Image image;
    private Material materialInstance;

    void Awake()
    {
        image = GetComponent<Image>();

        // Instance the material so we don't edit the shared asset at runtime.
        Shader shader = Shader.Find("Custom/VisionMask");
        if (shader == null)
        {
            Debug.LogError("VisionDarkness: Could not find shader 'Custom/VisionMask'. " +
                            "Make sure VisionMask.shader is in your project.");
            enabled = false;
            return;
        }

        materialInstance = new Material(shader);
        image.material = materialInstance;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (player == null || mainCamera == null || materialInstance == null)
            return;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(player.position);

        materialInstance.SetVector("_Center", new Vector4(viewportPos.x, viewportPos.y, 0f, 0f));
        materialInstance.SetFloat("_Radius", radius);
        materialInstance.SetFloat("_Softness", softness);
        materialInstance.SetColor("_Color", darknessColor);
        materialInstance.SetFloat("_Aspect", (float)Screen.width / Screen.height);
    }
}
