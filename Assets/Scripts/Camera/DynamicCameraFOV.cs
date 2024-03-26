using UnityEngine;

public class DynamicCameraFOV : MonoBehaviour
{
    Vector2 lastScreenWidth = Vector2.zero;

    [SerializeField] private Transform[] targets;
    private int fovFactor = 3;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        AdjustFOV();
    }

    private void AdjustFOV()
    {
        Vector2 newScreenWidth = new Vector2(Screen.width, Screen.height);

        if (lastScreenWidth == newScreenWidth) return;

        lastScreenWidth = newScreenWidth;

        float baseVerticalFOV = 57f;
        float aspectRatio = (float)Screen.safeArea.width / Screen.safeArea.height;

        float desiredVerticalFOV = baseVerticalFOV * (16f / 9f) / aspectRatio;

        cam.fieldOfView = desiredVerticalFOV + fovFactor;
    }
}