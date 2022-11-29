#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using Unity.VectorGraphics;

public class ReadOnlyAttribute : PropertyAttribute
{

}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}

[Serializable]
public struct StrokeShapeData
{
    [ReadOnly]
    public int strokeShapeId;
    public bool autoDraw;

    public StrokeShapeData(int strokeShapeId, bool autoDraw)
    {
        this.strokeShapeId = strokeShapeId;
        this.autoDraw = autoDraw;
    }
}

[Serializable]
public struct FillShapeData
{
    [ReadOnly]
    public int fillShapeId;
    public bool autoDraw;
    [ReadOnly]
    public Color mainColor;
    public Color[] possibleColors;
    public Vector2 startPosition;

    public FillShapeData(int fillShapeId, bool autoDraw, Color mainColor, Color[] possibleColors, Vector2 startPosition)
    {
        this.fillShapeId = fillShapeId;
        this.autoDraw = autoDraw;
        this.mainColor = mainColor;
        this.possibleColors = possibleColors;
        this.startPosition = startPosition;
    }
}

public class LevelDataWorker : MonoBehaviour
{
    public void OnDrawGizmos()
    {
        foreach(FillShapeData fsd in fillShapeData)
        {
            Gizmos.color = fsd.mainColor;
            Gizmos.DrawCube(fsd.startPosition, Vector3.one * 0.3f);
        }
    }


    public LevelEditDrawingZone drawingZone;
    [Header("Create level data from svg file")]
    public string svgFileName;
    [Header("Edit existing level data")]
    public LevelData sourceLevelData;
    [Header("Edited data info")]
    public string nameWhenSaved;
    public string saveFolder;
    public StrokeShapeData[] strokeShapeData;
    public FillShapeData[] fillShapeData;

    private LevelData originalLevelData;

    private int colorOptionsCount = 3;
 
    public bool wantRead;
   
    public bool wantSave;
  
    public bool wantReadData;

    [Header("Enter id of stroke shape to highlight")]
    public int highlightStrokeShapeId;
  
    public bool wantHighlightStroke;
    [Header("Enter id of fill shape to highlight")]
    public int highlightFillShapeId;

    public bool wantHighlightFill;

    private void FixedUpdate()
    {
        if (wantRead)
        {
            ReadLevelDataFromSvg();
            wantRead = false;
        }
        if (wantSave)
        {
            SaveLevelData();
            wantSave = false;
        }
        if (wantHighlightStroke)
        {
            HighlightStrokeShape();
            wantHighlightStroke = false;
        }
        if (wantHighlightFill)
        {
            HighlightFillShape();
            wantHighlightFill = false;
        }
        if (wantReadData)
        {
            ReadExistingLevelData();
            wantReadData = false;
        }
    }

    public void ReadExistingLevelData()
    {
        Scene vectorScene = FileIO.GetVectorSceneFromFile(sourceLevelData.svgFileName);
        List<Shape> strokeShapes = new List<Shape>();
        List<Shape> fillShapes = new List<Shape>();

        ShapeUtils.SeparateStrokeAndFill(vectorScene, out strokeShapes, out fillShapes);

        originalLevelData = GameObject.Instantiate(sourceLevelData);

        nameWhenSaved = sourceLevelData.svgFileName + "_level";
        svgFileName = sourceLevelData.svgFileName;

        strokeShapeData = new StrokeShapeData[strokeShapes.Count];
        int autoDrawIndex = strokeShapes.Count - 1;
        for (int i = 0; i < strokeShapes.Count; i++)
        {
            int baseIndex = originalLevelData.strokeShapesOrder[i];
            int index = baseIndex != -1 ? baseIndex : autoDrawIndex--;
            strokeShapeData[index] = new StrokeShapeData(i, baseIndex == -1);
        }
            

        fillShapeData = new FillShapeData[fillShapes.Count];
        autoDrawIndex = fillShapes.Count - 1;
        int autoFillIndex = fillShapes.Count - 1;
        for (int i = 0; i < fillShapes.Count; i++)
        {
            int baseIndex = originalLevelData.fillShapesOrder[i];

            int index = baseIndex != -1 ? baseIndex : autoDrawIndex--;
            Vector2 startPos = Vector2.zero;
            if (originalLevelData.startPositions.Length > i)
                startPos = originalLevelData.startPositions[i];
            fillShapeData[index] = new FillShapeData(i, baseIndex == -1, originalLevelData.GetColor(i, 0),
                new Color[2], startPos);
            fillShapeData[index].possibleColors[0] = originalLevelData.GetColor(i, 1);
            fillShapeData[index].possibleColors[1] = originalLevelData.GetColor(i, 2);
        }

        drawingZone.ShowAllDrawing(originalLevelData, strokeShapes, fillShapes);
    }

