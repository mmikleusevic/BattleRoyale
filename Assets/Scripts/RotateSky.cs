using UnityEngine;

public class RotateSky : MonoBehaviour
{
    private readonly string ROTATION = "_Rotation";

    private float RotateSpeed = 5f;

    private void Update()
    {
        RenderSettings.skybox.SetFloat(ROTATION, Time.time * RotateSpeed);
    }
}