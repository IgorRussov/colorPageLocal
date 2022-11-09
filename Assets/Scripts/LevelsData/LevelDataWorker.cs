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
    [ReadOnly]
    public Color mainColor;
    public Color[] possibleColors;

    public FillShapeData(int fillShapeId, Color mainColor, Color[] possibleColors)
    {
        this.fillShapeId = fillShapeId;
        this.mainColor = mainColor;
        this.possibleColors = possibleColors;
    }
}

public class LevelDataWorker : MonoBehaviour
{

    public LevelEditDrawingZone drawingZone;
    public string svgFileName;
    public string nameWhenSaved;
    public string saveFolder;
    public StrokeShapeData[] strokeShapeData;
    public FillShapeData[] fillShapeData;

    private LevelData originalLevelData;

    private int colorOptionsCount = 3;
    [HideInInspector]
    public bool wantRead;
    [HideInInspector]
    public bool wantSave;

    [Header("Enter id of stroke shape to highlight")]
    public int highlightStrokeShapeId;
    [HideInInspector]
    public bool wantHighlightStroke;
    [Header("Enter id of fill shape to highlight")]
    public int highlightFillShapeId;
    [HideInInspector]
    public bool wantHighlightFill;

    private void FixedUpdate()
    {
        if (wantRead)
        {
            ReadLevelData();
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
    }

    public void ReadLevelData()
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
            List<Color32> colors = ColorUtils.GetAdjacentColors(fill.Color, colorOptionsCount, 60);
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
            fillShapeData[i] = new FillShapeData(i, originalLevelData.GetColor(i, 0),
                new Color[2]);
            fillShapeData[i].possibleColors[0] = originalLevelData.GetColor(i, 1);
            fillShapeData[i].possibleColors[1] = originalLevelData.GetColor(i, 2);
        }

        drawingZone.ShowAllDrawing(originalLevelData, strokeShapes, fillShapes);
    }

    public void SaveLevelData()
    {
        LevelData levelData = LevelData.CreateInstance<LevelData>();
        levelData.svgFileName = svgFileName;

        levelData.strokeShapesOrder = new int[strokeShapeData.Length];

        for (int i = 0; i < strokeShapeData.Length; i++)
            levelData.strokeShapesOrder[strokeShapeData[i].strokeShapeId] = 
                strokeShapeData[i].autoDraw ? 
                -1 : i;

        levelData.fillShapesOrder = new int[fillShapeData.Length];
        levelData.InitColorsArray(fillShapeData.Length, colorOptionsCount);
        for(int i = 0; i < fillShapeData.Length; i++)
        {
            int fillShapeId = fillShapeData[i].fillShapeId;
            levelData.fillShapesOrder[fillShapeId] = i;
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