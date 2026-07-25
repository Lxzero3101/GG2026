using UnityEngine;

/// <summary>
/// Adds a gentle floating (up/down bob) animation to a UI element or
/// world-space object. Works on RectTransform (UI) or regular Transform.
///
/// SETUP:
/// 1. Add this script to the GameObject you want to float (e.g.
///    "InstructionText").
/// 2. Adjust Float Height / Float Speed in the Inspector to taste.
/// </summary>
public class FloatingAnimation : MonoBehaviour
{
    [Tooltip("How far up/down it moves, in local units (UI: pixels, World: units).")]
    public float floatHeight = 10f;

    [Tooltip("How fast it bobs. Higher = faster.")]
    public float floatSpeed = 2f;

    [Tooltip("Randomizes the starting point of the bob cycle so multiple floating objects don't move in perfect sync.")]
    public bool randomizePhase = true;

    private RectTransform rectTransform;
    private Vector3 basePosition;
    private float phaseOffset;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        phaseOffset = randomizePhase ? Random.Range(0f, Mathf.PI * 2f) : 0f;
    }

    void Start()
    {
        basePosition = rectTransform != null
            ? rectTransform.anchoredPosition3D
            : transform.localPosition;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * floatSpeed + phaseOffset) * floatHeight;
        Vector3 newPosition = basePosition + new Vector3(0f, offset, 0f);

        if (rectTransform != null)
            rectTransform.anchoredPosition3D = newPosition;
        else
            transform.localPosition = newPosition;
    }
}