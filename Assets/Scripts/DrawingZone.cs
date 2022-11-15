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
    public Transform autoStrokeShapesParentObject;
    public GameObject drawFillQuad;
    public GameObject shapesParentObject;
    public MeshCollider drawFillBoundsMeshCollider;
    public MeshCollider drawFillBoundsMeshCollider2;
    public CameraControl cameraControl;
    public SpriteMask drawFillMask;
    [Header("Image settings")]
    public float desiredSvgWidth;
    public Material spriteMaterial;
    [Header("Preview stroke settings")]
    public float previewStrokeWidth;
    public float previewStrokeFillLength;
    public float previewStrokeEmptyLength;
    public Color previewStrokeColor;
    [Header("Draw stroke settings")]
    public float drawStrokeWidth;
    public Color drawStrokeColor;
    [Header("Fill stroke settings")]
    public float fillStrokeStartRadius;
    public float fillStrokeMaxRadius;
    public float fillStrokeRadiusPerTime;
    public ComputeShader fillComputeShader;
    public ComputeShader fillPercentComputeShader;
    [Header("Tesselation options")]
    [Range(0f, 10f)]
    public float stepDistance;
    public float maxCordDeviation;
    public float maxTanAngleDeviation;
    public float samplingStepSize;
    public float svgPixelsPerUnit;
    [Header("Files for image - must be in StreamingAssets/VectorFiles")]
    //public string svgFileName;
    public string patternSvgFileName;
    [Header("Cue sprites")]
    public Sprite startLineSprite;
    public Sprite endLineSprite;


    private List<Shape> previewStrokeShapes; //Contour shapes from original image
    private List<Shape> drawStrokeShapes;    //Contour shapes from original image with line continuation added (for the late release mechanic)
    private List<Shape> autoStrokeShapes;    //Contour shapes which are drawn automaticaly after all strokes are drawn
    private List<Shape> fillShapes;          //Fill zone shapes from original image
    private SpriteRenderer[] drawingSprites; //References to spriteRenderer components in the hierarcy
    public List<Color>[] fillColors;         //Colors that can be used 
    private PatternFill patternFill;
    private SceneNode patternNode;
    private Rect patternRect;
    private GameObject startLineCueObject;
    private GameObject endLineCueObject;

    private Material drawFillQuadMaterial;

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
    public float GetDrawShapeLength(int shapeOrder, bool strokePreviewLine)
    {
        Shape shape = null;
        if (strokePreviewLine)
            shape = previewStrokeShapes[shapeOrder];
        else
            shape = drawStrokeShapes[shapeOrder];
        return VectorUtils.SegmentsLength(shape.Contours[0].Segments,
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
        float length = GetDrawShapeLength(shapeIndex, false);
        float compLength = length * pos0to1;
        return ShapeUtils.EvalShape(shape, compLength);
    }

    public Vector2 GetPreviewShapePos(int shapeIndex, float pos0to1)
    {
        Shape shape = previewStrokeShapes[shapeIndex];
        float length = GetDrawShapeLength(shapeIndex, true);
        float compLength = length * pos0to1;
        return ShapeUtils.EvalShape(shape, compLength);
    }

    

    #endregion

    #region Setup phase



    public void Awake()
    {
        ResetComputeBuffers();
        drawFillQuadMaterial = drawFillQuad.GetComponent<MeshRenderer>().material;
        SetShaderIds();
        SetShaderConstants();
    }

    public GameStageInfo SetupDrawing(LevelData levelData)
    {
        return PrepareData(levelData);
    }


    private GameStageInfo PrepareData(LevelData levelData)
    {
        //Debug.Log("Prepare data for " + levelData.svgFileName);
        //Set static values - prepares data
        TesselationOptions = Options;
        PositionConverter.SvgPixelsPerUnit = svgPixelsPerUnit;
        //Get pattern info
        //Scene patternScene = FileIO.GetVectorSceneFromFile(patternSvgFileName);

        //patternFill = ShapeUtils.GetPatternFillFromScene(patternScene);
        //patternNode = patternScene.Root;
        //patternRect = VectorUtils.ApproximateSceneNodeBounds(patternScene.Root);

        //Get main drawing scene and set some values based on it
        Scene scene = FileIO.GetVectorSceneFromFile(levelData.svgFileName);
        Rect sceneRect = VectorUtils.ApproximateSceneNodeBounds(scene.Root);
        //ShapeUtils.ScaleSceneToFit(scene, desiredSvgWidth);

        sceneRect = VectorUtils.ApproximateSceneNodeBounds(scene.Root);
        cameraControl.ViewRectWithCamera(sceneRect);
        shapesParentObject.transform.position = -PositionConverter.GetWorldCenterPos(sceneRect);
        PositionConverter.shapesObjectPos = shapesParentObject.transform.position;

        List<Shape> strokeShapes;


        ShapeUtils.SetDrawingSize(scene);
        ShapeUtils.SeparateStrokeAndFill(scene, out strokeShapes, out fillShapes);

        Shape[] sortedFillShapes = new Shape[fillShapes.Count];
        fillColors = new List<Color>[fillShapes.Count];

        for (int i = 0; i < levelData.fillShapesOrder.Length; i++)
        {
            Shape fillShape = fillShapes[i];
            List<Color> colors = levelData.GetColorsRow(i);
            int shapeSortedOrder = levelData.fillShapesOrder[i];
            sortedFillShapes[shapeSortedOrder] = fillShape;
            fillColors[shapeSortedOrder] = colors;
        }

        fillShapes = sortedFillShapes.ToList();
        Shape[] sortedStrokeShapes = new Shape[strokeShapes.Count];
        autoStrokeShapes = new List<Shape>();

        for (int i = 0; i < levelData.strokeShapesOrder.Length; i++)
        {
            Shape strokeShape = strokeShapes[i];
            int shapeSortedOrder = levelData.strokeShapesOrder[i];
            if (shapeSortedOrder == -1)
                autoStrokeShapes.Add(strokeShape);
            else
                sortedStrokeShapes[shapeSortedOrder] = strokeShape;
        }

        strokeShapes = sortedStrokeShapes.Where(s => s != null).ToList();


        CreateDrawObjects(scene, strokeShapes);

        GameStageInfo gameStageInfo = new GameStageInfo();
        gameStageInfo.strokeShapesCount = strokeShapes.Count;
        gameStageInfo.fillShapesCount = fillShapes.Count;

        return gameStageInfo;
    }


    public void CreateDrawObjects(Scene sourceScene, List<Shape> strokeShapes)
    {
        //Initialize variables
        previewStrokeShapes = strokeShapes;
        originalSceneMatrix = sourceScene.Root.Transform;
        int strokeSpritesCount = strokeShapes.Count * 2;
        drawingSprites = new SpriteRenderer[strokeSpritesCount + fillShapes.Count + autoStrokeShapes.Count];

        //Create stroke preview and stroke draw sprite renderer objects
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
        //Create fill sprites
        GameObject originalFillSprite = fillShapesParentObject.GetChild(0).gameObject;
        for (int i = 0; i < fillShapes.Count; i++)
        {
            GameObject newSprite = GameObject.Instantiate(originalFillSprite, fillShapesParentObject);
            drawingSprites[strokeSpritesCount + i] = newSprite.GetComponent<SpriteRenderer>();
            drawingSprites[strokeSpritesCount + i].enabled = false;
        }
        GameObject.Destroy(originalFillSprite);

        //Create stroke preview sprites (ïóíêòèðíûå ëèíèè)
        for (int i = 0; i < strokeShapes.Count; i++)
        {
            Sprite strokePreviewSprite = DrawingSpriteFactory.CreateLineSprite(strokeShapes[i],
                ShapeUtils.CreateStrokeArray(previewStrokeFillLength, previewStrokeEmptyLength),
                previewStrokeWidth,
                previewStrokeColor);
            drawingSprites[i].sprite = strokePreviewSprite;
        }
        //Create auto stroke sprite renderers and sprites
        GameObject originalAutoStrokeSprite = autoStrokeShapesParentObject.GetChild(0).gameObject;
        for (int i = 0; i < autoStrokeShapes.Count; i++)
        {
            GameObject newSprite = GameObject.Instantiate(originalAutoStrokeSprite, autoStrokeShapesParentObject);
            drawingSprites[strokeSpritesCount + fillShapes.Count + i] = newSprite.GetComponent<SpriteRenderer>();
            drawingSprites[strokeSpritesCount + fillShapes.Count + i].enabled = false;
            Sprite autoDrawSprite = DrawingSpriteFactory.CreateLineSprite(autoStrokeShapes[i],
                ShapeUtils.CreateStrokeArray(1000000, 0),
                drawStrokeWidth, drawStrokeColor);
            drawingSprites[strokeSpritesCount + fillShapes.Count + i].sprite = autoDrawSprite;
        }

        drawStrokeShapes = ShapeUtils.CreateDrawShapes(strokeShapes, GameControl.Instance.continueLineLength);

    }
    #endregion
    #region Update drawing sprites


    public void ContinueFillDrawSprite(int drawStageIndex, Vector2 direction, float dist)
    {
        drawStrokeShapes[drawStageIndex] = ShapeUtils.ContinueShape(previewStrokeShapes[drawStageIndex], direction, dist);
    }


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

        SetupNewDrawFillTexture();
        SetMaskSprite(drawStageIndex);
        SetDrawFillBoundsCollider(drawStageIndex);
    }

    public static Mesh CopyMesh(Mesh mesh)
    {
        Mesh newmesh = new Mesh();
        newmesh.vertices = mesh.vertices;
        newmesh.triangles = mesh.triangles;
        newmesh.uv = mesh.uv;
        newmesh.normals = mesh.normals;
        newmesh.colors = mesh.colors;
        newmesh.tangents = mesh.tangents;
        return newmesh;
    }

    public void SetDrawFillBoundsCollider(int fillStageIndex)
    {
        Shape shape = fillShapes[fillStageIndex];
        Mesh mesh = DrawingSpriteFactory.CreateMeshForCollider(shape);
        Mesh mesh2 = CopyMesh(mesh);
        mesh2.SetIndices(mesh2.GetIndices(0).Concat(mesh2.GetIndices(0).Reverse()).ToArray(), MeshTopology.Triangles, 0);
        drawFillBoundsMeshCollider.sharedMesh = mesh;
        drawFillBoundsMeshCollider2.sharedMesh = mesh2;
    }

    bool setMaskTexture = false;

    /// <summary>
    /// Sets the mask sprite for drawing fill
    /// </summary>
    /// <param name="drawStageIndex"></param>
    public void SetMaskSprite(int drawStageIndex)
    {
        //Set mask sprite
        Sprite maskSprite =
            DrawingSpriteFactory.CreateMaskSprite(fillShapes[drawStageIndex]);
        drawFillMask.sprite = maskSprite;
        Texture2D texture2d = VectorUtils.RenderSpriteToTexture2D(maskSprite, 
            Mathf.RoundToInt(ShapeUtils.drawingSize.x),
            Mathf.RoundToInt(ShapeUtils.drawingSize.y), spriteMaterial);

        drawFillQuadMaterial.SetTexture("_Alpha", texture2d);
        fillPercentComputeShader.SetTexture(kernelInit, maskTexId, texture2d);
        fillPercentComputeShader.SetTexture(kernelMain, maskTexId, texture2d);
        setMaskTexture = true;
        //drawFillQuadMaterial.SetTexture("_Alpha", 
        //    DrawingSpriteFactory.TextureFromSprite(fillShapes[drawStageIndex], maskSprite, spriteMaterial));
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
        drawFillMask.sprite = DrawingSpriteFactory.CreateTextureSprite(drawFillTexture);
    }

    public void HideCurrentDrawFillSprites(int fillStageIndex)
    {
        drawingSprites[previewStrokeShapes.Count * 2 + fillStageIndex].enabled = false;
        drawFillMask.sprite = null;
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
        drawStrokeShapes[drawingStage] = ShapeUtils.CloneShape(previewStrokeShapes[drawingStage]);
        drawingSprites[previewStrokeShapes.Count + drawingStage].enabled = true;
        drawingSprites[previewStrokeShapes.Count + drawingStage].sprite =
            DrawingSpriteFactory.CreateLineSprite(previewStrokeShapes[drawingStage],
            ShapeUtils.CreateStrokeArray(0, 10000), drawStrokeWidth, drawStrokeColor);


        //Create cue sprites
        Vector2 startLinePos = PositionConverter.VectorPosToWorldPos(GetPreviewShapePos(drawingStage, 0));
        Vector2 endLinePos = PositionConverter.VectorPosToWorldPos(GetPreviewShapePos(drawingStage, 1));

        startLineCueObject = CreateSpriteObject(startLineSprite, startLinePos, "Start line marker");
        endLineCueObject = CreateSpriteObject(endLineSprite, endLinePos, "End line marker");

        endLineCueObject.SetActive(false);
    }

    public void HideCurrentStrokeDrawStageSprites(int drawingStage)
    {
        drawingSprites[drawingStage].enabled = false;
        drawingSprites[previewStrokeShapes.Count + drawingStage].enabled = false;
        HideEndLineSprite();
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

    public void SetPerfectStrokeDrawSprite(int drawStageIndex)
    {
        Sprite newSprite = DrawingSpriteFactory.CreateLineSprite(
          previewStrokeShapes[drawStageIndex],
          ShapeUtils.CreateStrokeArray(1000000f, 0),
          drawStrokeWidth, drawStrokeColor);
        drawingSprites[previewStrokeShapes.Count + drawStageIndex].sprite = newSprite;
    }

    public void SetAutoDrawSpritesEnabled(bool enabled)
    {
        for (int i = 0; i < autoStrokeShapes.Count; i++)
        {
            drawingSprites[previewStrokeShapes.Count * 2 + fillShapes.Count + i].enabled = enabled;
        }
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
        spriteRenderer.sortingOrder = 100;
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
    #region Draw fill shader work
    private int widthId;
    private int heightId;
    private int timeId;
    private int startRadiusId;
    private int maxRadiusId;
    private int radiusPerSecondId;
    private int painterCountId;
    private int colorId;

    private int resultTextureId;
    private int setPixelsBufferId;
    private int colorPaintersBufferId;

    private int drawTexId;
    private int maskTexId;
    private int resultSumId;

    private int kernelMain;
    private int kernelInit;


    private ComputeBuffer textureSetBuffer;
    private ComputeBuffer colorPainterComputeBuffer;
    private ComputeBuffer colorPercentResultBuffer;

    private List<ColorPainter> colorPainters;

    private ComputeBuffer CreateTextureSetComputeBuffer(int textureWidth, int textureHeight)
    {
        ComputeBuffer buffer = new ComputeBuffer(textureWidth * textureHeight, 4);
        return buffer;
    }

    private ComputeBuffer CreateColorPainterComputeBuffer(int paintersNumber)
    {
        if (paintersNumber < 1)
            paintersNumber = 1;
        ComputeBuffer buffer = new ComputeBuffer(paintersNumber, ColorPainter.StructSize);
        return buffer;
    }

    private ComputeBuffer CreateColorSetResultBuffer()
    {
        ComputeBuffer buffer = new ComputeBuffer(2, sizeof(int));
        return buffer;
    }

        private void SetShaderIds()
    {
        widthId = Shader.PropertyToID("_Width");
        heightId = Shader.PropertyToID("_Height");
        heightId = Shader.PropertyToID("_Height");
        timeId = Shader.PropertyToID("_Time");
        startRadiusId = Shader.PropertyToID("_StartRadius");
        maxRadiusId = Shader.PropertyToID("_MaxRadius");
        radiusPerSecondId = Shader.PropertyToID("_RadiusPerSecond");
        painterCountId = Shader.PropertyToID("_PainterCount");
        colorId = Shader.PropertyToID("_Color");

        resultTextureId = Shader.PropertyToID("Result");
        setPixelsBufferId = Shader.PropertyToID("_SetPixels");
        colorPaintersBufferId = Shader.PropertyToID("_ColorPainters");

        drawTexId = Shader.PropertyToID("DrawTex");
        maskTexId = Shader.PropertyToID("MaskTex");

        resultSumId = Shader.PropertyToID("_ResultSum");

        kernelMain = fillPercentComputeShader.FindKernel("CSMain");
        kernelInit = fillPercentComputeShader.FindKernel("CSInit");
    }

    public void SetShaderConstants()
    {
        fillComputeShader.SetFloat(startRadiusId, fillStrokeStartRadius);
        fillComputeShader.SetFloat(maxRadiusId, fillStrokeMaxRadius);
        fillComputeShader.SetFloat(radiusPerSecondId, fillStrokeRadiusPerTime);
    }

    private void UpdateFilledPercent()
    {

    }

    public void UpdateDrawFill()
    {
        SetShaderConstants();
        fillComputeShader.SetFloat(timeId, Time.time);
        RemoveExpiredPainters();
        DispatchShader();


    }


    public void ResetComputeBuffers()
    {
        textureSetBuffer = CreateTextureSetComputeBuffer(32, 32);
        colorPainterComputeBuffer = CreateColorPainterComputeBuffer(1);
        colorPainters = new List<ColorPainter>();
        colorPercentResultBuffer = CreateColorSetResultBuffer();
    }

    public void ResetComputeBuffers(int width, int height)
    {
        textureSetBuffer = CreateTextureSetComputeBuffer(width, height);
        colorPainterComputeBuffer = CreateColorPainterComputeBuffer(10000);
        colorPainters = new List<ColorPainter>();
        colorPercentResultBuffer = CreateColorSetResultBuffer();
    }

    public Vector2 FillTextureSize
    {
        get
        {
            return new Vector2(drawFillQuadMaterial.mainTexture.width, drawFillQuadMaterial.mainTexture.height);
        }
    }

    private int counter = 0;

    private void DispatchShader()
    {
        int width = drawFillQuadMaterial.mainTexture.width / 8 + 1;
        int height = drawFillQuadMaterial.mainTexture.height / 8 + 1;

        fillComputeShader.Dispatch(0, width, height, 1);
        
        if (counter++ >= 10)
        {
            counter = 0;
            RenderTexture rt = ShapeUtils.CreateSceneSizedRenderTexture(Vector2.one * 100);
            Graphics.CopyTexture(drawFillQuadMaterial.mainTexture, rt);
            fillPercentComputeShader.SetTexture(kernelMain, drawTexId, rt);

            int[] result = new int[2];

            fillPercentComputeShader.Dispatch(kernelInit, 1, 1, 1);

            fillPercentComputeShader.Dispatch(kernelMain, width, height, 1);
            colorPercentResultBuffer.GetData(result);
            Debug.Log(result[0] + " " + result[1]);
        }
        
    }

    public void SetupNewDrawFillTexture()
    {
        RenderTexture renderTexture = ShapeUtils.CreateSceneSizedRenderTexture(Vector2.one * 100);
        drawFillQuadMaterial.mainTexture = renderTexture;
        fillComputeShader.SetTexture(0, resultTextureId, renderTexture);
        fillComputeShader.SetFloat(widthId, renderTexture.width);
        fillComputeShader.SetFloat(heightId, renderTexture.height);
        fillComputeShader.SetFloats(colorId, new float[] { 0, 0, 0, 0 });

        fillPercentComputeShader.SetFloat(widthId, renderTexture.width);
        fillPercentComputeShader.SetFloat(heightId, renderTexture.height);

        ResetComputeBuffers(renderTexture.width, renderTexture.height);
        fillComputeShader.SetBuffer(0, setPixelsBufferId, textureSetBuffer);
        fillComputeShader.SetFloat(painterCountId, 1);
        fillComputeShader.SetBuffer(0, colorPaintersBufferId, colorPainterComputeBuffer);

        fillPercentComputeShader.SetBuffer(kernelInit, resultSumId, colorPercentResultBuffer);
        fillPercentComputeShader.SetBuffer(kernelMain, resultSumId, colorPercentResultBuffer);


        RenderTexture rt = ShapeUtils.CreateSceneSizedRenderTexture(Vector2.one * 100);
        fillPercentComputeShader.SetTexture(kernelInit, drawTexId, rt);
        fillPercentComputeShader.SetTexture(kernelMain, drawTexId, rt);
        

        drawFillQuad.transform.localScale = new Vector3(renderTexture.width / PositionConverter.SvgPixelsPerUnit,
            renderTexture.height / PositionConverter.SvgPixelsPerUnit, 1);

        DispatchShader();
    }

    private float[] ColorToFloat4(Color32 color)
    {
        float[] ret = new float[4];
        ret[0] = (float)color.r / 255;
        ret[1] = (float)color.g / 255;
        ret[2] = (float)color.b / 255;
        ret[3] = (float)color.a / 255;
        return ret;
    }

    private void CreateAndSetPainterBuffer()
    {
       // Debug.Log("Create buffer: " + colorPainters.Count);
        colorPainterComputeBuffer = CreateColorPainterComputeBuffer(10000);
        colorPainterComputeBuffer.SetData(colorPainters);
        fillComputeShader.SetBuffer(0, colorPaintersBufferId, colorPainterComputeBuffer);
    }

    private void UpdatePainterBuffer()
    {
        colorPainterComputeBuffer.SetData(colorPainters);
        fillComputeShader.SetFloat(painterCountId, colorPainters.Count);
    }

    public float TimeToMaxRadius
    {
        get
        {
            return (fillStrokeMaxRadius - fillStrokeStartRadius) / fillStrokeRadiusPerTime;
        }
    }

    public void RemoveExpiredPainters()
    {
        for(int i = 0; i < colorPainters.Count; i++)
        {
            ColorPainter cp = colorPainters[i];
            if (cp.startTime + TimeToMaxRadius < Time.time)
            {
                colorPainters.Remove(cp);
                i--;
            }
        }
        UpdatePainterBuffer();
    }

    public void AddColorPainter(Vector2 pos, int fillStageId)
    {
        fillComputeShader.SetFloats(colorId, ColorToFloat4(Pencil.instance.GetColorByStage(fillStageId)));

        ColorPainter painter = new ColorPainter();
        painter.texturePos = pos;
        painter.startTime = Time.time;
        colorPainters.Add(painter);

        UpdatePainterBuffer();

    }

    #endregion
}


