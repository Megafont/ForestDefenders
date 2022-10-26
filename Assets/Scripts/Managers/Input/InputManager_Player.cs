using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager_Player : InputSubManager
{
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;

    public bool Attack;
    public bool EnterBuildMode;
    public bool EndBuildPhase;


    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;



    protected override void Init()
    {

    }



    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (cursorInputForLook)
        {
            LookInput(context.ReadValue<Vector2>());
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        JumpInput(context.control.IsPressed());
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        SprintInput(context.control.IsPressed());
    }


    public void OnAttack(InputAction.CallbackContext context)
    {
        AttackInput(context.control.IsPressed());
    }



    public void OnBuildMode(InputAction.CallbackContext context)
    {
        EnterBuildModeInput(context.control.IsPressed());
    }

    public void OnEndBuildPhase(InputAction.CallbackContext context)
    {
        // We use performed here because this control has a hold interaction attached to it in the input action bindings asset.
        EndBuildPhaseInput(context.performed);
    }



    public void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    }

    public void LookInput(Vector2 newLookDirection)
    {
        look = newLookDirection;
    }

    public void JumpInput(bool newJumpState)
    {
        jump = newJumpState;
    }

    public void SprintInput(bool newSprintState)
    {
        sprint = newSprintState;
    }



    private void AttackInput(bool newAttackState)
    {
        Attack = newAttackState;
    }

    private void EnterBuildModeInput(bool newEnterBuildModeState)
    {
        EnterBuildMode = newEnterBuildModeState;
    }

    private void EndBuildPhaseInput(bool newEndBuildModeState)
    {
        EndBuildPhase = newEndBuildModeState;
    }




    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
