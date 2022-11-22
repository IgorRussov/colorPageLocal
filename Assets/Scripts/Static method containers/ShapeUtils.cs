using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Unity.VectorGraphics;
using System.Linq;
using System;

/// <summary>
/// Has methods required for processing and creating new vector shapes
/// </summary>
public class ShapeUtils
{
    public static Vector2 drawingSize; //stores the size of the scene we are working with, required for texture generation
    public static Rect sceneRect;

    /// <summary>
    /// Shape is expected to contain only one contour.
    /// Extracts bezier segments from a vector shape
    /// Bezier segments are different from Bezier path segments contained in the shape's contours,
    /// because bezier segments have four control poins, while bezier path segments have three.
    /// They use the following segment's point 0 as their point 3, so a conversion is required.
    /// </summary>
    /// <param name="shape">Is expected to conain only one contour</param>
    /// <returns>An array of BezierSegments whichi create this shape</returns>
    public static BezierSegment[] ShapeToBezierSegments(Shape shape)
    {
        BezierPathSegment[] seg = shape.Contours[0].Segments;
        //If the shape is not closed, the last segment will contain just point 0, so it
        //will only be used as the point 3 of the second to last segment.
        //if the shape is closed, point 0 of the first segment will be used as point 3 of 
        //the last segment. This is achivied in the cycle via mod division
        int count = shape.Contours[0].Closed ? seg.Length : seg.Length - 1;
        BezierSegment[] result = new BezierSegment[count];
        for(int i = 0; i < count; i++)
        {
            BezierPathSegment s1 = seg[i];
            BezierSegment s2 = new BezierSegment();
            s2.P0 = s1.P0;
            s2.P1 = s1.P1;
            s2.P2 = s1.P2;
            s2.P3 = seg[(i + 1) % seg.Length].P0; //Mod division to get next segment OR first segment if this is the last
            result[i] = s2;
        }
        return result;
    }

    public static Vector2 TangentAt(BezierSegment s, float t)
    {
        Vector2 p1 = VectorUtils.Eval(s, t - 0.01f);
        Vector2 p2 = VectorUtils.Eval(s, t);
        return p2 - p1;

        //-3(1-t)^2 * P0 + 3(1-t)^2 * P1 - 6t(1-t) * P1 - 3t^2 * P2 + 6t(1-t) * P2 + 3t^2 * P3 
        Vector2 tan = 
            -3 * (1 - t) * (1 - t) * s.P0 
            + 3 * (1 - t) * (1 - t) * s.P1 
            - 6 * t * (1 - t) * s.P1
            - 3 * t * t * s.P2
            + 6 * t * (1 - t) * s.P2
            + 3 * t * t * s.P3;
        return tan;
    }


    private static float prevEvalPoint;
    private static int prevIndex;
    private static Shape prevShape;
    private static float[] segmentsLength;

    /// <summary>
    /// Gets svg space coordinates of a point on the shape's contour which is 
    /// evalPoint units away from the start of the contour.
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="evalPoint"></param>
    /// <returns></returns>
    public static Vector2 EvalShape(Shape shape, float evalPoint, bool setSegmentsLength)
    {
        float lengthRemaining = evalPoint;
        BezierSegment[] arr = ShapeToBezierSegments(shape);
        int i = -1;
        float length = 1;
        
        if (shape == prevShape && evalPoint > prevEvalPoint && !setSegmentsLength)
        { 
            if (prevIndex != 0)
            {
                i = prevIndex - 1;
                if (i < segmentsLength.Length)
                    lengthRemaining -= segmentsLength[i];
            }
            
           
        }

        if (setSegmentsLength)
            segmentsLength = new float[arr.Length]; 

        bool flag = false;
        try
        {
            do //We are looking for the segment which contains the required point
            {
                i++;
                length = VectorUtils.SegmentLength(arr[i]);
                if (setSegmentsLength)
                {
                    segmentsLength[i] = length;
                    if (i != 0)
                        segmentsLength[i] += segmentsLength[i - 1];
                }    
                    
                if (lengthRemaining <= length)
                    flag = true;
                else
                    lengthRemaining -= length;
                if (i >= arr.Length)
                    flag = true;
            } while (!flag );
        }catch (Exception e)
        {
               Debug.Log("Shape index wrong");
        }

        prevEvalPoint = evalPoint;
        prevIndex = i;
        prevShape = shape;

        BezierSegment evalSegment = i < arr.Length ? arr[i] : arr.Last();
        //The parameter must be from 0 to 1 for the VecturUtils lib funcion used here
        float evalParameter = i < arr.Length ? 
            lengthRemaining / length 
            : 1; //if we could not get the required segment we are checking the last segment at its' last point
        
        Vector2 result = VectorUtils.Eval(evalSegment, evalParameter);
        //Debug.Log(result);
        return result * PositionConverter.DrawingScale;
    }

