using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Has utility functions for working with color
/// </summary>
public class ColorUtils : MonoBehaviour
{
    private static int colorState;
    private static Color[] colors = new Color[] { Color.cyan, Color.green, Color.blue, Color.white };
    /// <summary>
    /// Returns some color, will return different colors
    /// </summary>
    /// <returns></returns>
    public static Color GetColor()
    {
        return colors[colorState++ % colors.Length];
    }
    /// <summary>
    /// Returns a list of color which are adjacent to the center color on the HSV wheel
    /// Color will have same S and V values, but will have different H value
    /// </summary>
    /// <param name="centerColor"></param>
    /// <param name="totalColors">How many colors are required</param>
    /// <param name="maxDeviation">Distance of first color hue from the center color (on a 360 scale)</param>
    /// <returns></returns>
    public static List<Color32> GetAdjacentColors(Color centerColor, int totalColors, float maxDeviation)
    {
        maxDeviation = maxDeviation / 360;
        float H, S, V;
        Color.RGBToHSV(centerColor, out H, out S, out V);

        List<Color32> result = new List<Color32>();
        result.Add(centerColor);

        for (int i = 0; i < totalColors - 0; i++)
        {
            float devH = Mathf.Lerp(H - maxDeviation, H + maxDeviation, (float)i / totalColors - 0);
            if (devH < 0)
                devH += 1;
            if (devH > 1)
                devH -= 1;

            result.Add(Color.HSVToRGB(devH, S, V));
        }
            
  
        return result;
    }
}
