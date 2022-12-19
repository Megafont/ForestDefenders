﻿using System;

using UnityEngine;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.iOS;
#endif

using Random = UnityEngine.Random;

using StarterAssets;


/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */


/// <summary>
/// This third person character controller is a modified version of Unity's third person character controller asset.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(SoundSetPlayer))]
public class PlayerController : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public AudioClip LandingAudioClip;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

        
    [Header("Player Stats")]
    public float AttackPower = 10;
    public float AttackCooldownTime = 0.5f;
       

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;


    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;



    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    // My Animation IDs
    private int _AnimAttack1;
    private int _AnimAttack2;
    private int _AnimAttack3;


    private InputManager _InputManager;
    private InputManager_Player _InputManager_Player;

    //private PlayerInput _playerInput;
    //private StarterAssetsInputs _input;

    private Animator _animator;
    private CharacterController _controller;
    private GameObject _mainCamera;

    private GameManager _GameManager;
    private BuildModeManager _BuildModeManager;
    private ResourceManager _ResourceManager;
    private VillageManager_Buildings _VillageManager_Buildings;

    private const float _threshold = 0.01f;

    private bool _hasAnimator;

    private bool _IsAttacking;
    private bool _IsDead;

    private float _AttackCooldownRemainingTime;


    private SoundSetPlayer _SoundSetPlayer;
    private SoundParams _SoundParams;



    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            return _InputManager.GetPlayerInputComponent().currentControlScheme == "KeyboardMouse";
#else
			return false;
#endif
        }
    }


    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }


        _GameManager = GameManager.Instance;

        _ResourceManager = _GameManager.ResourceManager;

        _SoundSetPlayer = GetComponent<SoundSetPlayer>();
        _SoundParams = _GameManager.SoundParams;
        _SoundSetPlayer.SoundSet = _SoundParams.GetSoundSet("Sound Set - Player Footsteps");


        _VillageManager_Buildings = _GameManager.VillageManager_Buildings;
    }

    private void Start()
    {
        _BuildModeManager = _GameManager.BuildModeManager;

        _InputManager = _GameManager.InputManager;
        _InputManager_Player = _InputManager.Player;


        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();


        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;


        GetComponent<Health>().OnDeath += OnDeath;
    }



    private void Update()
    {
        _hasAnimator = TryGetComponent(out _animator);

        if (!_BuildModeManager.IsSelectingBuilding && !_IsDead)
        {
            JumpAndGravity();
            GroundedCheck();
            Move();

            DoAttackChecks();


            if (_InputManager_Player.OpenTechTree && _GameManager.GameState == GameStates.PlayerBuildPhase && !Dialog_Base.AreAnyDialogsOpen())
            {
                _GameManager.TechTreeDialog.OpenDialog();
            }

            if (_InputManager_Player.DestroyBuilding)
            {
                if (Physics.Raycast(transform.position + Vector3.up * 0.1f,
                                transform.forward,
                                out RaycastHit hit,
                                1.0f,
                                LayerMask.GetMask("Buildings")))
                {
                    DoDestroyAction(hit.collider.gameObject);
                }
            }
}

    }

    private void LateUpdate()
    {
        if (!_BuildModeManager.IsSelectingBuilding)
            CameraRotation();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

        _AnimAttack1 = Animator.StringToHash("Attack 1");
        _AnimAttack2 = Animator.StringToHash("Attack 2");
        _AnimAttack3 = Animator.StringToHash("Attack 3");
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_InputManager.Player.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _InputManager.Player.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _InputManager.Player.look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _InputManager.Player.sprint ? SprintSpeed : MoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (_InputManager.Player.move == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _InputManager.Player.analogMovement ? _InputManager.Player.move.magnitude : 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // normalise input direction
        Vector3 inputDirection = new Vector3(_InputManager.Player.move.x, 0.0f, _InputManager.Player.move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (_InputManager.Player.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }


        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // move the player
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                            new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_InputManager.Player.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, true);
                }
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }

            // if we are not grounded, do not jump
            _InputManager.Player.jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void DoAttackChecks()
    {
        // Return since the player cannot attack while in midair. This could possibly be changed later, but the attack animations are ground animations. There appears to be an arial version of the spin animation, though.
        if (!Grounded)
            return;


        // Make the cooldown timer elapse if an attack is still in progress.
        if (_AttackCooldownRemainingTime > 0)
        {
            _AttackCooldownRemainingTime -= Time.deltaTime;


            // An attack is already in progress, so return to prevent another one from being started if the user presses the attack button again.
            return;
        }


        if (_InputManager.Player.Attack)
        {
            _AttackCooldownRemainingTime = AttackCooldownTime;
            DoAttackAction();
        }

    }

    private void DoAttackAction()
    {
        int n = Random.Range(1, 4);

        string trigger = $"Attack {n}";
        _animator.ResetTrigger(trigger);
        _animator.SetTrigger(trigger);

        RaycastHit[] raycastHits = Physics.SphereCastAll(transform.position + transform.forward * 0.75f, 1.0f, transform.forward, 0.1f);
        foreach (RaycastHit hit in raycastHits)
        { 
            Health health = hit.collider.GetComponent<Health>();


            if (health)
            {
                if (hit.collider.CompareTag("Monster") || hit.collider.CompareTag("Villager"))
                {
                    health.DealDamage(AttackPower, gameObject);
                }
            }
            

            ResourceNode node = hit.collider.GetComponent<ResourceNode>();
            if (node != null && !node.IsDepleted)
                node.Gather();

        } // end foreach hit

    }

    private void DoDestroyAction(GameObject objToDestroy)
    {
        // We need to get the object with the IBuilding component on it in case
        // the passed in object is a child of that object. This way we can destroy
        // the entire building.
        IBuilding building = objToDestroy.GetComponentInParent<IBuilding>();

        if (building != null)
        {
            _VillageManager_Buildings.DeconstructBuilding(building);
        }
        else
        {
            Debug.LogError($"GameObject \"{objToDestroy.name}\" cannot be destroyed as it is not a building!");
        }
    }

    private void OnDeath(GameObject sender)
    {
        Debug.Log("Player died!");


        _IsDead = true;

        _animator.ResetTrigger("Die");

        // Play death animation.
        _animator.SetTrigger("Die");


        //Destroy(gameObject);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }



    // NOTE: The methods below are called by animation events.
    // ====================================================================================================

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
            _SoundSetPlayer.PlaySound();
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        // I disabled this if statement since it is not needed with our player animation.
        //if (animationEvent.animatorClipInfo.weight > 0.5f)
        //{
            AudioSource.PlayClipAtPoint(_SoundParams.PlayerLandingSound, transform.TransformPoint(_controller.center), _SoundParams.PlayerLandingSoundVolume);
        //}
    }
    
}