using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;


public class InputMapManager_BuildMode : InputMapManager
{
    // Input Actions
    // ----------------------------------------------------------------------------------------------------
    private InputAction _MoveBuildPositionAction;
    private InputAction _RotateBuildLeftAction;
    private InputAction _RotateBuildRightAction;

    private InputAction _ConstructBuildingAction;
    private InputAction _SelectBuildingAction;
    private InputAction _ToggleGridSnapAction;
    private InputAction _ExitBuildModeAction;
    private InputAction _PauseGameAction;
    // ----------------------------------------------------------------------------------------------------



    protected override void Init()
    {
        // Get the action map this manager handles.
        _InputActionMap = _InputManager.GetPlayerInputComponent().actions.FindActionMap("Build Mode", true);

        _MoveBuildPositionAction = _InputActionMap["Move Build Position"];
        _RotateBuildLeftAction = _InputActionMap["Rotate Build Left"];
        _RotateBuildRightAction = _InputActionMap["Rotate Build Right"];

        _ConstructBuildingAction = _InputActionMap["Construct Building"];
        _SelectBuildingAction = _InputActionMap["Select Building"];
        _ToggleGridSnapAction = _InputActionMap["Toggle Grid Snap"];
        _ExitBuildModeAction = _InputActionMap["Exit Build Mode"];

        _PauseGameAction = _InputActionMap["Pause Game (Build Mode)"];
    }

    protected override void UpdateInputs()
    {
        MoveBuildPosition = _MoveBuildPositionAction.ReadValue<Vector2>();

        RotateBuildLeft = _RotateBuildLeftAction.IsPressed();
        RotateBuildRight = _RotateBuildRightAction.IsPressed();
        RotateBuildLeftReleased = _RotateBuildLeftAction.WasPerformedThisFrame();
        RotateBuildRightReleased = _RotateBuildRightAction.WasPerformedThisFrame();

        ConstructBuilding = _ConstructBuildingAction.WasPerformedThisFrame();
        SelectBuilding = _SelectBuildingAction.WasPerformedThisFrame();

        if (_ToggleGridSnapAction.WasPerformedThisFrame())
            ToggleGridSnap = !ToggleGridSnap;
                            
        ExitBuildMode = _ExitBuildModeAction.WasPerformedThisFrame();

        PauseGame = _PauseGameAction.WasPerformedThisFrame();
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
        PauseGame = false;
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

    public bool PauseGame { get; private set; }

}
