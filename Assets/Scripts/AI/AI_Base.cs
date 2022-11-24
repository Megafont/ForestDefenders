using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// This is the base class for all villagers.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(NavMeshAgent))]
public abstract class AI_Base : MonoBehaviour
{
    // Whether or not to display the AI paths.
    protected const bool DISPLAY_AI_PATHS = true;



    public float DeathFadeOutTime = 2.0f;

    [Tooltip("This is the maximum movement speed. Make sure the walk/run thresholds are set correctly in the Animator's blend tree node, too.")]
    public float MaxMovementSpeed = 3.5f; // 3.5 is the default movement speed of NavMeshAgents.

    [Tooltip("How often (in seconds) the AI will interact with a non-combat target such as a resource node.")]
    public float InteractionFrequency = 2.0f;


    public Health HealthComponent { get { return _Health; } }


    protected Animator _Animator;
    protected Health _Health;
    protected NavMeshAgent _NavMeshAgent;

    protected GameObject _Target;
    protected GameObject _PrevTarget;

    protected bool _MovingToTargetAndIgnoreAllUntilArrived;

    protected float _InteractionRange;
    protected float _LastInteractionTime;
    protected bool _IsInteracting;

    protected WaitForSeconds _DeathFadeOutDelay;


    private void Awake()
    {
        _Animator = GetComponent<Animator>();
        _DeathFadeOutDelay = new WaitForSeconds(DeathFadeOutTime);
        _Health = GetComponent<Health>();
        _NavMeshAgent = GetComponent<NavMeshAgent>();

        _InteractionRange = _NavMeshAgent.radius + 1.0f;

        _Health.OnDeath += OnDeath;
    }


    // Start is called before the first frame update
    [ExecuteInEditMode]
    void Start()
    {
        if (Application.isPlaying)
            InitAI();

        if (MaxMovementSpeed <= 0)
            MaxMovementSpeed = _NavMeshAgent.speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_MovingToTargetAndIgnoreAllUntilArrived && HealthComponent.CurrentHealth > 0)
            UpdateAI();

