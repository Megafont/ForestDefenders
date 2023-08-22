using System;

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    using UnityEngine.InputSystem;
    // NOTE: I commented this line out because otherwise Unity will fail if you tell it to create a build of the game,
    //       since the "iOS Build Support" component is not installed as we don't need it.
    //using UnityEngine.InputSystem.iOS;
#endif

using Random = UnityEngine.Random;


/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */


/// <summary>
/// This third person character controller is a modified version of Unity's third person character controller asset.
/// </summary>
[SelectionBase]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    [SerializeField] private float _MoveSpeed = 5.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    [SerializeField] private float _SprintSpeed = 10f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    [SerializeField] private float _RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    [SerializeField] private float _SpeedChangeRate = 10.0f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    [SerializeField] private float _JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    [SerializeField] private float _Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    [SerializeField] private float _JumpTimeout = 0.30f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    [SerializeField] private float _FallTimeout = 0.15f;

        
    [Header("Player Stats")]
    [SerializeField] private float _AttackPower = 20;
    [SerializeField] private float _AttackCooldownTime = 0.5f;
    [SerializeField] private DamageTypes _DamageType = DamageTypes.Physical;
    [Tooltip("The player's fall damage is the height fallen multiplied by this value.")] 
    [SerializeField] private float _FallDamageMultiplier = 1.0f;
    [Tooltip("The player has to fall at least this many units to take fall damage.")]
    [SerializeField] private float _MinFallDamageHeight = 2.0f;


    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    [SerializeField] private bool _Grounded = true;

    [Tooltip("Useful for rough ground")]
    [SerializeField] private float _GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    [SerializeField] private float _GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    [SerializeField] private LayerMask _GroundLayers;


    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    [SerializeField] private GameObject _CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    [SerializeField] private float _TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    [SerializeField] private float _BottomClamp = -15.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    [SerializeField] private float _CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    [SerializeField] private bool _LockCameraPosition = false;



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
    private InputMapManager_Player _InputManager_Player;
    private InputMapManager_BuildMode _InputManager_BuildMode;

    // I moved the PlayerInput component into my input manager GameObject.
    //private PlayerInput _playerInput;
    //private StarterAssetsInputs _input;

    private Animator _animator;
    private CharacterController _controller;
    private GameObject _mainCamera;

    private GameManager _GameManager;
    private BuildModeManager _BuildModeManager;
    private ResourceManager _ResourceManager;
    private VillageManager_Buildings _VillageManager_Buildings;

    private Health _HealthComponent;
    private Hunger _HungerComponent;

    private const float _threshold = 0.01f;

    private bool _hasAnimator;

    private bool _IsAttacking;
    private bool _IsDead;

    private float _AttackCooldownRemainingTime;


    private SoundParams _SoundParams;
    private SoundSetPlayer _SoundSetPlayer_Attacks;
    private SoundSetPlayer _SoundSetPlayer_Footsteps;

    private float _FallStartHeight;
    private bool _IsFalling;

    private AudioSource _PlayerLandAudio;
    private ParticleSystem _PlayerMoveParticles;
    private float _PlayerMoveParticlesRate;
    private ParticleSystem _PlayerLandingParticles;



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
            _mainCamera = GameObject.FindGameObjectWithTag("Player Follow Camera");
        }


        _GameManager = GameManager.Instance;

        _HealthComponent = GetComponent<Health>();
        _HungerComponent = GetComponent<Hunger>();

        _ResourceManager = _GameManager.ResourceManager;

                                                             
        GameObject particleObj1 = gameObject.transform.Find("Move Particles").gameObject;
        if (particleObj1 == null)
            throw new Exception("Failed to find the \"Walk\\Run Particles\" child game object!");

        _PlayerMoveParticles = particleObj1.GetComponent<ParticleSystem>();
        _PlayerMoveParticlesRate = _PlayerMoveParticles.emission.rateOverDistance.constantMax; // We're its rateOverDistance property, which makes particles spawn as the particle system covers distance.


        GameObject particleObj2 = gameObject.transform.Find("Landing Particles").gameObject;
        if (particleObj2 == null)
            throw new Exception("Failed to find the \"Landing Particles\" child game object!");

        _PlayerLandingParticles = particleObj2.GetComponent<ParticleSystem>();


        _AttackPower = _GameManager.PlayerStartingAttackPower;

        InitAudio();

        _VillageManager_Buildings = _GameManager.VillageManager_Buildings;
    }

    private void Start()
    {
        _BuildModeManager = _GameManager.BuildModeManager;

        _InputManager = _GameManager.InputManager;
        _InputManager_Player = (InputMapManager_Player) _InputManager.GetInputMapManager((uint) InputActionMapIDs.Player);
        _InputManager_BuildMode = (InputMapManager_BuildMode) _InputManager.GetInputMapManager((uint)InputActionMapIDs.BuildMode);

        _cinemachineTargetYaw = _CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();


        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = _JumpTimeout;
        _fallTimeoutDelta = _FallTimeout;


        _HealthComponent.OnDeath += OnDeath;

        if (_GameManager.GodMode)
            _HealthComponent.IsInvincible = true;
    }


    private void Update()
    {
        _hasAnimator = TryGetComponent(out _animator);


        if (_InputManager_Player.PauseGame || _InputManager_BuildMode.PauseGame)
            _GameManager.TogglePauseGameState();


        DoFallDamageCheck();

        JumpAndGravity();
        GroundedCheck();
        Move();


        // Adjust the move particles emission rate depending on the player's move speed.
        if (_speed > 0)
        {
            float percent = _speed / _SprintSpeed;
            ParticleSystem.EmissionModule emission = _PlayerMoveParticles.emission;
            emission.rateOverDistanceMultiplier = percent * _PlayerMoveParticlesRate;

            //Debug.Log($"PERCENT: {percent}    RATE: {emission.rateOverDistance.constant}");
        }


        if (!_BuildModeManager.IsSelectingBuilding && !_IsDead)
        {
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

    private void InitAudio()
    {
        _SoundParams = _GameManager.SoundParams;

        _SoundSetPlayer_Attacks = gameObject.AddComponent<SoundSetPlayer>();
        _SoundSetPlayer_Attacks.SoundSet = _SoundParams.GetSoundSet("Sound Set - Player Attacks");

        _SoundSetPlayer_Footsteps = gameObject.AddComponent<SoundSetPlayer>();
        _SoundSetPlayer_Footsteps.SoundSet = _SoundParams.GetSoundSet("Sound Set - Player Footsteps");

        _PlayerLandAudio = this.AddComponent<AudioSource>();
        _PlayerLandAudio.spatialize = false; // Disable 3D sound since the player land and footstep sound is too quiet when played as a 3D sound.
        _PlayerLandAudio.clip = _SoundParams.PlayerLandingSound;
        _PlayerLandAudio.volume = _SoundParams.PlayerLandingSoundVolume;
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
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _GroundedOffset,
            transform.position.z);
        _Grounded = Physics.CheckSphere(spherePosition, _GroundedRadius, _GroundLayers, QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, _Grounded);
        }

    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_InputManager_Player.Look.sqrMagnitude >= _threshold && !_LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _InputManager_Player.Look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _InputManager_Player.Look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _BottomClamp, _TopClamp);

        // Cinemachine will follow this target
        _CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private void Move()
    {

        // Get player input. 
        // ------------------------------------------------------------------------------------------------------------------------------------------------------

        // NOTE: I modified this code, and a few lines from above and below were moved in here to put it in one place and have only one if statement.


        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _InputManager_Player.Sprint ? _SprintSpeed : _MoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (_InputManager_Player.Move == Vector2.zero) targetSpeed = 0.0f;


        float inputMagnitude = _InputManager_Player.AnalogMovement ? _InputManager_Player.Move.magnitude : 1f;
        
        // normalise input direction
        Vector3 inputDirection = new Vector3(_InputManager_Player.Move.x, 0.0f, _InputManager_Player.Move.y).normalized;

        if (!_HealthComponent.IsAlive)
        {
            targetSpeed = 0;
            inputMagnitude = 0;
            inputDirection = Vector3.zero;
        }

        // ------------------------------------------------------------------------------------------------------------------------------------------------------


        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;


        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * _SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * _SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;
        
        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (_InputManager_Player.Move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                _RotationSmoothTime);

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
        if (_Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = _FallTimeout;

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
            if (_HealthComponent.IsAlive &&
                _InputManager_Player.Jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(_JumpHeight * -2f * _Gravity);

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
            _jumpTimeoutDelta = _JumpTimeout;

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
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += _Gravity * Time.deltaTime;
        }
    }

    private void DoAttackChecks()
    {
        // Return since the player cannot attack while in midair. This could possibly be changed later, but the attack animations are ground animations. There appears to be an arial version of the spin animation, though.
        if (!_Grounded)
            return;


        // Make the cooldown timer elapse if an attack is still in progress.
        if (_AttackCooldownRemainingTime > 0)
        {
            _AttackCooldownRemainingTime -= Time.deltaTime;


            // An attack is already in progress, so return to prevent another one from being started if the user presses the attack button again.
            return;
        }


        if (_InputManager_Player.Attack)
        {
            _AttackCooldownRemainingTime = _AttackCooldownTime;
            DoAttackAction();
        }

    }

    private float foodGatheredWhileHungry;
    private void DoAttackAction()
    {
        int n = Random.Range(1, 4);

        string trigger = $"Attack {n}";
        _animator.ResetTrigger(trigger);
        _animator.SetTrigger(trigger);


        // Find all objects that the player hit.
        RaycastHit[] raycastHits = Physics.SphereCastAll(transform.position + transform.forward * 0.75f, 1.0f, transform.forward, 0.1f);


        bool hitResourceNode = false;

        // Check each object the player hit.
        foreach (RaycastHit hit in raycastHits)
        { 
            Health health = hit.collider.GetComponent<Health>();


            if (health)
            {
                if (hit.collider.CompareTag("Monster") || 
                    (_GameManager.PlayerCanDamageVillagers && hit.collider.CompareTag("Villager")))
                {
                    health.DealDamage(_AttackPower, _DamageType, gameObject);
                }
            }
            

            ResourceNode node = hit.collider.GetComponent<ResourceNode>();
            // If node is null, then check if the ResourceNode component is on the parent.
            if (node == null)
            {
                //Debug.Log("node is null! Checking parent!");
                node = hit.collider.GetComponentInParent<ResourceNode>();
            }

            // If we found a ResourceNode component and it is not depleted, then mine it.
            if (node != null && !node.IsDepleted)
            {
                hitResourceNode = true;

                //Debug.Log("node is not depleted. Mining it!");
                float gatherAmount = node.Gather(gameObject);


                if (_HungerComponent.CurrentHunger > 0 && 
                    node.ResourceType == ResourceTypes.Food)
                { 
                    EatFood(gatherAmount);
                }


                // Since we found a resource node and gathered from it, break out of this loop.
                // This prevents the player mining the same garden/farm multiple times in one hit
                // if their sword is colliding with multiple plants.
                break;
            }

        } // end foreach hit


        // If the player did not hit a resource node, then play an attack sound
        // NOTE: Different sounds are played if a resource node was hit.
        if (!hitResourceNode)
            _SoundSetPlayer_Attacks.PlayRandomSound();
        
    }

    private void EatFood(float gatherAmount)
    {
        // Track food gathered while the player is hungry.
        foodGatheredWhileHungry += gatherAmount;

        float foodCostPerHungerPoint = _HungerComponent.HungerReductionFoodAmount;

        // How many points of hunger will be alleviated this time?
        float hungerPtsToRestore = Mathf.Min(_HungerComponent.CurrentHunger, _HungerComponent.HungerPointsToHealEachTimeOnEating);
        _HungerComponent.AlleviateHunger(hungerPtsToRestore);

        // Calculate total food cost.
        float totalFoodCost = foodCostPerHungerPoint * hungerPtsToRestore;
        
        // Deduct food eaten from the player's foodGatheredWhileHungry counter.
        foodGatheredWhileHungry -= totalFoodCost;

        //Debug.Log($"Eating. Healed {hungerPtsToRestore} hunger with {totalFoodCost} food. Gather amount: {gatherAmount}");

        // Expend eaten food from the stockpile, as all gathered food was previously added to the stockpile (see next comment).
        _ResourceManager.TryToExpendFromStockpile(ResourceTypes.Food, totalFoodCost);

        // If the player's hunger is fully satiated, then clear the foodGatheredWhileHungry counter.
        // We DO NOT add excess food into the stockpile. This is because all food gathered was already
        // added to the stockpile in DoAttackAction(). The line above expends some of it from the stockpile.
        if (foodGatheredWhileHungry > 0 && _HungerComponent.CurrentHunger == 0)
            foodGatheredWhileHungry = 0;

        TextPopup.ShowTextPopup(TextPopup.AdjustStartPosition(gameObject),
                                $"Ate {totalFoodCost} Food", 
                                TextPopupColors.ExpendedResourceColor,
                                gameObject);
    }

    private void DoDestroyAction(GameObject objToDestroy)
    {
        if (objToDestroy == null)
        { 
            //Debug.LogWarning($"GameObject cannot be destroyed since it is null!");
            return;
        }


        // We need to get the object with the IBuilding component on it in case
        // the passed in object is a child of that object. This way we can destroy
        // the entire building.
        IBuilding building = objToDestroy.GetComponentInParent<IBuilding>();

        if (building != null)        
        {
            if (_VillageManager_Buildings == null)
                throw new Exception("_VillageManager_Buildings is null!");

            _VillageManager_Buildings.DeconstructBuilding(building);
        }
        else
        {
            Debug.LogError($"GameObject \"{objToDestroy.name}\" cannot be destroyed as it is not a building!");
        }
    }

    private void DoFallDamageCheck()
    {
        if (!_Grounded && !_IsFalling)
        {
            _IsFalling = true;
            _FallStartHeight = transform.position.y;
        }
        else if (_Grounded && _IsFalling)
        {
            _IsFalling = false;
            float fallEndHeight = transform.position.y;
            float fallHeight = _FallStartHeight - fallEndHeight;

            if (fallHeight >= _MinFallDamageHeight)
            {
                float fallDamage = fallHeight * _FallDamageMultiplier;
                _HealthComponent.DealDamage(fallDamage, DamageTypes.Physical, null);

                //Debug.Log($"Player took {fallDamage} fall damage.    Start height: {_FallStartHeight}    End height: {fallEndHeight}    Fall Height: {fallHeight}");
            }
        }
    }

    private void OnDeath(GameObject sender, GameObject attacker)
    {
        //Debug.Log("Player died!");


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

        if (_Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - _GroundedOffset, transform.position.z),
            _GroundedRadius);
    }



    // NOTE: The methods below are called by animation events.
    // ====================================================================================================

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
            _SoundSetPlayer_Footsteps.PlayRandomSound();
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        // I disabled this if statement since it is not needed with our player animation.
        //if (animationEvent.animatorClipInfo.weight > 0.5f)
        //{
            //AudioSource.PlayClipAtPoint(_SoundParams._PlayerLandingSound, transform.TransformPoint(_controller.center), _SoundParams._PlayerLandingSoundVolume);

            _PlayerLandAudio.spatialize = _SoundParams.PlayPlayerLandingSoundAs3DSound;
            _PlayerLandAudio.spatialBlend = _SoundParams.PlayerLandingSoundSpatialBlend;
            _PlayerLandAudio.volume = _SoundParams.PlayerLandingSoundVolume;
            _PlayerLandAudio.PlayOneShot(_SoundParams.PlayerLandingSound);

            _PlayerLandingParticles.Play();
        //}
    }



    public float AttackPower { get { return _AttackPower; } set { _AttackPower = value; } }


}