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

    /// <summary>
    /// Called from the Ui manager when player presses the required button
    /// </summary>
    /// <param name="color"></param>
    public void ColorSelected(Color color)
    {
        Pencil.instance.SetColorByStage(color, stateManager.gameControl.gameStageInfo.FillStageIndex);
        stateManager.SwitchState(stateManager.drawingFillState);

    }

    public override void EnterState(GameStateManager game)
    {
       
        stateManager = game;
        game.gameControl.drawingZone.SetFillPreviewSprite(game.gameControl.gameStageInfo.FillStageIndex); 
        UiControl.Instance.EnableColorSelection(game.gameControl.GetFillStageColors(), this);
        Pencil.instance.SetPencilMode(PencilMode.Inactive);
        Pencil.instance.MoveToPosForNextFillShape();
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
