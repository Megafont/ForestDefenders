using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;


public class InputMapManager_UI : InputMapManager
{
    // Input Actions
    // ----------------------------------------------------------------------------------------------------
    private InputAction _NavigateAction;

    private InputAction _SubmitAction;
    private InputAction _CancelAction;

    private InputAction _CloseTechTreeAction;
    private InputAction _UnpauseGame;
    // ----------------------------------------------------------------------------------------------------



    protected override void Init()
    {
        // Get the action map this manager handles.
        _InputActionMap = _PlayerInputComponent.actions.FindActionMap("UI", true);


        // Get the action references.
        _NavigateAction = _InputActionMap["Navigate"];

        _SubmitAction = _InputActionMap["Submit"];
        _CancelAction = _InputActionMap["Cancel"];

        _CloseTechTreeAction = _InputActionMap["Close Tech Tree"];
        _UnpauseGame = _InputActionMap["Unpause Game"];
    }

    protected override void UpdateInputs()
    {
        Navigate = _NavigateAction.ReadValue<Vector2>();

        Submit = _SubmitAction.WasPerformedThisFrame();
        Cancel = _CancelAction.WasPerformedThisFrame();

        CloseTechTree = _CloseTechTreeAction.WasPerformedThisFrame();
        UnpauseGame = _UnpauseGame.WasPerformedThisFrame();
    }


    public override void ResetInputs()
    {
        Navigate = Vector2.zero;

        Submit = false;
        Cancel = false;

        CloseTechTree = false;
    }

    protected override void SetID()
    {
        ID = (uint) InputActionMapIDs.UI;
    }



    public Vector2 Navigate { get; private set; }

    public bool Submit { get; private set; }
    public bool Cancel { get; private set; }

    public bool CloseTechTree { get; private set; }
    public bool UnpauseGame { get; private set; }

}
