using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Abstract class representing a possible state of the game
/// Implementations of this class provide behaviours for each state of the game
/// </summary>
public abstract class GameBaseState 
{
    /// <summary>
    /// Called when the game enters this state
    /// </summary>
    /// <param name="game"></param>
    public abstract void EnterState(GameStateManager game);
    /// <summary>
    /// Called every fixed update cycle while the game is in this state
    /// </summary>
    /// <param name="game"></param>
    public abstract void UpdateState(GameStateManager game);
    /// <summary>
    /// Called when player presses down on the screen
    /// </summary>
    /// <param name="game"></param>
    public abstract void InputPressed(GameStateManager game);
    /// <summary>
    /// Called when player stops pressing on the screen
    /// </summary>
    /// <param name="game"></param>
    public abstract void InputReleased(GameStateManager game);
    /// <summary>
    /// Called when player moves finger on the screen
    /// </summary>
    /// <param name="game"></param>
    /// <param name="delta"></param>
    public abstract void InputDelta(GameStateManager game, Vector2 delta);

    public abstract void UndoRequested(GameStateManager game, GameStageInfo info);
   
}
