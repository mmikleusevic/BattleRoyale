using UnityEngine;

public static class ColorExtension
{
    public static Color ToColor(this string colorString)
    {
        string[] rgba = colorString.Substring(5, colorString.Length - 6).Split(", ");
        Color color = new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), float.Parse(rgba[3]));

        return color;
    }
}