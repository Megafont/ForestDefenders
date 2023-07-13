using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;


public class InputMapManager_Player : InputMapManager
{
    [Header("Movement Settings")]
    [Tooltip("If enabled, the player's walking speed will depend on how far you press the joystick.")]
    public bool AnalogMovement;

    [Header("Mouse Cursor Settings")]
    public bool CursorLocked = true;
    public bool CursorInputForLook = true;



    // Input Actions
    // ----------------------------------------------------------------------------------------------------
    private InputAction _MoveAction;
    private InputAction _LookAction;
    private InputAction _JumpAction;
    private InputAction _SprintAction;
    private InputAction _AttackAction;

    private InputAction _EnterBuildModeAction;
    private InputAction _DestroyBuildingAction;
    private InputAction _OpenTechTreeAction;
    private InputAction _EndPlayerBuildPhaseAction;
    private InputAction _PauseGameAction;
    // ----------------------------------------------------------------------------------------------------



    protected override void Init()
    {
        // Get the action map this manager handles.
        _InputActionMap = _InputManager.GetPlayerInputComponent().actions.FindActionMap("Player", true);

        _MoveAction = _InputActionMap["Move"];
        _LookAction = _InputActionMap["Look"];
        _JumpAction = _InputActionMap["Jump"];
        _SprintAction = _InputActionMap["Sprint"];
        _AttackAction = _InputActionMap["Attack"];

        _EnterBuildModeAction = _InputActionMap["Enter Build Mode"];
        _DestroyBuildingAction = _InputActionMap["Destroy Building"];
        _OpenTechTreeAction = _InputActionMap["Open Tech Tree"];
        _EndPlayerBuildPhaseAction = _InputActionMap["End Player Build Phase"];
        _PauseGameAction = _InputActionMap["Pause Game"];
    }

    protected override void UpdateInputs()
    {
        Move = _MoveAction.ReadValue<Vector2>();
        Look = _LookAction.ReadValue<Vector2>();
        Jump = _JumpAction.WasPerformedThisFrame();
        Sprint = _SprintAction.IsPressed();
        Attack = _AttackAction.IsPressed();

        EnterBuildMode = _EnterBuildModeAction.WasPerformedThisFrame();
        DestroyBuilding = _DestroyBuildingAction.WasPerformedThisFrame();
        OpenTechTree = _OpenTechTreeAction.WasPerformedThisFrame();
        EndPlayerBuildPhase = _EndPlayerBuildPhaseAction.WasPerformedThisFrame();
        PauseGame = _PauseGameAction.WasPerformedThisFrame();
    }

    public override void ResetInputs()
    {
        Move = Vector2.zero;
        Look = Vector2.zero;
        Jump = false;
        Sprint = false;
        Attack = false;

        EnterBuildMode = false;
        OpenTechTree = false;
        DestroyBuilding = false;
        EndPlayerBuildPhase = false;
        PauseGame = false;
    }

    protected override void SetID()
    {
        ID = (uint) InputActionMapIDs.Player;
    }



    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(CursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }



    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool Jump { get; private set; }
    public bool Sprint { get; private set; }
    public bool Attack { get; private set; }

    public bool EnterBuildMode { get; private set; }
    public bool DestroyBuilding { get; private set; }
    public bool OpenTechTree { get; private set; }
    public bool EndPlayerBuildPhase { get; private set; }
    public bool PauseGame { get; private set; }

}