    public static Vector2[] PointsFromShape(Shape shape, float samplesForSegment, float minDiff)
    {
        List<Vector2> ret = new List<Vector2>();
        Vector2 prevPoint = Vector2.one * 1000000f;
        BezierSegment[] arr = ShapeToBezierSegments(shape);

        foreach(BezierSegment segment in arr)
        {
            for(float t = 0; t < 1; t += 1/samplesForSegment)
            {
                Vector2 point = VectorUtils.Eval(segment, t);
                if ((prevPoint - point).magnitude > minDiff)
                {
                    prevPoint = point;
                    ret.Add(point);
                }
            }
        }

        return ret.ToArray();

    }
    
    /// <summary>
    /// Returns a shape which is transformed by the Matrix2d
    /// </summary>
    /// <param name="s"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static Shape TransformedShape(Shape s, Matrix2D transform)
    {
        s.Contours[0].Segments = VectorUtils.TransformBezierPath(s.Contours[0].Segments, transform);
        return s;
    }

    /// <summary>
    /// Recursively gets all shapes from the scene node
    /// parentTransform is required to preserve proper transforms in child nodes and is also
    /// passed down recursively
    /// </summary>
    /// <param name="node"></param>
    /// <param name="parentTransform"></param>
    /// <returns></returns>
    public static List<Shape> getAllShapes(SceneNode node, Matrix2D parentTransform)
    {
        List<Shape> shapes = new List<Shape>();
        if (node.Shapes != null)
            node.Shapes.ForEach(s => shapes.Add(TransformedShape(s, parentTransform)));
        if (node.Children != null)
            node.Children.ForEach(c => shapes = shapes.Concat(getAllShapes(c, parentTransform * c.Transform)).ToList());
        return shapes;
    }
    
    /// <summary>
    /// Creates a shape with the same values as original
    /// This method is called from the method with the same name in the
    /// DrawingSpriteFactory class
    /// </summary>
    /// <param name="s1"></param>
    /// <returns></returns>
    public static Shape CloneShape(Shape s1)
    {
        Shape s2 = new Shape();
        s2.Contours = (BezierContour[])s1.Contours.Clone();
        s2.Fill = s1.Fill;
        s2.FillTransform = s1.FillTransform;
        s2.IsConvex = s1.IsConvex;
        s2.PathProps = s1.PathProps;
        return s2;
    }

    /// <summary>
    /// Returns a cloned shape with fill set to null
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static Shape GetNoFillShape(Shape source)
    {
        Shape ret = CloneShape(source);
        ret.Fill = null;
        return ret;
    }

    /// <summary>
    /// Returns a cloned shape with stroke set to null
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static Shape GetNoStrokeShape(Shape source)
    {
        Shape ret = CloneShape(source);
        PathProperties props = ret.PathProps;
        props.Stroke = null;
        ret.PathProps = props;
        return ret;
    }

    /// <summary>
    /// Sets a drawing size rect which is later required to generate texture
    /// with size of the scene (to cover all the drawing)
    /// Must be called before the drawing stage begins, during setup
    /// </summary>
    /// <param name="scene"></param>
    public static void SetDrawingSize(float width, float height)
    {
        Rect rect = new Rect(0, 0, width, height);
        if (sceneRect.height == 0)
            sceneRect = rect;

        drawingSize.x = width;
        drawingSize.y = height;
    }

