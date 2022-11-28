 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Game state for when the player must select a color for drawing fill
/// </summary>
public class GameStateSelectColor : GameBaseState
{
    GameStateManager stateManager;

    private Color correctColor;

    /// <summary>
    /// Called from the Ui manager when player presses the required button
    /// </summary>
    /// <param name="color"></param>
    public void ColorSelected(Color color)
    {
        int fillStageIndex = stateManager.gameControl.gameStageInfo.FillStageIndex;
        Pencil.instance.SetColorByStage(color, fillStageIndex);
        stateManager.SwitchState(stateManager.drawingFillState);
        float error = color == correctColor ? 0 : 0.7f;
        stateManager.gameControl.errorByStage[stateManager.gameControl.gameStageInfo.drawStage] = error;
    }

    public override void EnterState(GameStateManager game)
    {
       
        stateManager = game;
        game.gameControl.drawingZone.SetFillPreviewSprite(game.gameControl.gameStageInfo.FillStageIndex);
        Color[] stageColors = game.gameControl.GetFillStageColors();
        correctColor = stageColors[0];
        UiControl.Instance.EnableColorSelection(stageColors, this);
        Pencil.instance.SetPencilMode(PencilMode.Inactive);
        Pencil.instance.MoveOffscren();
        Pencil.instance.lifted = false;
    }

    public override void InputDelta(GameStateManager game, Vector2 delta)
    {
        
    }

    public override void InputPressed(GameStateManager game)
    {
        
    }

    public override void InputReleased(GameStateManager game)
    {
        
    }

    public override void UndoRequested(GameStateManager game, GameStageInfo info)
    {
        info.drawStage--;
        if (info.InFillStage)
        {
            game.gameControl.PreviousFillDrawStage();
            
        }
        else
        {
            game.gameControl.drawingZone.SetAutoDrawSpritesEnabled(false);
            GameObject.FindObjectOfType<UiControl>().HideColorButtons();
            game.gameControl.gameStageInfo.drawStage--;
            game.gameControl.drawingZone.HideCurrentDrawFillSprites(0);
            game.SwitchState(game.waitingDrawLineState);
        }
    }

    public override void UpdateState(GameStateManager game)
    {
        
    }
}
