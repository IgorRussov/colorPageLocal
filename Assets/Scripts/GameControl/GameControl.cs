using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;
using System;


public class GameControl : MonoBehaviour
{
    public float fillMoveSpeed;
    public float continueLineLength;
    public float drawSpeed;

    public static GameControl Instance;
    public DrawingZone drawingZone;

    public GameStageInfo gameStageInfo;
    [HideInInspector]
    public bool drawingNow;
  
    /// <summary>
    /// If more of the line can be drawn
    /// </summary>
    private bool canDraw = false;

    public Action StrokeDrawStarted;
    public Action StrokeDrawStopped;

    /// <summary>
    /// If the player has finished drawing the required ammount of stroke line
    /// </summary>
    public bool CanEndDraw
    {
        get
        {
            return gameStageInfo.drawnAmmount >= gameStageInfo.strokeShapeLength - continueLineLength;
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

    /// <summary>
    /// Unity start function 
    /// </summary>
    private void Start()
    {
        Instance = this;
        gameStageInfo = drawingZone.SetupDrawing();
        Pencil.instance.BindGameControlEvents(this);
    }

    /// <summary>
    /// Ends the drawing process of a stroke line
    /// </summary>
    private void EndStrokeLineDraw()
    {
        gameStageInfo.drawnAmmount = 0;
        gameStageInfo.drawStage++;
        gameStageInfo.strokeShapeLength = 0;
        canDraw = false;
        drawingZone.HideEndLineSprite();
    }

    /// <summary>
    /// Increases the length of already drawn stroke line by drawSpeed
    /// Updates sprite and moves the pencil to show line being drawn
    /// </summary>
    public void AddStrokeLineDraw()
    {
        if (!canDraw)
            return;
        gameStageInfo.drawnAmmount += drawSpeed * Time.fixedDeltaTime;
        drawingZone.UpdateStrokeDrawSprite(gameStageInfo.drawnAmmount, gameStageInfo.drawStage);
        Pencil.instance.ForcedMove(PositionConverter.VectorPosToWorldPos(
            drawingZone.GetDrawShapePos(gameStageInfo.drawStage, gameStageInfo.DrawnPart)), false);
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
        Pencil.instance.lifted = true;
        Pencil.instance.ForcedMove(PositionConverter.VectorPosToWorldPos(
            drawingZone.GetDrawShapePos(gameStageInfo.drawStage, 0)
            ), false);
        gameStageInfo.strokeShapeLength = drawingZone.GetDrawShapeLength(gameStageInfo.drawStage);
        canDraw = true;
    }

    /// <summary>
    /// Called when the fill stage is finished
    /// Shows final fill sprite and advances draw stage
    /// </summary>
    public void FinishFillStage()
    {
        drawingZone.SetFinalFillSprite(gameStageInfo.FillStageIndex, Pencil.instance.Color);
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

    
}
