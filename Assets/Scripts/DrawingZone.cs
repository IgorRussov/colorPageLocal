using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;
using UnityEditor;
using System.Linq;
using System.IO;

/// <summary>
/// Has methods required to display drawing 
/// Also contains inspector values that control drawing settings such as stroke size, color etc.
/// </summary>
public class DrawingZone : MonoBehaviour
{
    [Header("Hierarchy objects")]
    public Transform strokeShapesParentObject;
    public Transform fillShapesParentObject;
    public SpriteRenderer fillDrawSprite;
    public SpriteMask fillDrawMask;
    public GameObject shapesParentObject;
    [Header("Preview stroke settings")]
    public float previewStrokeWidth;
    public float previewStrokeFillLength;
    public float previewStrokeEmptyLength;
    public Color previewStrokeColor;
    [Header("Draw stroke settings")]
    public float drawStrokeWidth;
    public Color drawStrokeColor;
    [Header("Fill stroke settings")]
    public float fillStrokeWidth;
    [Header("Tesselation options")]
    [Range(0f, 10f)]
    public float stepDistance;
    public float maxCordDeviation;
    public float maxTanAngleDeviation;
    public float samplingStepSize;
    public float svgPixelsPerUnit;
    [Header("Files for image - must be in StreamingAssets/VectorFiles")]
    public string svgFileName;
    public string patternSvgFileName;
    [Header("Cue sprites")]
    public Sprite startLineSprite;
    public Sprite endLineSprite;


    private List<Shape> previewStrokeShapes; //Contour shapes from original image
    private List<Shape> drawStrokeShapes;    //Contour shapes from original image with line continuation added (for the late release mechanic)
    private List<Shape> fillShapes;          //Fill zone shapes from original image
    private SpriteRenderer[] drawingSprites; //References to spriteRenderer components in the hierarcy
    public List<Color>[] fillColors;         //Colors that can be used 
    private PatternFill patternFill;
    private SceneNode patternNode;
    private Rect patternRect;
    private GameObject startLineCueObject;
    private GameObject endLineCueObject;

    public static Matrix2D originalSceneMatrix;
    public static VectorUtils.TessellationOptions TesselationOptions;

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
    #region Helper functions
    /// <summary>
    /// Get total svg unit length of a draw shape with the provided index
    /// </summary>
    /// <param name="shapeOrder"></param>
    /// <returns></returns>
    public float GetDrawShapeLength(int shapeOrder)
    {
        return VectorUtils.SegmentsLength(drawStrokeShapes[shapeOrder].Contours[0].Segments,
               drawStrokeShapes[shapeOrder].IsConvex, 0.01f);
    }

    /// <summary>
    /// Gets parametric position in svg units on a draw shape with the provided index
    /// </summary>
    /// <param name="shapeIndex"></param>
    /// <param name="pos0to1"></param>
    /// <returns></returns>
    public Vector2 GetDrawShapePos(int shapeIndex, float pos0to1)
    {
        Shape shape = drawStrokeShapes[shapeIndex];
        float length = GetDrawShapeLength(shapeIndex);
        float compLength = length * pos0to1;
        return ShapeUtils.EvalShape(shape, compLength);
    }
    #endregion

    #region Setup phase
    public GameStageInfo SetupDrawing()
    {
        return PrepareData();
    }


    private GameStageInfo PrepareData()
    {
        //Set static values - prepares data
        TesselationOptions = Options;
        PositionConverter.SvgPixelsPerUnit = svgPixelsPerUnit;
        //Get pattern info
        Scene patternScene = FileIO.GetVectorSceneFromFile(patternSvgFileName);

        //patternFill = ShapeUtils.GetPatternFillFromScene(patternScene);
        patternNode = patternScene.Root;
        patternRect = VectorUtils.ApproximateSceneNodeBounds(patternScene.Root);
        //Get main drawing scene and set some values based on it
        Scene scene = FileIO.GetVectorSceneFromFile(svgFileName);
        Rect sceneRect = VectorUtils.ApproximateSceneNodeBounds(scene.Root);
        FindObjectOfType<CameraControl>().ViewRectWithCamera(sceneRect);
        shapesParentObject.transform.position = -PositionConverter.GetWorldCenterPos(sceneRect);

        List<Shape> strokeShapes = new List<Shape>();
        fillShapes = new List<Shape>();

        ShapeUtils.SetDrawingSize(scene);
        ShapeUtils.SeparateStrokeAndFill(scene, out strokeShapes, out fillShapes);
        //Creates dictionary with colors avaliable for each fill zone
        Dictionary<Shape, List<Color>> fillShapesWithColors = new Dictionary<Shape, List<Color>>();
        foreach (Shape fillShape in fillShapes)
        {
            SolidFill fill = fillShape.Fill as SolidFill;
            fillShapesWithColors.Add(fillShape, ColorUtils.GetAdjacentColors(fill.Color, 3, 60));

        }

        CreateDrawObjects(scene, strokeShapes, fillShapesWithColors);

        GameStageInfo gameStageInfo = new GameStageInfo();
        gameStageInfo.strokeShapesCount = strokeShapes.Count;
        gameStageInfo.fillShapesCount = fillShapesWithColors.Count;

        return gameStageInfo;
    }


