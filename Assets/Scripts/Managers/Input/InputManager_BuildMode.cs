using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager_BuildMode : InputSubManager
{
    public bool Build;
    public bool SelectBuilding;
    public Vector2 MoveBuildPosition;
    public bool RotateBuildLeft;
    public bool RotateBuildRight;
    public bool GridSnap;
    public bool ExitBuildMode;


    private bool _LastGridSnapState;



    public void Reset()
    {
        Build = false;
        SelectBuilding = false;
        MoveBuildPosition = Vector2.zero;
        RotateBuildLeft = false;
        RotateBuildRight = false;
        // We skip GridSnap here because of how it is toggled in the GridSnapInput() function.
        ExitBuildMode = false;
    }



    protected override void Init()
    {
        
    }


    public void OnBuild(InputAction.CallbackContext context)
    {
        // NOTE: This used to be "context.control.IsPressed()". However, this apparently caused a bug where sometimes this callback would not set the
        //       input value to false when the button is released. Changing it to "context.performed" fixes this problem.
        //       This issue does not seem to affect the player button controls like attack and jump for some reason, though.
        //       It turns out this may have been caused by Steam running in the background.
        BuildInput(context.performed);
    }

    public void OnSelectBuilding(InputAction.CallbackContext context)
    {
        SelectBuildingInput(context.control.IsPressed());
    }

    public void OnMoveBuildPosition(InputAction.CallbackContext context)
    {
        MoveBuildPositionInput(context.ReadValue<Vector2>());
    }

    public void OnRotateBuildLeft(InputAction.CallbackContext context)
    {
        RotateBuildLeftInput(context.control.IsPressed());
    }

    public void OnRotateBuildRight(InputAction.CallbackContext context)
    {
        RotateBuildRightInput(context.control.IsPressed());
    }

    public void OnGridSnap(InputAction.CallbackContext context)
    {
        // NOTE: This used to be "context.control.IsPressed()". However, this apparently caused a bug where sometimes this callback would not set the
        //       input value to false when the button is released. Changing it to "context.performed" fixes this problem.
        //       This issue does not seem to affect the player button controls like attack and jump for some reason, though.
        //       It turns out this may have been caused by Steam running in the background.
        GridSnapInput(context.performed);
    }

    public void OnExitBuildMode(InputAction.CallbackContext context)
    {
        // NOTE: This used to be "context.control.IsPressed()". However, this apparently caused a bug where sometimes this callback would not set the
        //       input value to false when the button is released. Changing it to "context.performed" fixes this problem.
        //       This issue does not seem to affect the player button controls like attack and jump for some reason, though.
        //       It turns out this may have been caused by Steam running in the background.
        ExitBuildModeInput(context.performed);
    }


    private void BuildInput(bool newBuildState)
    {
        Build = newBuildState;
    }

    private void SelectBuildingInput(bool newSelectBuildingState)
    {
        SelectBuilding = newSelectBuildingState;
    }

    private void MoveBuildPositionInput(Vector2 newMoveBuildPositionState)
    {
        MoveBuildPosition = newMoveBuildPositionState;
    }

    private void RotateBuildLeftInput(bool newRotateBuildLeftState)
    {
        RotateBuildLeft = newRotateBuildLeftState;
    }

    private void RotateBuildRightInput(bool newRotateBuildRightState)
    {
        RotateBuildRight = newRotateBuildRightState;
    }

    private void GridSnapInput(bool newGridSnapState)
    {        
        // Toggle the value with each button press.
        if (newGridSnapState && !_LastGridSnapState)
            GridSnap = !GridSnap;

        _LastGridSnapState = newGridSnapState;
    }

    private void ExitBuildModeInput(bool newExitBuildModeState)
    {
        ExitBuildMode = newExitBuildModeState;
    }

}
