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

        public bool _Attack;
        public bool _BuildMode;

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

        public void OnBuildMode(InputAction.CallbackContext context)
		{
			BuildModeInput(context.control.IsPressed());
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


        public void AttackInput(bool newAttackState)
        {
            _Attack = newAttackState;
        }

        private void BuildModeInput(bool newBuildModeState)
        {
            _BuildMode = newBuildModeState;
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