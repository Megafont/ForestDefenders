using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager_UI : InputSubManager
{
    public Vector2 Navigate;
    public bool Confirm;
    public bool Cancel;

    public bool CloseTechTree;


    protected override void Init()
    {

    }



    public void Reset()
    {
        Confirm = false;
        Cancel = false;
    }



    public void OnNavigate(InputAction.CallbackContext context)
    {
        NavigateInput(context.ReadValue<Vector2>());
    }

    public void OnConfirm(InputAction.CallbackContext context)
    {
        // NOTE: This used to be "context.control.IsPressed()". However, this apparently caused a bug where sometimes this callback would not set the
        //       input value to false when the button is released. Changing it to "context.performed" fixes this problem.
        //       This issue does not seem to affect the player button controls like attack and jump for some reason, though.
        ConfirmInput(context.performed);
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        // NOTE: This used to be "context.control.IsPressed()". However, this apparently caused a bug where sometimes this callback would not set the
        //       input value to false when the button is released. Changing it to "context.performed" fixes this problem.
        //       This issue does not seem to affect the player button controls like attack and jump for some reason, though.
        CancelInput(context.performed);
    }

    public void OnTechTree(InputAction.CallbackContext context)
    {
        // NOTE: This used to be "context.control.IsPressed()". However, this apparently caused a bug where sometimes this callback would not set the
        //       input value to false when the button is released. Changing it to "context.performed" fixes this problem.
        //       This issue does not seem to affect the player button controls like attack and jump for some reason, though.
        CloseTechTreeInput(context.performed);
    }



    private void NavigateInput(Vector2 newUINavigateDirection)
    {
        Navigate = newUINavigateDirection;
    }

    private void ConfirmInput(bool newUIConfirmState)
    {
        Confirm = newUIConfirmState;
    }

    private void CancelInput(bool newUICancelState)
    {
        Cancel = newUICancelState;
    }

    private void CloseTechTreeInput(bool newOpenTechTreeInput)
    {
        CloseTechTree = newOpenTechTreeInput;
    }


}
