using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;
using System.Linq;
using System;

/// <summary>
/// Has methods that allow creation of sprites, mainly from vector shapes 
/// </summary>
public class DrawingSpriteFactory
{
    #region Cloning graphical elements
    /// <summary>
    /// Creates a new shape with all properties of original
    /// Has a call to ShapeUtils.CloneShape
    /// </summary>
    /// <param name="original">Original shape</param>
    /// <returns></returns>
    private static Shape CloneShape(Shape original)
    {
        return ShapeUtils.CloneShape(original);
    }

    /// <summary>
    /// Creates a scene with one shape, which is a clone of the provided shape
    /// reference to the new shape is provided as an output parameter
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="newShape"></param>
    /// <returns></returns>
    public static Scene CreateSceneWithClonedShape(Shape shape, out Shape newShape, bool addEmptyRectToScene)
    {
        Scene scene = new Scene();
        SceneNode node = new SceneNode();
        scene.Root = node;
        node.Transform = DrawingZone.originalSceneMatrix;
        node.Shapes = new List<Shape>();

        newShape = CloneShape(shape);

        if (addEmptyRectToScene)
        {
            Shape rectShape = new Shape();
            rectShape.Fill = CreateSolidFill(Color.clear, 1);

            Rect rect = new Rect(ShapeUtils.sceneRect);
            //Debug.Log("Mask rect: " + rect);
            //rect.x -= 50;
            //rect.y -= 50;
            //rect.width += 100;
            //rect.height += 100;
            VectorUtils.MakeRectangleShape(rectShape, rect);
            node.Shapes.Add(rectShape);
        }

        node.Shapes.Add(newShape);

        return scene;
    }

    /// <summary>
    /// Screates a solid color fill with NonZero fill mode
    /// </summary>
    /// <param name="fillColor"></param>
    /// <param name="opacity"></param>
    /// <returns></returns>
    private static SolidFill CreateSolidFill(Color fillColor, float opacity)
    {
        SolidFill fill = new SolidFill();
        fill.Color = fillColor;
        fill.Opacity = opacity;
        fill.Mode = FillMode.OddEven;

        return fill;
    }

    /// <summary>
    /// Creates a stroke object with provided properties
    /// </summary>
    /// <param name="dashesArray">Encodes dashed line</param>
    /// <param name="strokeWidth"></param>
    /// <param name="strokeColor"></param>
    /// <param name="original">Original stroke with some important data</param>
    /// <returns></returns>
    private static Stroke CreateStroke(float[] dashesArray, float strokeWidth, Color strokeColor, Stroke original)
    {
        Stroke stroke = new Stroke();
        //Replace stroke to dashes (пунктир)
        stroke.Pattern = dashesArray;
        stroke.HalfThickness = strokeWidth / 2;
        stroke.Color = strokeColor;

        SolidFill fill = CreateSolidFill(strokeColor, 1);

        stroke.FillTransform = original.FillTransform;
        stroke.PatternOffset = original.PatternOffset;
        stroke.TippedCornerLimit = original.TippedCornerLimit;

        return stroke;
    }

    /// <summary>
    /// Creates path properties with provided stroke; corners, head and tail are set to ROUND,
    /// </summary>
    /// <param name="stroke"></param>
    /// <returns></returns>
    private static PathProperties CreatePathProperties(Stroke stroke)
    {
        PathProperties props = new PathProperties();
        props.Corners = PathCorner.Round;
        props.Head = PathEnding.Round;
        props.Tail = PathEnding.Round;
        props.Stroke = stroke;

        return props;
    }

    /// <summary>
    /// Creates a pattern fill
    /// </summary>
    /// <param name="patternNode"></param>
    /// <param name="patternRect"></param>
    /// <returns></returns>
    private static PatternFill CreatePatternFill(SceneNode patternNode, Rect patternRect)
    {
        PatternFill fill = new PatternFill();
        fill.Pattern = patternNode;
        fill.Rect = patternRect;
        fill.Opacity = 1;
        fill.Mode = FillMode.NonZero;

        return fill;
    }

    #endregion
    #region Create generic sprite
    private static List<VectorUtils.Geometry> GetGeomFromScene(Scene scene)
    {
        return VectorUtils.TessellateScene(scene, DrawingZone.TesselationOptions);
    }

    private static void GetGeomFromScene(Scene scene, ref List<VectorUtils.Geometry> retList)
    {
        retList = VectorUtils.TessellateScene(scene, DrawingZone.TesselationOptions);

    }


