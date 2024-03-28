using UnityEngine;

public class TextureColorChanger : MonoBehaviour
{
    public Texture2D textureToChange;

    public void ChangeTextureColor(Color newColor)
    {
        Color[] pixels = textureToChange.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = newColor;
        }

        textureToChange.SetPixels(pixels);
        textureToChange.Apply();
    }
}