    public static Color GetShapeColor(Shape shape)
    {
        return ((SolidFill)shape.Fill).Color;
    }

    /// <summary>
    /// Checks all shapes in the scene,
    ///  for each shape checks if it has fill and stroke
    ///  If has fill, adds cloned shape with no stroke to fill shapes
    ///  If has stroke, adds cloned shape with no fill to stroke shapes
    ///  Thus it can separate one shape to two if it has both stroke and fill
    ///  Is required for the proper game flow
    /// </summary>
    /// <param name="source">Scene from which shapes are taken</param>
    /// <param name="strokeShapes">List containing created shapes with only stroke</param>
    /// <param name="fillShapes">List containing created shapes with only fill</param>
    public static void SeparateStrokeAndFill(Scene source, out List<Shape> strokeShapes, out List<Shape> fillShapes)
    {
        //List<Shape> shapes = getAllShapes(source.Root, source.Root.Transform);
        List<Shape> shapes = getAllShapes(source.Root, Matrix2D.identity);
        strokeShapes = new List<Shape>();
        fillShapes = new List<Shape>();
        Dictionary<Color, List<Shape>> shapesOfColor = new Dictionary<Color, List<Shape>>();


        foreach (Shape s in shapes)
        {  
            if (s.Fill != null)
            {
                Shape fillShape = GetNoStrokeShape(s);
                Color color = GetShapeColor(fillShape);

                if (!shapesOfColor.ContainsKey(color))
                    shapesOfColor.Add(color, new List<Shape>());
                shapesOfColor[color].Add(fillShape);
               
            }
            if (s.PathProps.Stroke != null)
            {
                Shape strokeShape = GetNoFillShape(s);
                strokeShapes.Add(strokeShape);
            }
        }

        foreach(List<Shape> shapesWithColor in shapesOfColor.Values)
        {
            Shape baseShape = shapesWithColor[0];
            baseShape.Contours = shapesWithColor.Select(s => s.Contours[0]).ToArray();
            fillShapes.Add(baseShape);
        }
    }



    public static Shape ContinueShape(Shape shape, Vector2 direction, float dist)
    {
        BezierContour shapeContour = shape.Contours.First();

        BezierSegment[] segments = ShapeToBezierSegments(shape);
        BezierSegment lastSegment;
        if (shapeContour.Closed)
            lastSegment = segments[segments.Length - 2];
        else
            lastSegment = segments[segments.Length - 1];

        BezierSegment newSegment = VectorUtils.MakeLine(lastSegment.P3, lastSegment.P3 + direction.normalized * dist);

        BezierSegment[] newSegments = new BezierSegment[segments.Length + 1];
        Array.Copy(segments, newSegments, segments.Length);
        newSegments[segments.Length] = newSegment;

        BezierPathSegment[] newCountour = VectorUtils.BezierSegmentsToPath(newSegments);

        Shape newShape = CloneShape(shape);
        shapeContour.Closed = false;
        shapeContour.Segments = newCountour;
        newShape.Contours[0] = shapeContour;

        return newShape;
    }

    /// <summary>
    /// <remarcs>
    /// !===! DOES NOT WORK PROPERLY FOR NOW, LINES GO IN STRANGE DIRECTIONS !===!
    /// </remarcs>
    /// <br></br>
    /// Takes shapes and adds a straight line at the end of each shape's contour
    /// This line is required for the mechanic when the players stops drawing to late and the line
    /// is continued
    /// </summary>
    /// <param name="sourceShapes"></param>
    /// <param name="continueLineLength">Length of the continuing line in svg pixels</param>
    /// <returns></returns>
    public static List<Shape> CreateDrawShapes(List<Shape> sourceShapes, float continueLineLength)
    {
        List<Shape> ret = new List<Shape>();
        foreach (Shape s in sourceShapes)
            ret.Add(CloneShape(s));
        return ret;
        foreach(Shape s in sourceShapes)
        {
            BezierContour shapeContour = s.Contours.First();

            BezierSegment[] segments = ShapeToBezierSegments(s);
            BezierSegment lastSegment;
            if (shapeContour.Closed)
                lastSegment = segments[segments.Length - 2];
            else
                lastSegment = segments[segments.Length - 1];

            //Vector2 endTangent = VectorUtils.EvalTangent(lastSegment, 1);
            Vector2 endTangent = TangentAt(lastSegment, 1);
            Vector2 direction = endTangent.normalized * continueLineLength;

            BezierSegment newSegment = VectorUtils.MakeLine(lastSegment.P3, lastSegment.P3 + direction);

            BezierSegment[] newSegments = new BezierSegment[segments.Length + 1];
            Array.Copy(segments, newSegments, segments.Length);
            newSegments[segments.Length] = newSegment;

            BezierPathSegment[] newCountour = VectorUtils.BezierSegmentsToPath(newSegments);

            Shape newShape = CloneShape(s);
            shapeContour.Closed = false;
            shapeContour.Segments = newCountour;
            newShape.Contours[0] = shapeContour;

            ret.Add(newShape);
        }
        return ret;
    }

