using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;
using System;


public class GameControl : MonoBehaviour
{
    [Header("Stroke speed info")]
    public float minStrokeTime;
    public float maxStrokeTime;
    public float midStrokeTime;
    public float midStrokeLength;
    [Header("")]
    [Range(0, 1)]
    public float perfectStrokeMargin;
    public float continueLineLength;
    [Range(0, 1)]
    public float requiredFillToContinue;


    public static GameControl Instance;
    public DrawingZone drawingZone;
    public EffectsControl effectsControl;

    public GameStageInfo gameStageInfo;
    [HideInInspector]
    public bool drawingNow;
  
    /// <summary>
    /// If more of the line can be drawn
    /// </summary>
    private bool canDraw = false;

    public Action StrokeDrawStarted;
    public Action StrokeDrawStopped;
    public Action<Vector3, float> StrokeShapeFinished;

    private GameStateManager gameStateManager;

    [HideInInspector]
    public bool continuedLine;
    [HideInInspector]
    public float[] errorByStage;

    public float GetRequiredLength
    {
        get
        {
            return gameStageInfo.strokeShapeLength - (continuedLine ? continueLineLength : 0);
        }
    }

    /// <summary>
    /// If the player has finished drawing the required ammount of stroke line
    /// </summary>
    public bool CanEndDraw
    {
        get
        {
            float needLength = GetRequiredLength;
            if (gameStageInfo.drawnAmmount > needLength)
                return true;
            return WithinPerfectStroke;
               
        }
    }

    public float CurrentStrokeError
    {
        get
        {
            if (WithinPerfectStroke)
                return 0;
            float baseLength = GetRequiredLength;
            float drawn = gameStageInfo.drawnAmmount;
            float overDraw = drawn - baseLength;
            return overDraw / continueLineLength;
        }
    }

    public bool WithinPerfectStroke
    {
        get
        {
            float needLength = GetRequiredLength;
            float drawn = gameStageInfo.drawnAmmount/*+ drawingZone.drawStrokeWidth*/;

            if (drawn > needLength * (1 + perfectStrokeMargin))
                return false;
            if (drawn < needLength * (1 - perfectStrokeMargin))
                return false;
            return true;
        }
    }

    /// <summary>
    /// Returns the colors that are avaliable for the current fill stage
    /// </summary>
    /// <returns></returns>
    public Color[] GetFillStageColors()
    {
        return drawingZone.fillColors[gameStageInfo.FillStageIndex].ToArray();
    }

    private void Awake()
    {
        Instance = this;
        
        gameStateManager = gameObject.GetComponent<GameStateManager>();
        VectorUtils.tipGenerated += PositionConverter.SetTipPosition;
    }

    /// <summary>
    /// Unity start function 
    /// </summary>
    private void Start()
    {
        Pencil.instance.BindGameControlEvents(this);
        effectsControl.BindToEvents(this);
    }

    public void StartLevel(LevelData levelData)
    {
        gameStageInfo = drawingZone.SetupDrawing(levelData);
        int totalStages = gameStageInfo.fillShapesCount + gameStageInfo.strokeShapesCount;
        errorByStage = new float[totalStages];

    }


    /// <summary>
    /// Ends the drawing process of a stroke line
    /// </summary>
    private void EndStrokeLineDraw()
    {
        StrokeShapeFinished?.Invoke(drawingZone.EndLineWorldPos, CurrentStrokeError);
        if (WithinPerfectStroke)
            drawingZone.SetPerfectStrokeDrawSprite(gameStageInfo.drawStage);
        errorByStage[gameStageInfo.drawStage] = CurrentStrokeError;

        gameStageInfo.drawnAmmount = 0;
        gameStageInfo.drawStage++;
        gameStageInfo.strokeShapeLength = 0;
        canDraw = false;
        drawingZone.HideEndLineSprite();
    }

    Vector2 prevDrawShapePos;