    public void ReadLevelDataFromSvg()
    {
        Scene vectorScene = FileIO.GetVectorSceneFromFile(svgFileName);
        List<Shape> strokeShapes = new List<Shape>();
        List<Shape> fillShapes = new List<Shape>();

        ShapeUtils.SeparateStrokeAndFill(vectorScene, out strokeShapes, out fillShapes);

        originalLevelData = LevelData.CreateInstance<LevelData>();
        originalLevelData.svgFileName = svgFileName;
        originalLevelData.strokeShapesOrder = new int[strokeShapes.Count];
        for (int i = 0; i < originalLevelData.strokeShapesOrder.Length; i++)
            originalLevelData.strokeShapesOrder[i] = i;
        originalLevelData.fillShapesOrder = new int[fillShapes.Count];
        for (int i = 0; i < originalLevelData.fillShapesOrder.Length; i++)
            originalLevelData.fillShapesOrder[i] = i;
        originalLevelData.InitColorsArray(fillShapes.Count, colorOptionsCount);

        for(int i = 0; i < originalLevelData.fillShapesOrder.Length; i++)
        {
            SolidFill fill = fillShapes[i].Fill as SolidFill;
            List<Color32> colors = ColorUtils.GetAdjacentColors(fill.Color, colorOptionsCount, 120);
            for (int j = 0; j < colorOptionsCount; j++)
                originalLevelData.SetColor(colors[j], i, j);
        }

        nameWhenSaved = svgFileName + "_level";
        strokeShapeData = new StrokeShapeData[strokeShapes.Count];
        for (int i = 0; i < strokeShapes.Count; i++)
            strokeShapeData[i] = new StrokeShapeData(i, false);
        fillShapeData = new FillShapeData[fillShapes.Count];
        for(int i = 0; i < fillShapes.Count; i++)
        {
            fillShapeData[i] = new FillShapeData(i, false, originalLevelData.GetColor(i, 0),
                new Color[2], Vector2.zero);
            fillShapeData[i].possibleColors[0] = originalLevelData.GetColor(i, 1);
            fillShapeData[i].possibleColors[1] = originalLevelData.GetColor(i, 2);
        }

        drawingZone.ShowAllDrawing(originalLevelData, strokeShapes, fillShapes);
    }

    public void SaveLevelData()
    {
        drawingZone.StopAllHighlight();
        LevelData levelData = LevelData.CreateInstance<LevelData>();
        levelData.svgFileName = svgFileName;

        //Save snapshot
        //string path = AssetDatabase.GetAssetPath(baseImageForAssetDirectory);
        //path = path.Substring(0, path.IndexOf(path.Last(c => c == '/')));
        string path = "Assets/Resources/LevelImages";

        path = svgFileName + "_image.png";
        //Debug.Log(path);

        Camera.main.GetComponent<CameraSnapshotSaver>().RenderCameraToAsset(path);
        

        levelData.strokeShapesOrder = new int[strokeShapeData.Length];
        int autoDrawShapes = 0;
        for (int i = 0; i < strokeShapeData.Length; i++)
        {
            if (strokeShapeData[i].autoDraw)
            {
                levelData.strokeShapesOrder[strokeShapeData[i].strokeShapeId] = -1;
                autoDrawShapes++;
            }
            else
            {
                levelData.strokeShapesOrder[strokeShapeData[i].strokeShapeId] = i - autoDrawShapes;
            }
        }

        levelData.fillShapesOrder = new int[fillShapeData.Length];
        levelData.startPositions = new Vector2[fillShapeData.Length];
        levelData.InitColorsArray(fillShapeData.Length, colorOptionsCount);
        autoDrawShapes = 0;

        for(int i = 0; i < fillShapeData.Length; i++)
        {
            int fillShapeId = fillShapeData[i].fillShapeId;
            if (fillShapeData[i].autoDraw)
            {
                levelData.fillShapesOrder[fillShapeId] = -1;
                autoDrawShapes++;
            }
            else
            {
                levelData.fillShapesOrder[fillShapeId] = i -  autoDrawShapes;
            }

            levelData.startPositions[fillShapeId] = fillShapeData[i].startPosition;
            levelData.SetColor(fillShapeData[i].mainColor, fillShapeId, 0);
           
            for (int j = 1; j < colorOptionsCount; j++)
                levelData.SetColor(fillShapeData[i].possibleColors[j - 1], fillShapeId, j);

        }

        AssetDatabase.CreateAsset(levelData, FileIO.GetLevelDataPath(nameWhenSaved, saveFolder));

    }

    public void HighlightStrokeShape()
    {
        drawingZone.HighlightStrokeShape(highlightStrokeShapeId);
    }

    public void HighlightFillShape()
    {
        drawingZone.HighlightFillShape(highlightFillShapeId);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        Selection.selectionChanged += ChangedSelection;
    }
    void OnDisable()
    {
        Selection.selectionChanged -= ChangedSelection;
    }

    private void ChangedSelection()
    {
        Debug.Log(Selection.activeObject);
    }
}
#endif