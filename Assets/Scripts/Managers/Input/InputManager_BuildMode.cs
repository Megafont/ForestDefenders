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



    protected override void Init()
    {
        
    }


    public void OnBuild(InputAction.CallbackContext context)
    {
        BuildInput(context.control.IsPressed());
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
        GridSnapInput(context.control.IsPressed());
    }

    public void OnExitBuildMode(InputAction.CallbackContext context)
    {
        ExitBuildModeInput(context.control.IsPressed());
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
