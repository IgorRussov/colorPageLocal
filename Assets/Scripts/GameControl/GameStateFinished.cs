using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// State for when the level is finished
/// </summary>
public class GameStateFinished : GameBaseState
{
    public override void EnterState(GameStateManager game)
    {
        UiControl.Instance.ShowFinishedText();
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

    public override void UpdateState(GameStateManager game)
    {
        
    }
}