    public static Rect GetShapeRect(Shape shape)
    {
        Shape newShape = null;
        Scene newScene = DrawingSpriteFactory.CreateSceneWithClonedShape(shape, out newShape, false);

        return VectorUtils.SceneNodeBounds(newScene.Root);
    }

    public static void ScaleSceneToFit(Scene scene, float desiredWitdh)
    {
        Rect sceneRect = VectorUtils.ApproximateSceneNodeBounds(scene.Root);
        float currentWidth = sceneRect.width;
        float scale = desiredWitdh / currentWidth;
        PositionConverter.DrawingScale = scale;
        Matrix2D matrix = Matrix2D.Scale(Vector2.one * scale);
        scene.Root.Transform = scene.Root.Transform * matrix;
    }

    /// <summary>
    /// Extracts a pattern fill from scene
    /// Not sure if it works properly, not used for now  
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    public static PatternFill GetPatternFillFromScene(Scene scene)
    {
        List<Shape> shapes = getAllShapes(scene.Root, scene.Root.Transform);
        return shapes.Where(s => s.Fill as PatternFill != null).First().Fill as PatternFill;
    }

    /// <summary>
    /// Combines two floats into an array.
    /// </summary>
    /// <param name="dashLength">Length of dashes (filled line)</param>
    /// <param name="emptyLength">Length of empty space between dashes</param>
    /// <returns></returns>
    public static float[] CreateStrokeArray(float dashLength, float emptyLength)
    {
        return new float[] { dashLength, emptyLength };
    }

    /// <summary>
    /// Creats a texture with the size of the svg scene that is being processed
    /// </summary>
    /// <param name="color">Color of all texture or of pattern lines</param>
    /// <param name="padding"></param>
    /// <param name="pattern">If false, generates a solid color texture. If true, adds diagonal lines</param>
    /// <returns></returns>
    public static Texture2D CreateSceneSizedTexture(Color32 color, Vector2 padding, bool pattern)
    {
        int textureWidth = Mathf.RoundToInt(drawingSize.x + padding.x);
        int textureHeigth = Mathf.RoundToInt(drawingSize.y + padding.y);
        Texture2D spriteTexture = new Texture2D(textureWidth, textureHeigth, TextureFormat.ARGB32, false);
        Color32[] fillColorArray = new Color32[textureWidth * textureHeigth];
        for (int i = 0; i < fillColorArray.Length; i++)
        {
            if (pattern)
                fillColorArray[i] = (i % 35 < 10) ? color : (Color32)Color.HSVToRGB(192.0f/360, 0.01f, 0.98f); 
            else
                fillColorArray[i] = color;
        }
            
        spriteTexture.SetPixels32(fillColorArray);
        spriteTexture.Apply();
        return spriteTexture;
    }

    public static RenderTexture CreateSceneSizedRenderTexture(Vector2 padding)
    {
        int textureWidth = Mathf.RoundToInt(drawingSize.x + padding.x);
        int textureHeigth = Mathf.RoundToInt(drawingSize.y + padding.y);
        RenderTexture texture = new RenderTexture(textureWidth, textureHeigth, 0, RenderTextureFormat.ARGB32);
        texture.enableRandomWrite = true;
        texture.Create();
        texture.Release();
        return texture;
    }
}
