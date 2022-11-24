using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public enum FillQuadrant
{
    Empty,
    Unvisited,
    Visited
}

public static class TextureCounter 
{ 
    public static async void CalculateFillPercent(Texture2D fill, Texture2D mask, Action<float> callback)
    {
        if (fill == null)
            return;
        if (mask == null)
            return;
        Color32[] fillPixels = fill.GetPixels32();
        Color[] maskPixels = mask.GetPixels();
        int fillPixelCount = 0;
        int maskPixelCount = 0;
        await Task.Run(() => CountPixels(fillPixels, maskPixels, ref fillPixelCount, ref maskPixelCount));

        float percent = fillPixelCount * 1.0f / maskPixelCount;
        if (float.IsNaN(percent))
            percent = 1;
        callback.Invoke(percent);
    }

    private static void CountPixels(Color32[] fillPixels, Color[]  maskPixels, ref int fillPixelCount, ref int maskPixelCount)
    {
        for (int i = 0; i < fillPixels.Length; i++)
        {
            if (maskPixels[i].r > 0)
            {
                maskPixelCount++;
                if (fillPixels[i].r > 0 || fillPixels[i].g > 0 || fillPixels[i].b > 0)
                    fillPixelCount++;
            }
        }
    }

    public static FillQuadrant[,] GetFillQuadrants(Texture2D maskTexture, int pixelsPerQuadrant, out int unvisitedNumber)
    {
        int w = maskTexture.width / pixelsPerQuadrant + 1;
        int h = maskTexture.height / pixelsPerQuadrant + 1;

        FillQuadrant[,] ret = new FillQuadrant[w, h];
        unvisitedNumber = 0;
        for (int i = 0; i < w; i++)
            for (int j = 0; j < h; j++)
                if (maskTexture.GetPixel(i * pixelsPerQuadrant, 
                                         j * pixelsPerQuadrant).r > 0)
                {
                    ret[i, j] = FillQuadrant.Unvisited;
                    unvisitedNumber++;
                }
                    

        return ret;

    }
}
