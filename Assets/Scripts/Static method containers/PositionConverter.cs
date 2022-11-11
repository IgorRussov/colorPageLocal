using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Has methods and values required to convert positions in svg space to world space
/// </summary>
public class PositionConverter
{
    /// <summary>
    /// Ammount of svg pixels in one unity unit
    /// </summary>
    public static float SvgPixelsPerUnit;
    public static float DrawingScale = 1;
    public static Vector2 shapesObjectPos;
    
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
        pos /= SvgPixelsPerUnit;

        return pos;
    }

    public static Vector2 VectorPosToWorldPos(Vector2 vectorPos)
    {
        return new Vector2(vectorPos.x, -vectorPos.y) / SvgPixelsPerUnit;
    }

    public static Vector2[] ConvertPoints(Vector2[] points)
    {
        Vector2[] result = new Vector2[points.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = points[i] * DrawingScale;
        return result;
    }
    
    public static Vector2 ScreenSpaceToLocalSpace(Vector2 screenSpacePos)
    {
        float k = 1.12272f;
        float camPixelHeight = Camera.main.pixelHeight;
        float camPixelWidth = Camera.main.pixelWidth;
        float camUnitsWidth = Camera.main.orthographicSize * k;
        float camUnitsHeight = camUnitsWidth / Camera.main.aspect;

        float paramPosX = (screenSpacePos.x / camPixelWidth) - 0.5f;
        float paramPosY = (screenSpacePos.y / camPixelHeight) - 0.5f;

        float posX = -shapesObjectPos.x + paramPosX * camUnitsWidth;
        float posY = -shapesObjectPos.y + paramPosY * camUnitsHeight;

        return new Vector2(posX, posY);
    }

    public static Vector2 LocalSpaceToScreenSpace(Vector2 localSpacePos)
    {
        float k = 1.12272f;
        float camPixelHeight = Camera.main.pixelHeight;
        float camPixelWidth = Camera.main.pixelWidth;
        float camUnitsWidth = Camera.main.orthographicSize * k;
        float camUnitsHeight = camUnitsWidth / Camera.main.aspect;

        float paramPosX = ((localSpacePos.x + shapesObjectPos.x) / camUnitsWidth) + 0.5f;
        float paramPosY = ((localSpacePos.y + shapesObjectPos.y) / camUnitsHeight) + 0.5f;

        float posX = paramPosX * camPixelWidth;
        float posY = paramPosY * camPixelHeight;

        return new Vector2(posX, posY);
    }
}
