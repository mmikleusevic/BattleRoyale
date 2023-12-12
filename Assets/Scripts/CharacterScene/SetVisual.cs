using UnityEngine;

public class SetVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer bodyMeshRenderer;

    private Material material;

    private void Awake()
    {
        material = new Material(bodyMeshRenderer.material);
        bodyMeshRenderer.material = material;
    }

    public void SetColor(Color color)
    {
        material.color = color;
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}