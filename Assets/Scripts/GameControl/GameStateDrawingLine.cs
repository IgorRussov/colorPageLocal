using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// State for when the player is drawing a stroke line
/// </summary>
public class GameStateDrawingLine : GameBaseState
{
    private bool screenPressed = false;

    public override void EnterState(GameStateManager game)
    {
        screenPressed = true; //Game enters this state on screen press so it is true
        game.gameControl.StartDraw();
        game.gameControl.drawingZone.ShowEndLineSprite();
        Pencil.instance.SetPencilMode(PencilMode.DrawStroke);
    }

    /// <summary>
    /// Called when player releases press and checks if the drawing line is finished
    /// </summary>
    /// <param name="game"></param>
    private void PauseDrawing(GameStateManager game)
    {

        if (game.gameControl.TryEndDraw())
        { //Ended this line
            if (game.gameControl.gameStageInfo.InFillStage)
                game.SwitchState(game.selectColorState);
            else
                game.SwitchState(game.waitingDrawLineState);
        }
        else
        {

        }
    }

    public override void UpdateState(GameStateManager game)
    {
        if (screenPressed)
            game.gameControl.AddStrokeLineDraw();
    }

    public override void InputPressed(GameStateManager game)
    {
        game.gameControl.StartDraw();
        screenPressed = true;
        
    }

    public override void InputReleased(GameStateManager game)
    {
        PauseDrawing(game);
        screenPressed = false;
    }

    public override void InputDelta(GameStateManager game, Vector2 delta)
    {
        
    }
}
