using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// State for when the players sees the line to be drawn but has not started yet
/// </summary>
public class GameStateWaitingDrawLine : GameBaseState
{
    public override void EnterState(GameStateManager game)
    {
        game.gameControl.NextStrokeStage();
    }

    public override void InputDelta(GameStateManager game, Vector2 delta)
    {
        
    }

    public override void InputPressed(GameStateManager game)
    {
        game.SwitchState(game.drawingLineState);
    }

    public override void InputReleased(GameStateManager game)
    {
        
    }

    public override void UpdateState(GameStateManager game)
    {
      
    }
}
