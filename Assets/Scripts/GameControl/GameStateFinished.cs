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
        Pencil.instance.SetPencilMode(PencilMode.Inactive);
        Pencil.instance.MoveOffscren();
        game.gameControl.drawingZone.SetAutoFillSpritesEnabled(true);
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
        UiControl.Instance.HideFinishedText();
        game.gameControl.PreviousFillDrawStage();
        game.gameControl.drawingZone.SetAutoFillSpritesEnabled(false);

    }

    public override void UpdateState(GameStateManager game)
    {
        
    }
}
