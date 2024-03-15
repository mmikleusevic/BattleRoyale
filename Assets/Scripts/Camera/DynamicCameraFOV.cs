using UnityEngine;

public class DynamicCameraFOV : MonoBehaviour
{
    [SerializeField] private Transform[] targets;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        AdjustFOV();
    }

    void AdjustFOV()
    {
        float baseVerticalFOV = 57f;
        float aspectRatio = (float)Screen.width / Screen.height;

        float desiredVerticalFOV = baseVerticalFOV * (16f / 9f) / aspectRatio;

        cam.fieldOfView = desiredVerticalFOV;
    }
}