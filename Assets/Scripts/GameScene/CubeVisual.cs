using UnityEngine;

public class CubeVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer bodyMeshRenderer;

    private Material material;

    private void Awake()
    {
        material = new Material(bodyMeshRenderer.material);
        bodyMeshRenderer.material = material;
    }

    public void SetCubeColor(Color color)
    {
        material.color = color;
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}