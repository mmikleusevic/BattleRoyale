using UnityEngine;

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

        float screenWidth = canvasRectTransform.rect.width;
        float screenHeight = canvasRectTransform.rect.height;

        int newScreenWidth = (int)screenWidth / 4;
        int newScreenHeight = (int)screenHeight / 4;

        float aspectRatio = screenWidth / screenHeight;

        int renderTextureWidth = newScreenWidth;
        int renderTextureHeight = (int)(newScreenWidth / aspectRatio);

        if (renderTextureHeight > newScreenHeight)
        {
            renderTextureHeight = newScreenHeight;
            renderTextureWidth = (int)(newScreenHeight * aspectRatio);
        }

        foreach (RenderTexture texture in renderTextures)
        {
            texture.Release();

            texture.width = renderTextureWidth;
            texture.height = renderTextureHeight;
            texture.anisoLevel = 0;
            texture.useDynamicScale = true;

            texture.Create();
        }
    }
}
