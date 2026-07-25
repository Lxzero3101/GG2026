using UnityEngine;

public class FloatingAnimation : MonoBehaviour
{
    public float floatHeight = 10f;
    public float floatSpeed = 2f;
    public bool randomizePhase = true;

    private RectTransform rectTransform;
    private Vector3 basePosition;
    private float phaseOffset;
    private bool initialized;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        phaseOffset = randomizePhase ? Random.Range(0f, Mathf.PI * 2f) : 0f;
    }

    void OnEnable()
    {
        // Capture base position on enable instead of Start — more reliable
        // if the object gets activated/deactivated during play.
        CaptureBasePosition();
    }

    void CaptureBasePosition()
    {
        basePosition = rectTransform != null
            ? rectTransform.anchoredPosition3D
            : transform.localPosition;
        initialized = true;
    }

    void Update()
    {
        if (!initialized) CaptureBasePosition();

        float offset = Mathf.Sin(Time.time * floatSpeed + phaseOffset) * floatHeight;
        Vector3 newPosition = basePosition + new Vector3(0f, offset, 0f);

        if (rectTransform != null)
            rectTransform.anchoredPosition3D = newPosition;
        else
            transform.localPosition = newPosition;
    }
}