    public void CreateDrawObjects(Scene sourceScene, List<Shape> strokeShapes, Dictionary<Shape, List<Color>> fillShapesWithColors)
    {

        //Initialize variables
        previewStrokeShapes = strokeShapes;
        originalSceneMatrix = sourceScene.Root.Transform;
        int strokeSpritesCount = strokeShapes.Count * 2;
        drawingSprites = new SpriteRenderer[strokeSpritesCount + fillShapesWithColors.Count];

        fillColors = new List<Color>[fillShapesWithColors.Count];
        for (int i = 0; i < fillShapesWithColors.Count; i++)
            fillColors[i] = fillShapesWithColors.Values.ElementAt(i);

        //Create game objects with sprites
        GameObject originalStrokeSprite = strokeShapesParentObject.GetChild(0).gameObject;
        for (int i = 0; i < strokeSpritesCount; i++)
        {
            GameObject newSprite = GameObject.Instantiate(originalStrokeSprite, strokeShapesParentObject);
            drawingSprites[i] = newSprite.GetComponent<SpriteRenderer>();
            if (i < strokeSpritesCount / 2)
                drawingSprites[i].sortingOrder = 1;
            else
                drawingSprites[i].sortingOrder = 2;
            drawingSprites[i].enabled = false;
        }
        GameObject.Destroy(originalStrokeSprite);
        GameObject originalFillSprite = fillShapesParentObject.GetChild(0).gameObject;
        for (int i = 0; i < fillShapesWithColors.Count; i++)
        {
            GameObject newSprite = GameObject.Instantiate(originalFillSprite, fillShapesParentObject);
            drawingSprites[strokeSpritesCount + i] = newSprite.GetComponent<SpriteRenderer>();
            drawingSprites[strokeSpritesCount + i].enabled = false;
        }
        GameObject.Destroy(originalFillSprite);

        //Create stroke preview sprites (пунктирные линии)
        for (int i = 0; i < strokeShapes.Count; i++)
        {
            Sprite strokePreviewSprite = DrawingSpriteFactory.CreateLineSprite(strokeShapes[i],
                ShapeUtils.CreateStrokeArray(previewStrokeFillLength, previewStrokeEmptyLength),
                previewStrokeWidth,
                previewStrokeColor);
            drawingSprites[i].sprite = strokePreviewSprite;
        }
        drawStrokeShapes = ShapeUtils.CreateDrawShapes(strokeShapes, GameControl.Instance.continueLineLength);

    }
    #endregion
    #region Update drawing sprites
    [HideInInspector]
    public Texture2D drawFillTexture; //Reference to the texture that the player draws on when drawing fill
    /// <summary>
    /// Sets mask for drawing fill,
    /// creates empty texture for the drawing fill sprite
    /// </summary>
    /// <param name="drawStageIndex"></param>
    public void SetFillPreviewSprite(int drawStageIndex)
    {
        //Fill preview sprite
        SpriteRenderer renderer = drawingSprites[previewStrokeShapes.Count * 2 + drawStageIndex];
        renderer.enabled = true;
        //renderer.sprite = ShapeUtils.CreatePatternFillSprite(
        //    fillShapes[drawStageIndex],
        //    patternFill);
        //renderer.sprite = ShapeUtils.CreatePatternFillSprite(
        //    fillShapes[drawStageIndex],
        //    patternNode, patternRect);
        //Clear fill draw texture
        drawFillTexture = ShapeUtils.CreateSceneSizedTexture(Color.grey, Vector2.one * 100, true);
        fillDrawSprite.sprite = DrawingSpriteFactory.CreateTextureSprite(drawFillTexture);

        SetMaskSprite(drawStageIndex);
    }

