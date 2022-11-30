using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;
using System.IO;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System;
using System.Globalization;

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

    public static Texture2D LoadTextureForLevel(string levelName)
    {
        string path = Path.Combine(Application.persistentDataPath, levelName + "_complete.png");
        if (!File.Exists(path))
            return null;
        WWW reader = new WWW(path);
        while (!reader.isDone) { }
        Texture2D ret = new Texture2D(256, 256);
        reader.LoadImageIntoTexture(ret);
        return ret;
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
        ShapeUtils.sceneRect = new Rect();


        string filePath = GetSvgPath(fileName);

        return SVGParser.ImportSVG(GetFixedSceneReader(fileName)).Scene;

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

    private static TextReader GetFixedSceneReader(string fileName)
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
        XDocument doc = XDocument.Parse(textReader.ReadToEnd());
        //Get viewbox info
        string viewBox = doc.Root.Attribute("viewBox").Value;
        string[] viewBoxNumbers = viewBox.Split(' ');
        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";

        float width = Single.Parse(viewBoxNumbers[2], NumberStyles.Any, ci);
        float height = Single.Parse(viewBoxNumbers[3], NumberStyles.Any, ci);
        ShapeUtils.SetDrawingSize(width, height);

        //Split path into separate elements
        List<XElement> startingPathElements = new List<XElement>();

        IEnumerable<XElement> e1 = doc.Descendants("path");
        IEnumerable<XElement> e2 = doc.Elements();
        IEnumerable<XElement> e3 = doc.XPathSelectElements("./path");
        List<XElement> e4 = doc.DescendantNodes().OfType<XElement>().ToList();

        foreach (XElement pathElement in e4)
        {
            if (pathElement.Name != "{http://www.w3.org/2000/svg}path")
                continue;
            startingPathElements.Add(pathElement);
            string baseD = pathElement.Attribute("d").Value;
            string[] paths = baseD.Split('M');
            paths = paths.Where(s => s.Length > 0).ToArray();

            for(int i = 0; i < paths.Length; i++)
            {
                XElement newPathElement = new XElement(pathElement);
                newPathElement.SetAttributeValue("d", "M" + paths[i]);
                pathElement.Parent.Add(newPathElement);
            }
        }

        foreach (XElement pathElement in startingPathElements)
            pathElement.Remove();

        return new StringReader(doc.ToString());



    }
}
