using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// This is the base class for all monsters.
/// </summary>
public class Monster_Base : MonoBehaviour, IMonster
{
    public float AttackPower = 3;
    public float AttackCooldownTime = 2.0f;

    public float AI_AttackRange = 1.75f;
    public float AI_MaxPlayerChaseDistance = 10.0f;
    public float AI_TargetCheckFrequency = 5.0f;
    public float AI_TargetCheckRadius = 5.0f;

    public int ScoreValue = 10;



    protected Animator _Animator;
    protected Health _Health;
    protected NavMeshAgent _NavMeshAgent;
    protected GameObject _Target;
    protected GameObject _PrevTarget;

    protected bool _IsAttacking;
    protected float _LastAttackTime;
    protected float _LastTargetCheckTime;



    public int GetScoreValue()
    {
        return ScoreValue;
    }



    void Awake()
    {
        _LastTargetCheckTime = Time.time;

        _Animator = GetComponent<Animator>();
        _Health = GetComponent<Health>();
        _NavMeshAgent = GetComponent<NavMeshAgent>();

        GetComponent<Health>().OnDeath += OnDeath;
    }

    void Start()
    {

    }

    void Update()
    {
        UpdateMonster();
    }



    public void SetTarget(GameObject target)
    {
        if (_Target == target)
            return;


        if (target)
        {
            _PrevTarget = _Target;
            _Target = target;
            _NavMeshAgent.destination = _Target.transform.position;
        }
        else if (_Target == null)
        {
            if (_PrevTarget)
            {
                _Target = _PrevTarget;
                _PrevTarget = null;
                _NavMeshAgent.destination = _Target.transform.position;
            }
            else
            {
                _Target = null;
                _PrevTarget = null;
                _NavMeshAgent.destination = transform.position;
            }

        }

    }


    protected virtual void UpdateMonster()
    {
        if (_Health.CurrentHealth <= 0 || _Target == null)
            return;


        if (_Animator)
            _Animator.SetFloat("Speed", _NavMeshAgent.velocity.magnitude);


        CheckIfInAttackRange();

        if (Time.time - _LastTargetCheckTime >= AI_TargetCheckFrequency)
        {
            _LastTargetCheckTime = Time.time;

            DoTargetCheck();
        }

        if (_IsAttacking)
        {
            // Check if the target is still alive, and our attack cooldown period has fully elapsed.
            if (_Target != null &&
                Time.time - _LastAttackTime >= AttackCooldownTime)
            {
                DoAttack();
            }
            else
            {
                _IsAttacking = false;
            }
        }

    }

    protected virtual void DoTargetCheck()
    {
        GameObject player = GameManager.Instance.Player;

        // If this enemy is chasing the player and the player gets far enough away, revert to the previous target.
        if (_Target == player &&
            Vector3.Distance(transform.position, player.transform.position) >= AI_MaxPlayerChaseDistance)
        {
            SetTarget(_PrevTarget);
        }
    }

    protected virtual void CheckIfInAttackRange()
    {
        if (Vector3.Distance(transform.position, _Target.transform.position) <= AI_AttackRange)
        {
            _IsAttacking = true;

            // Setting destination to the monster's current position prevents them from moving around the target as they bump into each other.
            _NavMeshAgent.destination = transform.position;

            // Force the monster to face the target since they sometimes end up facing in a somewhat odd direction.
            // We set y to 0 so the monster doesn't tilt to look up if the player is on his head, which looks kind of dumb.
            transform.LookAt(new Vector3(_Target.transform.position.x, 
                                         0.0f,
                                         _Target.transform.position.y));
        }
        else
        {
            _IsAttacking = false;

            _NavMeshAgent.destination = _Target.transform.position;
        }
    }

    protected virtual void DoAttack()
    {
        // Debug.Log($"Attacking \"{_Target.name}\"");

        _Target.GetComponent<Health>().TakeDamage(AttackPower);
        _LastAttackTime = Time.time;        
    }

    protected virtual void OnDeath(GameObject sender)
    {
        _Animator.ResetTrigger("Die");

        // Play death animation.
        _Animator.SetTrigger("Die");


        StartCoroutine(FadeOutAfterDeath());
    }


    WaitForSeconds _FadeOutDelay = new WaitForSeconds(2.0f);
    protected virtual IEnumerator FadeOutAfterDeath()
    {
        yield return _FadeOutDelay;

        Destroy(gameObject);
    }

}
