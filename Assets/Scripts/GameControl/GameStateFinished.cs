using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// State for when the level is finished
/// </summary>
public class GameStateFinished : GameBaseState
{
    private float AccuracyPercent(float[] error)
    {
        float errorPercent = error.Average();
        return (1 - errorPercent);
    }

    public override void EnterState(GameStateManager game)
    {
        UiControl.Instance.ShowVictoryScreen();
        Pencil.instance.gameObject.SetActive(false);
        Pencil.instance.SetPencilMode(PencilMode.Inactive);
        Pencil.instance.MoveOffscren();
        game.gameControl.drawingZone.SetAutoFillSpritesEnabled(true);
        float accuracy = AccuracyPercent(game.gameControl.errorByStage);
        UiControl.Instance.ShowAccuracyDisplay(accuracy);
        Debug.Log("Accuracy: " + accuracy.ToString("0.00%"));

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
        //UiControl.Instance.HideFinishedText();
        game.gameControl.PreviousFillDrawStage();
        game.gameControl.drawingZone.SetAutoFillSpritesEnabled(false);

    }

    public override void UpdateState(GameStateManager game)
    {
        
    }
}
