using UnityEngine;

public class DiceTextureResizerUI : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private RenderTexture[] renderTextures;

    private void Start()
    {
        ResizeRenderTexture();
    }

    private void ResizeRenderTexture()
    {
        float canvasWidth = canvasRectTransform.rect.width;
        float canvasHeight = canvasRectTransform.rect.height;

        foreach (RenderTexture texture in renderTextures)
        {
            texture.Release();

            texture.width = (int)canvasWidth;
            texture.height = (int)canvasHeight;

            texture.Create();
        }
    }
}
