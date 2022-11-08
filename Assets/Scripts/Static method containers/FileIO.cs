using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;
using System.IO;

/// <summary>
/// Has methods for working with file system
/// </summary>
/// 
public class FileIO 
{
    public static string GetSvgPath(string fileName)
    {
        return Path.Combine(Application.streamingAssetsPath, "VectorFiles", fileName + ".svg");
    }

    public static string GetLevelDataPath(string dataName, string saveFolder)
    {
        return Path.Combine(saveFolder, dataName + ".asset");
    }

    /// <summary>
    /// Gets the vector scene from a provided file path.
    /// The file must be in Assets/StreamingAssets/VectorFiles folder
    /// And be of type .svg
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static Scene GetVectorSceneFromFile(string fileName)
    {
        string filePath = GetSvgPath(fileName);
        TextReader textReader = null;
        if (Application.platform == RuntimePlatform.Android) //We must use web request to get file if on android
        {
            WWW reader = new WWW(filePath);
            while (!reader.isDone) { }

            textReader = new StringReader(reader.text);
        }
        else
        {
            textReader = new StreamReader(filePath);
        }
        SVGParser.SceneInfo sceneInfo = SVGParser.ImportSVG(textReader);
        return sceneInfo.Scene;
    }
}
