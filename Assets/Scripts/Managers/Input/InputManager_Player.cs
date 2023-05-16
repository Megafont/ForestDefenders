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
    public bool OpenTechTree;
    public bool DestroyBuilding;
    public bool EndBuildPhase;
    public bool Pause;


    [Header("Movement Settings")]
    [Tooltip("If enabled, the player's walking speed will depend on how far you press the joystick.")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;



    protected override void Init()
    {

    }



    public void Reset()
    {
        Attack = false;
        EnterBuildMode = false;
        OpenTechTree = false;
        DestroyBuilding = false;
        EndBuildPhase = false;
        Pause = false;
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
        // NOTE: This used to be "context.control.IsPressed()". However, this apparently caused a bug where sometimes this callback would not set the
        //       input value to false when the button is released. Changing it to "context.performed" fixes this problem.
        //       This issue does not seem to affect the player button controls like attack and jump for some reason, though.
        AttackInput(context.performed);
    }

    public void OnBuildMode(InputAction.CallbackContext context)
    {
        EnterBuildModeInput(context.control.IsPressed());
    }

    public void OnTechTree(InputAction.CallbackContext context)
    {
        // NOTE: This used to be "context.control.IsPressed()". However, this apparently caused a bug where sometimes this callback would not set the
        //       input value to false when the button is released. Changing it to "context.performed" fixes this problem.
        //       This issue does not seem to affect the player button controls like attack and jump for some reason, though.
        OpenTechTreeInput(context.performed);
    }

    public void OnDestroyBuilding(InputAction.CallbackContext context)
    {
        // We use performed here because this control has a hold interaction attached to it in the input action bindings asset.
        DestroyBuildingInput(context.performed);
    }

    public void OnEndBuildPhase(InputAction.CallbackContext context)
    {
        // We use performed here because this control has a hold interaction attached to it in the input action bindings asset.
        EndBuildPhaseInput(context.performed);
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        //Debug.Log($"C: {context.canceled}    P: {context.performed}    S: {context.started}");

        // Only call if the button was released.
        if (context.performed)
            GameManager.Instance.TogglePauseGameState();

        PauseInput(context.performed);

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

    private void OpenTechTreeInput(bool newOpenTechTreeInput)
    {
        OpenTechTree = newOpenTechTreeInput;
    }

    private void DestroyBuildingInput(bool newDestroyBuildingInput)
    {
        DestroyBuilding = newDestroyBuildingInput;
    }

    private void EndBuildPhaseInput(bool newEndBuildModeState)
    {
        EndBuildPhase = newEndBuildModeState;
    }

    private void PauseInput(bool newPauseState)
    {
        Pause = newPauseState;
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
