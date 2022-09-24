using System.Collections;
using System.Collections.Generic;

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
    public int AttackPower = 3;
    public float AttackCooldownTime = 2.0f;

    public float DeathFadeOutTime = 2.0f;

    [Tooltip("This is the maximum movement speed. Make sure the walk/run thresholds are set correctly in the Animator's blend tree node, too.")]
    public float MaxMovementSpeed;


    public Health HealthComponent { get { return _Health; } }


    protected Animator _Animator;
    protected Health _Health;
    protected NavMeshAgent _NavMeshAgent;

    protected GameObject _Target;
    protected GameObject _PrevTarget;

    protected bool _MovingToTargetAndIgnoreAllUntilArrived;



    private void Awake()
    {
        _Animator = GetComponent<Animator>();
        _Health = GetComponent<Health>();
        _NavMeshAgent = GetComponent<NavMeshAgent>();

        GetComponent<Health>().OnDeath += OnDeath;
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



    protected abstract void InitAI();
    protected abstract void UpdateAI();

    protected virtual void AnimateAI()
    {
        if (_Animator)
        {
            _Animator.SetFloat("Speed", _NavMeshAgent.velocity.magnitude);
            _Animator.SetFloat("MotionSpeed", _NavMeshAgent.velocity.magnitude / MaxMovementSpeed); // This controls the animation speed of the blend tree in the animation controller. Divide by max. run speed.
        }
    }

    /// <summary>
    /// Sets the target.
    /// </summary>
    /// <param name="target">The GameObject to set as the target.</param>
    /// <returns>True if the target was set and false otherwise.</returns>
    public virtual bool SetTarget(GameObject target)
    {
        if (ValidateTarget(target))
        {
            _NavMeshAgent.destination = target.transform.position;
            return true;
        }

        return false;
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

    protected IEnumerator MoveToTargetAndIgnoreAllElseUntilArriving(GameObject target)
    {
        if (_NavMeshAgent == null)
            yield break;


        _MovingToTargetAndIgnoreAllUntilArrived = true;


        _NavMeshAgent.destination = target.transform.position;

        while (Vector3.Distance(transform.position, target.transform.position) > 3.0f)
        {
            yield return new WaitForSeconds(1.0f);
        }

        StopMoving();

        _MovingToTargetAndIgnoreAllUntilArrived = false;
    }

    protected float GetDistanceToTarget()
    {
        float distance = float.MaxValue;


        Vector3 rayHeightAboveGround = Vector3.up * 0.4f;
        Vector3 rayStartPos = new Vector3(transform.position.x, rayHeightAboveGround.y, transform.position.z);
        Vector3 rayDirection = Vector3.Normalize(_Target.transform.position - transform.position);


        if (Physics.Raycast(rayStartPos, rayDirection, out RaycastHit hitInfo, 5f))
        {
            //Debug.Log("Ray hit " + hitInfo.collider.name);

            if (hitInfo.collider.gameObject == _Target)
            {
                distance = hitInfo.distance;
            }
        }


        //Debug.DrawLine(rayStartPos, rayStartPos + rayDirection * 5.0f);
        //Debug.Log($"Distance from target: {distance}");


        return distance;
    }

    protected void StopMoving()
    {
        // Set the NavMeshAgent's destination to its current position to make it stop moving.
        _NavMeshAgent.destination = transform.position;
    }


    protected virtual void OnDeath(GameObject sender)
    {
        _Animator.ResetTrigger("Die");

        // Play death animation.
        _Animator.SetTrigger("Die");


        StartCoroutine(FadeOutAfterDeath());
    }

    protected virtual IEnumerator FadeOutAfterDeath()
    {
        yield return new WaitForSeconds(DeathFadeOutTime);

        Destroy(gameObject);

        yield break;
    }

}