    /// <summary>
    /// Increases the length of already drawn stroke line by drawSpeed
    /// Updates sprite and moves the pencil to show line being drawn
    /// </summary>
    public void AddStrokeLineDraw()
    {
        if (!canDraw)
            return;
       
        gameStageInfo.drawnAmmount += gameStageInfo.drawSpeed * Time.fixedDeltaTime;
        Vector2 newDrawShapePos = drawingZone.GetDrawShapePos(gameStageInfo.drawStage, gameStageInfo.DrawnPart, false);
        
        if (gameStageInfo.drawnAmmount / gameStageInfo.strokeShapeLength > 0.999f && !continuedLine)
        {
            Vector2 direction = newDrawShapePos - prevDrawShapePos;
            drawingZone.ContinueFillDrawSprite(gameStageInfo.drawStage, direction, continueLineLength);
            gameStageInfo.strokeShapeLength += continueLineLength;
            continuedLine = true;
        }
        prevDrawShapePos = newDrawShapePos;
        drawingZone.UpdateStrokeDrawSprite(gameStageInfo.drawnAmmount, gameStageInfo.drawStage);
        //Pencil.instance.InstantMove(PositionConverter.VectorPosToWorldPos(newDrawShapePos), false);
        Pencil.instance.InstantMove(PositionConverter.PencilAtTipPosition, false);
        if (gameStageInfo.MustEndDraw)
        {
            canDraw = false;
        }
    }

    /// <summary>
    /// Called when the new stroke line for drawing is shown
    /// Sets up sprites and positions pencil
    /// </summary>
    public void NextStrokeStage()
    {
        drawingZone.SetupSpritesForStrokeDrawingStage(gameStageInfo.drawStage);
        gameStageInfo.drawnAmmount = 0;
        Pencil.instance.lifted = true;
        Pencil.instance.ForcedMove(PositionConverter.VectorPosToWorldPos(
            drawingZone.GetDrawShapePos(gameStageInfo.drawStage, 0, false)
            ), false);
        PositionConverter.ResetFirstTipPosition();
        continuedLine = false;
        gameStageInfo.strokeShapeLength = drawingZone.GetDrawShapeLength(gameStageInfo.drawStage, false);
        gameStageInfo.SetDrawSpeed(minStrokeTime, maxStrokeTime, midStrokeTime, midStrokeLength, gameStageInfo.strokeShapeLength);
        //Debug.Log(gameStageInfo.strokeShapeLength);
        canDraw = true;
    }

    /// <summary>
    /// Called when the fill stage is finished
    /// Shows final fill sprite and advances draw stage
    /// </summary>
    public void FinishFillStage()
    {
        drawingZone.SetFinalFillSprite(gameStageInfo.FillStageIndex, 
            Pencil.instance.GetColorByStage(gameStageInfo.FillStageIndex));
        gameStageInfo.drawStage++;
    }

    public void StartDraw()
    {
        StrokeDrawStarted?.Invoke();
    }

    /// <summary>
    /// Fires stop drawing event and checks if the player has fininshed the current
    /// stroke line that is drawn. In case of that, calls required method.
    /// </summary>
    /// <returns>true if current drawn line is finished
    /// <br></br>
    /// false otherwise
    /// </returns>
    public bool TryEndDraw()
    {
        StrokeDrawStopped?.Invoke();
        if (CanEndDraw)
        {
            EndStrokeLineDraw();
            return true;
        }

        return false;
    }

    public void Undo()
    {
        gameStateManager.UndoRequested(gameStageInfo);
    }

    public void PreviousStrokeDrawStage()
    {
        drawingZone.HideCurrentStrokeDrawStageSprites(gameStageInfo.drawStage);
        CameraControl.Instance.RemoveTargetsOfStage(gameStageInfo.drawStage);
        gameStageInfo.drawStage--;

        gameStateManager.SwitchState(gameStateManager.waitingDrawLineState);
    }

    public void PreviousFillDrawStage()
    {
        drawingZone.HideCurrentDrawFillSprites(gameStageInfo.FillStageIndex);
        gameStageInfo.drawStage--;
        drawingZone.SetFillPreviewSprite(gameStageInfo.FillStageIndex);
        gameStateManager.SwitchState(gameStateManager.drawingFillState);

        Pencil.instance.UpdateColorRepres(gameStageInfo.FillStageIndex);
    }

    
}
