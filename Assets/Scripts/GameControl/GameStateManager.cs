using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Object which contains references to all possible states and calls their abstract methods
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public GameControl gameControl;
    GameBaseState currentState;
    public GameStateDrawingLine drawingLineState = new GameStateDrawingLine();
    public GameStateWaitingDrawLine waitingDrawLineState = new GameStateWaitingDrawLine();
    public GameStateSelectColor selectColorState = new GameStateSelectColor();
    public GameStateDrawingFill drawingFillState = new GameStateDrawingFill();
    public GameStateFinished finishedState = new GameStateFinished();

    static TouchInput touchInput;

    public static void EnableInput()
    {
        touchInput.Enable();

    }

    public static void DisableInput()
    {
        touchInput.Disable();
    }

    private void Awake()
    {
        touchInput = new TouchInput();
        touchInput.Touch.TouchPress.started += ctx =>
        {
            //Debug.Log("PRESSED");
            currentState.InputPressed(this);
        };
        touchInput.Touch.TouchPress.canceled += ctx => currentState.InputReleased(this);
        touchInput.Touch.TouchDelta.performed += ctx =>
        {
            //Debug.Log("DELTA");
            currentState.InputDelta(this, ctx.ReadValue<Vector2>());
            
        };
        touchInput.Enable();
    }

    private void OnEnable()
    {
        touchInput.Enable();
    }

    private void OnDisable()
    {
        touchInput.Disable();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (currentState == null)
           SwitchState(waitingDrawLineState);
        currentState?.UpdateState(this);
    }

    public void SwitchState(GameBaseState newState)
    {
        currentState = newState;
        currentState.EnterState(this);
    }
}
