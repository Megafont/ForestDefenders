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



    protected override void Init()
    {

    }



    public void OnNavigate(InputAction.CallbackContext context)
    {
        NavigateInput(context.ReadValue<Vector2>());
    }

    public void OnConfirm(InputAction.CallbackContext context)
    {
        ConfirmInput(context.control.IsPressed());
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        CancelInput(context.control.IsPressed());
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


}
