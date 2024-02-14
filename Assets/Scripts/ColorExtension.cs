using UnityEngine;

public static class ColorExtension
{
    public static Color RGBToColor(this string colorString)
    {
        string[] rgba = colorString.Substring(5, colorString.Length - 6).Split(", ");
        Color color = new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), float.Parse(rgba[3]));

        return color;
    }

    public static Color HEXToColor(this string colorString)
    {
        colorString = colorString.Replace("0x", "");
        colorString = colorString.Replace("#", "");

        byte a = 255;
        byte r = byte.Parse(colorString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(colorString.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(colorString.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        if (colorString.Length == 8)
        {
            a = byte.Parse(colorString.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }

        return new Color32(r, g, b, a);
    }
}