    /// <summary>
    /// Sets the mask sprite for drawing fill
    /// </summary>
    /// <param name="drawStageIndex"></param>
    public void SetMaskSprite(int drawStageIndex)
    {
        //Set mask sprite
        Sprite maskSprite =
            DrawingSpriteFactory.CreateSolidColorFillSprite(fillShapes[drawStageIndex], Color.white);
        fillDrawMask.sprite = maskSprite;
    }

    /// <summary>
    /// Creates colored sprite that appears after the fill stage is finished
    /// </summary>
    /// <param name="fillStageIndex"></param>
    /// <param name="color"></param>
    public void SetFinalFillSprite(int fillStageIndex, Color color)
    {
        SpriteRenderer renderer = drawingSprites[previewStrokeShapes.Count * 2 + fillStageIndex];
        renderer.sprite =
            DrawingSpriteFactory.CreateSolidColorFillSprite(fillShapes[fillStageIndex], color);
        //Clear fill draw texture
        drawFillTexture = ShapeUtils.CreateSceneSizedTexture(Color.clear, Vector2.one * 100, false);
        fillDrawSprite.sprite = DrawingSpriteFactory.CreateTextureSprite(drawFillTexture);
    }
    
    /// <summary>
    /// Enables the preview and drawing sprite and sets drawing sprite to be not filled by line 
    /// Positions sprites that show the beginning and end of the line that is drawn now
    /// </summary>
    /// <param name="drawingStage"></param>
    public void SetupSpritesForStrokeDrawingStage(int drawingStage)
    {
        if (drawingStage > 0)
            drawingSprites[drawingStage - 1].enabled = false; //Disable previous preview sprite
        //Preview sprite renderer
        drawingSprites[drawingStage].enabled = true;
        //Draw sprite renderer
        drawingSprites[previewStrokeShapes.Count + drawingStage].enabled = true;
        UpdateStrokeDrawSprite(0, drawingStage);
        
        //Create cue sprites
        Vector2 startLinePos = GetDrawShapePos(drawingStage, 0) * 0.01f;
        startLinePos = new Vector2(startLinePos.x, -startLinePos.y);
        Vector2 endLinePos = GetDrawShapePos(drawingStage, 1) * 0.01f;
        endLinePos = new Vector2(endLinePos.x, -endLinePos.y);
        startLineCueObject = CreateSpriteObject(startLineSprite, startLinePos, "Start line marker");
        endLineCueObject = CreateSpriteObject(endLineSprite, endLinePos, "End line marker");

        endLineCueObject.SetActive(false);
    }

    /// <summary>
    /// Updates the currently being drawn stroke sprite to be drawn for the required ammount
    /// </summary>
    /// <param name="drawnAmmount">Ammount of drawn stuff (in svg units) </param>
    /// <param name="drawStageIndex"></param>
    public void UpdateStrokeDrawSprite(float drawnAmmount, int drawStageIndex)
    {
        Sprite newSprite = DrawingSpriteFactory.CreateLineSprite(
           drawStrokeShapes[drawStageIndex],
           ShapeUtils.CreateStrokeArray(drawnAmmount, 100000f),
           drawStrokeWidth, drawStrokeColor);
        drawingSprites[previewStrokeShapes.Count + drawStageIndex].sprite = newSprite;
    }

    #endregion
    #region Visual functions not for drawing sprites
    /// <summary>
    /// Creates a game object with the transmited sprite, position and name
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="position"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private GameObject CreateSpriteObject(Sprite sprite, Vector2 position, string name = "Sprite")
    {
        GameObject newObject = new GameObject(name);
        SpriteRenderer spriteRenderer = newObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        newObject.transform.SetParent(transform.GetChild(0).transform, true);
        newObject.transform.localPosition = position;

        return newObject;
    }

    /// <summary>
    /// Hides the start line sprite and shows the end line sprite
    /// </summary>
    public void ShowEndLineSprite()
    {
        startLineCueObject.SetActive(false);
        endLineCueObject.SetActive(true);
    }

    /// <summary>
    /// Destroys start line and end line objects
    /// </summary>
    public void HideEndLineSprite()
    {
        GameObject.Destroy(startLineCueObject);
        GameObject.Destroy(endLineCueObject);
    }
    #endregion
}