        AnimateAI();
    }

    private void OnEnable()
    {
        if (_Target && _NavMeshAgent.enabled)
            _NavMeshAgent.destination = _Target.transform.position;
    }


    protected abstract void InitAI();
    protected virtual void UpdateAI()
    {
        // If the path is invalid or partial, set target to null so the AI can find a new one.
        if (_Target &&
            (_NavMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid ||
            _NavMeshAgent.pathStatus == NavMeshPathStatus.PathPartial))
        {
            // Check if the end of the path is within interaction distance of the target.
            // If not, set target to null so the AI can try to find a new reachable one.
            if (Vector3.Distance(_NavMeshAgent.pathEndPosition, _Target.transform.position) > _InteractionRange)
            {
                SetTarget(null, true);
                //Debug.Log("PATH INVALID OR PARTIAL!");
            }
        }


        // This is here in case another NavMeshAgent bumps this one away from its target.
        // This allows it to move back to the target and continue working or attacking.
        if (_Target &&
            _NavMeshAgent.isStopped &&
            _NavMeshAgent.enabled)
        {
            StartMoving();
        }


        // Is the target a moving target? If so, then update the _NavMeshAgent's destination location.
        if (_Target && 
            (_Target.CompareTag("Player") || _Target.CompareTag("Villager")))
        {
            _NavMeshAgent.SetDestination(_Target.transform.position);
        }


        if (_Target && IsWithinInteractionRange())
        {
            if (!_NavMeshAgent.isStopped)
            {
                StopMoving();

                // Force the AI to face the target since they sometimes end up facing in a somewhat odd direction.
                // We set Y to our own Y-position so the AI doesn't tilt to look up or down if the target (such as the player) is above or below it, which looks kind of dumb.
                transform.LookAt(new Vector3(_Target.transform.position.x,
                                             transform.position.y,
                                             _Target.transform.position.z));
            }

            if (Time.time - _LastInteractionTime >= InteractionFrequency)
            {
                _LastInteractionTime = Time.time;

                _IsInteracting = true;

                InteractWithTarget();
            }
        }
        else
        {
            if (_Target && _NavMeshAgent.isStopped)
                StartMoving();

            _IsInteracting = false;
        }

    }

    protected virtual void AnimateAI()
    {
        if (_Animator)
        {
            float speed = _NavMeshAgent.velocity.magnitude;
            float motionSpeed = (speed == 0) ? 1 : speed / MaxMovementSpeed; // If speed is 0 (the AI is not moving), then set motionSpeed to 1 so the idle animation will play at normal speed.

            //Debug.Log($"AI Name: {gameObject.name}    Speed: {speed}    MotionSpeed: {motionSpeed}    MaxSpeed: {MaxMovementSpeed}");

            _Animator.SetFloat("Speed", speed);
            _Animator.SetFloat("MotionSpeed", motionSpeed); // This controls the animation speed of the blend tree in the animation controller. Divide by max. run speed.
        }
    }

    public virtual void ClearTargets()
    {
        _Target = null;
        _PrevTarget = null;
    }

    /// <summary>
    /// Sets the target.
    /// </summary>
    /// <param name="target">The GameObject to set as the target.</param>
    /// <param name="discardCurrentTarget">Whether or not the current target should be discarded rather than copied into _PrevTarget.</param>
    /// <returns>True if the target was set and false otherwise.</returns>
    public virtual bool SetTarget(GameObject target, bool discardCurrentTarget = false)
    {
        bool result;


        if (ValidateTarget(target))
        {
            result = true;


            if (!discardCurrentTarget)
                _PrevTarget = _Target;

            _Target = target;
        }
        else
        {
            result = false;

            if (_Target)
            {
                // Do nothing so we keep the current target.
            }
            else if (_Target == null && _PrevTarget != null)
            {
                _Target = _PrevTarget;
                _PrevTarget = null;
            }
            else
            {
                _Target = null;
                _PrevTarget = null;
            }

        }


        if (_Target)
        {
            if (_NavMeshAgent.enabled)
            {
                // Is the target a moving target?                   
                Vector3 randomPoint = Utils_Math.GetRandomPointAroundTarget(_Target.transform);
                _NavMeshAgent.SetDestination(randomPoint);

                StartMoving();

                //Debug.Log($"Target: {_Target.transform.position}    Point: {randomPoint}");
            }
        }
        else
        {
            if (_NavMeshAgent.enabled)
                StopMoving();
        }


        _IsInteracting = false;

        return result;
    }

    /// <summary>
    /// This simply resets the destination field of the NavMeshAgent component. It is called in cases where the character's NavMeshAgent is disabled
    /// so it can be manually controlled temporarily. Knockback is one example.
    /// </summary>
    public virtual void ResetTarget()
    {
        if (_NavMeshAgent)
        {
            _NavMeshAgent.destination = _Target.transform.position;
            _NavMeshAgent.isStopped = false;
        }
    }

    public virtual bool ValidateTarget(GameObject target)
    {
        if (_NavMeshAgent == null || _Target == target)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    protected abstract void InteractWithTarget();

    protected IEnumerator MoveToTargetAndIgnoreAllElseUntilArriving(GameObject target)
    {
        if (_NavMeshAgent == null)
            yield break;

        if (!_NavMeshAgent.enabled)
            throw new Exception("MoveToTargetAndIgnoreAllElseUntilArriving() cannot be called when the NavMeshAgent component is disabled!");


        _MovingToTargetAndIgnoreAllUntilArrived = true;


        _NavMeshAgent.destination = target.transform.position;

        WaitForSeconds delay = new WaitForSeconds(1.0f);
        while (Vector3.Distance(transform.position, target.transform.position) > 3.0f)
        {
            yield return delay;
        }

        StopMoving();

        _MovingToTargetAndIgnoreAllUntilArrived = false;
    }

    protected bool IsWithinInteractionRange()
    {
        return GetDistanceToTarget() <= _InteractionRange;                                 
    }

    protected float GetDistanceToTarget()
    {
        if (_Target == null)
            return 0f;


        float distance = float.MaxValue;

       
        // The part in parantheses shifts the start position of the ray upward so it is in the center of the AI
        // character's body rather than on the ground.
        Vector3 rayStartPos = transform.position + Vector3.up * (_NavMeshAgent.height / 2);
        Vector3 rayDirection = Vector3.Normalize(_Target.transform.position - rayStartPos + new Vector3(0, 0.25f, 0));
        

        // NOTE: This commented code doesn't work right if the origin point of the target object is too hight
        //       above ground. The test tree object has this problem, because the origin is half way up the trunk.
        //       This causes the raycast to point up at a steep angle so the distance ends up longer than it
        //       should be. A villager couldn't gather from the tree as a result. I fixed this by making the
        //       test tree shorter. All my objects imported from Blender have the origin set at the base of
        //       the object, so they don't have this problem.
        //       This line is still here for testing purposes.
        //rayDirection = new Vector3(rayDirection.x, rayHeightAboveGround, rayDirection.z);
        

        if (Physics.Raycast(rayStartPos, rayDirection, out RaycastHit hitInfo, 5f))
        {
            //Debug.Log("Ray hit " + hitInfo.collider.name);

            GameObject obj = hitInfo.collider.gameObject;

            if (obj == _Target || 
                (obj.CompareTag("Building") && obj.transform.IsChildOf(_Target.transform))) // Some buildings don't have their main mesh on the top-level GameObject, so we need this secondary condition.
            {
                distance = hitInfo.distance;
            }
        }


        // Debug.Log($"Distance from target: {distance}");
        Debug.DrawLine(rayStartPos, rayStartPos + rayDirection * 5.0f);

        return distance;
    }

    protected void StopMoving()
    {
        _NavMeshAgent.isStopped = true;

        // Set this NavMeshAgent to a high priority so other agents will not ignore it since it is not moving now.
        _NavMeshAgent.avoidancePriority = 90;
    }

    protected void StartMoving()
    {
        _NavMeshAgent.isStopped = false;

        // Set this NavMeshAgent back to default priority so other moving agents will ignore it.
        _NavMeshAgent.avoidancePriority = 50;
    }


    protected virtual void OnDeath(GameObject sender)
    {
        StopMoving();

        _Animator.ResetTrigger("Die");

        // Play death animation.
        _Animator.SetTrigger("Die");


        StartCoroutine(FadeOutAfterDeath());
    }

    protected virtual IEnumerator FadeOutAfterDeath()
    {
        //Debug.Log(name + " is starting death fade out!");

        yield return _DeathFadeOutDelay;

        //Debug.Log(name + " finished death fade out!");

        Destroy(gameObject);

    }



    public bool IsInteracting { get { return _IsInteracting; } }


    public bool TargetIsBuilding { get { return _Target.GetComponent<IBuilding>() != null; } }
    public bool TargetIsMonster { get { return _Target.GetComponent<IMonster>() != null; } }
    public bool TargetIsResourceNode { get { return _Target.GetComponent<ResourceNode>() != null; } }
    public bool TargetIsVillager { get { return _Target.GetComponent<IVillager>() != null; } }

}
