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
        //XElement root = doc.Nodes().ElementAt(1) as XElement;

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
            string baseClass = pathElement.Attribute("class").Value;
            string[] paths = baseD.Split('M');
            paths = paths.Where(s => s.Length > 0).ToArray();

            for(int i = 0; i < paths.Length; i++)
            {
                XElement newPathElement = new XElement("path");
                newPathElement.SetAttributeValue("d", "M" + paths[i]);
                newPathElement.SetAttributeValue("class", baseClass);
                pathElement.Parent.Add(newPathElement);
            }
        }

        foreach (XElement pathElement in startingPathElements)
            pathElement.Remove();

        return new StringReader(doc.ToString());



    }
}
