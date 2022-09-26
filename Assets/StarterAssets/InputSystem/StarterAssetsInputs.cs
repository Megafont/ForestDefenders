using Newtonsoft.Json.Bson;

using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
        public bool jump;
		public bool sprint;

        public bool Attack;
		public bool Build;
        public bool BuildMode;
		public bool SelectBuilding;
		public bool EndBuildMode;

		public Vector2 UI_Navigate;
		public bool UI_Confirm;
		public bool UI_Cancel;


		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;



#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		public void OnMove(InputAction.CallbackContext context)
		{
			MoveInput(context.ReadValue<Vector2>());
		}

		public void OnLook(InputAction.CallbackContext context)
		{
			if(cursorInputForLook)
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


		public void OnBuild(InputAction.CallbackContext context)
		{
			BuildInput(context.control.IsPressed());
		}

        public void OnBuildMode(InputAction.CallbackContext context)
		{
			BuildModeInput(context.control.IsPressed());
		}

		public void OnSelectBuilding(InputAction.CallbackContext context)
		{
			SelectBuildingInput(context.control.IsPressed());
		}

		public void OnEndBuildMode(InputAction.CallbackContext context)
		{
			EndBuildModeInput(context.control.IsPressed());
		}


        public void OnUINavigate(InputAction.CallbackContext context)
        {
            UI_NavigateInput(context.ReadValue<Vector2>());
        }

        public void OnUIConfirm(InputAction.CallbackContext context)
        {
			UI_ConfirmInput(context.control.IsPressed());
        }

        public void OnUICancel(InputAction.CallbackContext context)
        {
            UI_CancelInput(context.control.IsPressed());
        }
#endif



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

		private void BuildInput(bool newBuildState)
		{
			Build = newBuildState;
		}

        private void BuildModeInput(bool newBuildModeState)
        {
            BuildMode = newBuildModeState;
        }

		private void SelectBuildingInput(bool newSelectBuildingState)
		{
			SelectBuilding = newSelectBuildingState;
		}
		
		private void EndBuildModeInput(bool newEndBuildModeState)
		{
			EndBuildMode = newEndBuildModeState;
		}
		

		private void UI_NavigateInput(Vector2 newUINavigateDirection)
		{
			UI_Navigate = newUINavigateDirection;
		}

        private void UI_ConfirmInput(bool newUIConfirmState)
        {
            UI_Confirm = newUIConfirmState;
        }

        private void UI_CancelInput(bool newUICancelState)
        {
            UI_Cancel = newUICancelState;
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
	
}