    /// <summary>
    /// Creates a sprite from a vector scene
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="needAtlas">If a texture and uv atlas is needed: true if scene uses textures</param>
    /// <param name="svgPixelsPerUnit">How many svg units will a Unity distance unit contain</param>
    /// <param name="alignment">Sprite pivot position</param>
    /// <param name="gradientResolution"></param>
    /// <param name="flipYaxis"></param>
    /// <returns></returns>
    private static Sprite GetSpriteFromScene(Scene scene,
        bool needAtlas,
        VectorUtils.Alignment alignment = VectorUtils.Alignment.SVGOrigin,
        ushort gradientResolution = 64,
        bool flipYaxis = true)
    {
        List<VectorUtils.Geometry> geom = GetGeomFromScene(scene);
        if (needAtlas)
        {
            VectorUtils.GenerateAtlasAndFillUVs(geom, 128);
        }
        Sprite result = null;
        if (geom.Count > 0) //This check is required because if geom is empty (can happen if it is a transparent scene)
                            //the method would throw an exception
            result = VectorUtils.BuildSprite(geom, PositionConverter.SvgPixelsPerUnit, alignment, Vector2.zero, gradientResolution, flipYaxis);

        return result;
    }

    /// <summary>
    /// Creates a sprite from a vector shape with provided fill and stroke values
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="fill"></param>
    /// <param name="stroke"></param>
    /// <returns></returns>
    private static Sprite CreateSprite(Shape shape, IFill fill, Stroke stroke, bool addEmptyRectToScene = false)
    {
        Shape newShape = null;
        Scene newScene = CreateSceneWithClonedShape(shape, out newShape, addEmptyRectToScene);

        PathProperties props = CreatePathProperties(stroke);

        newShape.PathProps = props;
        newShape.Fill = fill;

        return GetSpriteFromScene(newScene, false);
    }

    private static Sprite CreatePatternSprite(Shape shape, Scene patternScene)
    {
        SceneNode clipper = new SceneNode();
        clipper.Shapes = new List<Shape>();
        clipper.Shapes.Add(shape);

        patternScene.Root.Clipper = clipper;

        return GetSpriteFromScene(patternScene, false);
    }
    #endregion

    #region Generate specific sprites 
    /// <summary>
    /// Creates a sprite with a solid stroke (can be dashed) and no fill
    /// </summary>
    /// <param name="shape">Shape from which to create sprite</param>
    /// <param name="dashesArray">Dashes array for the stroke</param>
    /// <param name="strokeWidth">In svg units</param>
    /// <param name="strokeColor"></param>
    /// <returns></returns>
    public static Sprite CreateLineSprite(Shape shape,
        float[] dashesArray,
        float strokeWidth,
        Color strokeColor)
    {
        Shape newShape = null;
        Scene newScene = CreateSceneWithClonedShape(shape, out newShape, false);

        Stroke original = shape.PathProps.Stroke;
        Stroke stroke = CreateStroke(dashesArray, strokeWidth, strokeColor, original);

        return CreateSprite(newShape, null, stroke);

    }

    public static async void UpdateLineSprite(System.Action<Sprite> callback, Shape shape,
        float[] dashesArray,
        float strokeWidth,
        Color strokeColor)
    {
        Shape newShape = null;
        Scene newScene = CreateSceneWithClonedShape(shape, out newShape, false);

        Stroke original = shape.PathProps.Stroke;
        Stroke stroke = CreateStroke(dashesArray, strokeWidth, strokeColor, original);

        PathProperties props = CreatePathProperties(stroke);

        newShape.PathProps = props;
        newShape.Fill = null;

        List<VectorUtils.Geometry> geom = null;

        await System.Threading.Tasks.Task.Run(() => GetGeomFromScene(newScene, ref geom));

        Sprite result = null;
        if (geom.Count > 0) //This check is required because if geom is empty (can happen if it is a transparent scene)
                            //the method would throw an exception
            result = VectorUtils.BuildSprite(geom, PositionConverter.SvgPixelsPerUnit, VectorUtils.Alignment.SVGOrigin, Vector2.zero, 64, true);

        callback.Invoke(result);
         
    }


    /// <summary>
    /// Creates a sprite with a fill based on the provided patternNode and patternRect,
    /// and no stroke
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="patternNode">Vector scene node which contains the svg pattern</param>
    /// <param name="patternRect">Rect which shows which part of the scene contains the pattern</param>
    /// <returns></returns>
    public static Sprite CreatePatternFillSprite(
       Shape shape,
       SceneNode patternNode,
       Rect patternRect)
    {
        PatternFill fill = CreatePatternFill(patternNode, patternRect);

        return CreateSprite(shape, fill, null);
    }

