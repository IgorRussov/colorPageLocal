using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Has methods and values required to convert positions in svg space to world space
/// </summary>
public class PositionConverter
{
    /// <summary>
    /// Ammount of svg pixels in one unity unit
    /// </summary>
    public static float SvgPixelsPerUnit;
    
    /// <summary>
    /// Returns the unity world position of the center of a svg coordinates rect
    /// Must always be used when this value is required
    /// Set by the drawing zone script to the value from inspector
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    public static Vector2 GetWorldCenterPos(Rect bounds)
    {
        Vector2 pos = new Vector2(bounds.width, -bounds.height) / 2;
        pos += new Vector2(bounds.x, -bounds.y);
        pos /= 100;

        return pos;
    }

    public static Vector2 VectorPosToWorldPos(Vector2 vectorPos)
    {
        return new Vector2(vectorPos.x, -vectorPos.y) / SvgPixelsPerUnit;
    }
}
