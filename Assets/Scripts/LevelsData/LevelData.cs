
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct ColorData
{
    public byte r;
    public byte g;
    public byte b;
    public byte a;
}

[Serializable]
public struct ColorRow
{
    public ColorData[] row;
}

[CreateAssetMenu()]
public class LevelData : ScriptableObject
{
    [Header("Must be in StreamingAssets/VectorFiles, no .svg")]
    public string svgFileName;
    public int[] strokeShapesOrder;
    public int[] fillShapesOrder;
    public ColorRow[] colors;

    public void InitColorsArray(int rows, int cols)
    {
        colors = new ColorRow[rows];
        for (int i = 0; i < rows; i++)
        {
            colors[i].row = new ColorData[cols];
        }
    }

    public void SetColor(Color32 color, int row, int column)
    {
        colors[row].row[column].r = color.r;
        colors[row].row[column].g = color.g;
        colors[row].row[column].b = color.b;
        colors[row].row[column].a = color.a;
    }

    public Color32 GetColor(int row, int column)
    {
        byte r = colors[row].row[column].r;
        byte g = colors[row].row[column].g;
        byte b = colors[row].row[column].b;
        byte a = colors[row].row[column].a;
        return new Color32(r, g, b, a);
    }

    public List<Color> GetColorsRow(int row)
    {
        List<Color> ret = new List<Color>();
        for (int j = 0; j < colors[0].row.Length; j++)
            ret.Add(GetColor(row, j));
        return ret;
    }

}