    /// <summary>
    /// Creates a sprite with provided patternFill and no stroke
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="patternFill"></param>
    /// <returns></returns>
    public static Sprite CreatePatternFillSprite(
       Shape shape,
       PatternFill patternFill)
    {
        return CreateSprite(shape, patternFill, null);
    }

    public static Sprite CreatePatternFillSprite(
       Shape baseShape,
       Scene patternScene)
    {
        return CreatePatternSprite(baseShape, patternScene);
    }


    /// <summary>
    /// Creates a sprite with solid fill of provided color and no stroke
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="fillColor"></param>
    /// <returns></returns>
    public static Sprite CreateSolidColorFillSprite(
        Shape shape,
        Color fillColor)
    {
        SolidFill fill = CreateSolidFill(fillColor, 1);

        return CreateSprite(shape, fill, null);
    }

    public static Sprite CreateMaskSprite(Shape shape, bool addEmptyRect)
    {
        SolidFill fill = CreateSolidFill(Color.white, 1);
        //sugma
        return CreateSprite(shape, fill, null, addEmptyRect);
    }

    /// <summary>
    /// Creates a sprite with the provided texture, according size and pivot in the center
    /// </summary>
    /// <param name="texture"></param>
    /// <returns></returns>
    public static Sprite CreateTextureSprite(Texture2D texture)
    {
        return Sprite.Create(texture,
            new Rect(0f, 0f, texture.width, texture.height), 
            Vector2.one * 0.5f);
    }

    private static Mesh CreateMeshFromPoints(Vector2[] p, float height)
    {
        p = PositionConverter.ConvertPoints(p);
        Vector2 avg = Vector2.zero;
        int l = p.Length;
        height *= -PositionConverter.SvgPixelsPerUnit;
        Vector3[] vertices = new Vector3[l * 2];

        Pencil.instance.insidePolygon = p.Select(point => point / PositionConverter.SvgPixelsPerUnit).ToArray();
        for(int i = 0; i < l; i++)
        {
            avg += p[i];

            vertices[i] = new Vector3(p[i].x, p[i].y, 0) / PositionConverter.SvgPixelsPerUnit;
            vertices[l + i] = new Vector3(p[i].x, p[i].y, height) / PositionConverter.SvgPixelsPerUnit;
        }

        avg = avg / p.Length / PositionConverter.SvgPixelsPerUnit;
        Vector2 pos = new Vector2(p[1].x, p[1].y) / PositionConverter.SvgPixelsPerUnit;

        //Debug.Log("Next pos: " + pos);
        pos += (avg - pos) * 0.05f;
        //Debug.Log("Next pos + avg: " + pos); 
        Pencil.PosForNextFillShape = pos;

        int[] triangles = new int[l * 6];
        for(int i = 0; i < l; i++)
        {
            //triangle 1
            triangles[6 * i] = i;   //point i
            triangles[6 * i + 1] = (i + 1) % l; //next point on contour after i 
            triangles[6 * i + 2] = (i + l); //point above i
            //triangle 2
            triangles[6 * i + 3] = (i + 1) % l; //next point on contour after i 
            triangles[6 * i + 4] = (i + 1) % l + l; //point above next point on contour after i 
            triangles[6 * i + 5] = (i + l); //point above i
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }

    private static Mesh CreateMeshFromShape(Shape shape)
    {
        Vector2[] points = ShapeUtils.PointsFromShape(shape, 10, 1);
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vector2(points[i].x, -points[i].y);
        return CreateMeshFromPoints(points, 1);
    }

    private static Mesh CreateMeshFromBounds(Rect bounds)
    {
        Vector2[] rectBounds = new Vector2[4];
        rectBounds[0] = new Vector2(bounds.x, -bounds.y) / PositionConverter.DrawingScale;
        rectBounds[1] = new Vector2(bounds.x, -bounds.y - bounds.height) / PositionConverter.DrawingScale;
        rectBounds[2] = new Vector2(bounds.x + bounds.width, -bounds.y - bounds.height) / PositionConverter.DrawingScale;
        rectBounds[3] = new Vector2(bounds.x + bounds.width, -bounds.y) / PositionConverter.DrawingScale;

        return CreateMeshFromPoints(rectBounds, 1);
    }

    public static Mesh CreateMeshForCollider(Shape shape)
    {

        Shape newShape = null;
        Scene newScene = CreateSceneWithClonedShape(shape, out newShape, false);
     
        if (shape.Contours.Length > 1)
        {
            return CreateMeshFromBounds(VectorUtils.SceneNodeBounds(newScene.Root));
        }
        else
        {
            return CreateMeshFromShape(newShape);
        }
    }

    #endregion

}
