#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;
using UnityEditor;

public class LevelEditDrawingZone : MonoBehaviour
{
    [Header("Hierarchy objects")]
    public Transform strokeShapesParentObject;
    public Transform fillShapesParentObject;
    public GameObject shapesParentObject;
    [Header("Draw stroke settings")]
    public float drawStrokeWidth;
    public Color drawStrokeColor;
    [Header("Tesselation options")]
    [Range(0f, 10f)]
    public float stepDistance;
    public float maxCordDeviation;
    public float maxTanAngleDeviation;
    public float samplingStepSize;
    public float svgPixelsPerUnit;

    private SpriteRenderer[] strokeSpriteRenderers;
    private SpriteRenderer[] fillSpriteRenderers;

    private List<Shape> strokeShapes;
    private List<Shape> fillShapes;
    private LevelData levelData;

    private VectorUtils.TessellationOptions Options
    {
        get
        {
            VectorUtils.TessellationOptions options = new VectorUtils.TessellationOptions();
            options.StepDistance = stepDistance;
            options.MaxCordDeviation = maxCordDeviation;
            options.MaxTanAngleDeviation = maxTanAngleDeviation;
            options.SamplingStepSize = samplingStepSize;

            return options;
        }
    }

    private List<Vector3> handlesPositions = new List<Vector3>();
    private List<string> handlesText = new List<string>();

    public void OnDrawGizmos()
    {
        /*
        for (int i = 0; i < handlesPositions.Count; i++)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.green;
            Handles.Label(handlesPositions[i], handlesText[i], style);
        }
        */
    }

    public void ShowAllDrawing(LevelData levelData, List<Shape> strokeShapes, List<Shape> fillShapes)
    {
        this.fillShapes = fillShapes;
        this.strokeShapes = strokeShapes;
        this.levelData = levelData;

        DrawingZone.TesselationOptions = Options;
        PositionConverter.SvgPixelsPerUnit = svgPixelsPerUnit;

        //Get main drawing scene and set some values based on it
        Scene scene = FileIO.GetVectorSceneFromFile(levelData.svgFileName);
        DrawingZone.originalSceneMatrix = scene.Root.Transform;

        fillSpriteRenderers = new SpriteRenderer[fillShapes.Count];
        strokeSpriteRenderers = new SpriteRenderer[strokeShapes.Count];

        //Create fill sprites
        GameObject originalFillSprite = fillShapesParentObject.GetChild(0).gameObject;
        for (int i = 0; i < fillShapes.Count; i++)
        {
            GameObject newSprite = GameObject.Instantiate(originalFillSprite, fillShapesParentObject);
            fillSpriteRenderers[i] = newSprite.GetComponent<SpriteRenderer>();
            fillSpriteRenderers[i].sprite = DrawingSpriteFactory.CreateSolidColorFillSprite(
                fillShapes[i], levelData.GetColor(i, 0));
            Bounds bounds = fillSpriteRenderers[i].sprite.bounds;
            Vector3 pos = bounds.center;
            handlesPositions.Add(pos + Vector3.back);
            handlesText.Add("fill shape " + i);
        }
        GameObject.Destroy(originalFillSprite);

        //Create stroke sprites
        GameObject originalStrokeSprite = strokeShapesParentObject.GetChild(0).gameObject;
        for (int i = 0; i < strokeShapes.Count; i++)
        {
            GameObject newSprite = GameObject.Instantiate(originalStrokeSprite, strokeShapesParentObject);
            strokeSpriteRenderers[i] = newSprite.GetComponent<SpriteRenderer>();
            strokeSpriteRenderers[i].sprite = DrawingSpriteFactory.CreateLineSprite(
                strokeShapes[i], new float[] { 1000000, 0 }, drawStrokeWidth, drawStrokeColor);
        }
        GameObject.Destroy(originalFillSprite);
    }

    private int prevHighlightStroke = -1;
    public void HighlightStrokeShape(int strokeShapeId)
    {
        if (strokeShapeId < 0 || strokeShapeId >= strokeShapes.Count)
            return;
        if (prevHighlightStroke != -1)
            strokeSpriteRenderers[prevHighlightStroke].sprite = DrawingSpriteFactory.CreateLineSprite
            (strokeShapes[prevHighlightStroke], new float[] { 100000, 0 }, drawStrokeWidth, drawStrokeColor);
        prevHighlightStroke = strokeShapeId;
        strokeSpriteRenderers[strokeShapeId].sprite = DrawingSpriteFactory.CreateLineSprite
            (strokeShapes[strokeShapeId], new float[] { 100000, 0 }, drawStrokeWidth, Color.red);

    }

    private int prevHighlightFill = -1;
    public void HighlightFillShape(int fillShapeId)
    {
        if (fillShapeId < 0 || fillShapeId >= fillShapes.Count)
            return;
        if (prevHighlightFill != -1)
            fillSpriteRenderers[prevHighlightFill].sprite = DrawingSpriteFactory.CreateSolidColorFillSprite
                (fillShapes[prevHighlightFill], levelData.GetColor(prevHighlightFill, 0));
        prevHighlightFill = fillShapeId;
        fillSpriteRenderers[prevHighlightFill].sprite = DrawingSpriteFactory.CreateSolidColorFillSprite
                (fillShapes[prevHighlightFill], Color.red);

    }

}
#endif