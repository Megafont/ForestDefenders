using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;


public class InputMapManager_BuildMode : InputMapManager
{
    // Input Actions
    // ----------------------------------------------------------------------------------------------------
    private InputAction _MoveBuildPosition;
    private InputAction _RotateBuildLeft;
    private InputAction _RotateBuildRight;

    private InputAction _ConstructBuilding;
    private InputAction _SelectBuilding;
    private InputAction _ToggleGridSnap;
    private InputAction _ExitBuildMode;
    // ----------------------------------------------------------------------------------------------------



    protected override void Init()
    {
        // Get the action map this manager handles.
        _InputActionMap = _InputManager.GetPlayerInputComponent().actions.FindActionMap("Build Mode", true);

        _MoveBuildPosition = _InputActionMap["Move Build Position"];
        _RotateBuildLeft = _InputActionMap["Rotate Build Left"];
        _RotateBuildRight = _InputActionMap["Rotate Build Right"];

        _ConstructBuilding = _InputActionMap["Construct Building"];
        _SelectBuilding = _InputActionMap["Select Building"];
        _ToggleGridSnap = _InputActionMap["Toggle Grid Snap"];
        _ExitBuildMode = _InputActionMap["Exit Build Mode"];
    }

    protected override void UpdateInputs()
    {
        MoveBuildPosition = _MoveBuildPosition.ReadValue<Vector2>();

        RotateBuildLeft = _RotateBuildLeft.IsPressed();
        RotateBuildRight = _RotateBuildRight.IsPressed();
        RotateBuildLeftReleased = _RotateBuildLeft.WasPerformedThisFrame();
        RotateBuildRightReleased = _RotateBuildRight.WasPerformedThisFrame();

        ConstructBuilding = _ConstructBuilding.WasPerformedThisFrame();
        SelectBuilding = _SelectBuilding.WasPerformedThisFrame();

        if (_ToggleGridSnap.WasPerformedThisFrame())
            ToggleGridSnap = !ToggleGridSnap;
                            
        ExitBuildMode = _ExitBuildMode.WasPerformedThisFrame();
    }

    public override void ResetInputs()
    {
        MoveBuildPosition = Vector2.zero;
        

        RotateBuildLeftReleased = false;
        RotateBuildRightReleased = false;

        ConstructBuilding = false;
        SelectBuilding = false;
        // We skip ToggleGridSnap here because of how it is toggled in the GridSnapInput() function. This way its state is remembered between build mode sessions.
        ExitBuildMode = false;
    }

    protected override void SetID()
    {
        ID = (uint) InputActionMapIDs.BuildMode;
    }



    public Vector2 MoveBuildPosition { get; private set; }
    
    public bool RotateBuildLeft { get; private set; }
    public bool RotateBuildRight { get; private set; }
    public bool RotateBuildLeftReleased { get; private set; }
    public bool RotateBuildRightReleased { get; private set; }

    public bool ConstructBuilding { get; private set; }
    public bool SelectBuilding { get; private set; }
    public bool ToggleGridSnap { get; private set; }
    public bool ExitBuildMode { get; private set; }

}
