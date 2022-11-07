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
    private static Scene CreateSceneWithClonedShape(Shape shape, out Shape newShape)
    {
        Scene scene = new Scene();
        SceneNode node = new SceneNode();
        scene.Root = node;
        node.Transform = DrawingZone.originalSceneMatrix;
        node.Shapes = new List<Shape>();

        newShape = CloneShape(shape);
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
        fill.Mode = FillMode.NonZero;

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
        List<VectorUtils.Geometry> geom = VectorUtils.TessellateScene(scene, DrawingZone.TesselationOptions);
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
    private static Sprite CreateSprite(Shape shape, IFill fill, Stroke stroke)
    {
        Shape newShape = null;
        Scene newScene = CreateSceneWithClonedShape(shape, out newShape);

        PathProperties props = CreatePathProperties(stroke);

        newShape.PathProps = props;
        newShape.Fill = fill;

        return GetSpriteFromScene(newScene, false);
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
        Scene newScene = CreateSceneWithClonedShape(shape, out newShape);

        Stroke original = shape.PathProps.Stroke;
        Stroke stroke = CreateStroke(dashesArray, strokeWidth, strokeColor, original);

        return CreateSprite(newShape, null, stroke);

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
    #endregion

}
