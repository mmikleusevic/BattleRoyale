using UnityEngine;

[ExecuteAlways]
public class DiceTextureResizerUI : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private RenderTexture[] renderTextures;

    Vector2 lastCanvasSize = Vector2.zero;

    private void Start()
    {
        ResizeRenderTexture();
    }

    private void Update()
    {
        ResizeRenderTexture();
    }

    private void ResizeRenderTexture()
    {
        Vector2 canvasSize = new Vector2(canvasRectTransform.rect.width, canvasRectTransform.rect.height);

        if (lastCanvasSize == canvasSize) return;

        int canvasWidth = (int)canvasRectTransform.rect.width;
        int canvasHeight = (int)canvasRectTransform.rect.height;

        lastCanvasSize.x = canvasWidth;
        lastCanvasSize.y = canvasHeight;

        foreach (RenderTexture texture in renderTextures)
        {
            texture.Release();

            texture.width = canvasWidth;
            texture.height = canvasHeight;

            texture.Create();
        }
    }
}
