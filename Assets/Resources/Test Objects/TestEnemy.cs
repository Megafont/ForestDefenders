using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;


public class TestEnemy : MonoBehaviour
{
    public float AttackPower = 3;
    public float AttackCooldownTime = 2.0f;

    public float AI_AttackRange = 1.5f;
    public float AI_MaxPlayerChaseDistance = 10.0f;
    public float AI_TargetCheckFrequency = 5.0f;
    public float AI_TargetCheckRadius = 5.0f;

    private NavMeshAgent _NavMeshAgent;
    private GameObject _Target;
    private GameObject _PrevTarget;

    private bool _IsAttacking;
    private float _LastAttackTime;
    private float _LastTargetCheckTime;


    void Awake()
    {
        _LastTargetCheckTime = Time.time;

        _NavMeshAgent = GetComponent<NavMeshAgent>();

        GetComponent<Health>().OnDeath += OnDeath;
    }

    void Update()
    {
        if (_Target == null)
            return;


        CheckIfInAttackRange();

        if (Time.time - _LastTargetCheckTime >= AI_TargetCheckFrequency)
        {
            _LastTargetCheckTime = Time.time;

            DoTargetCheck();
        }

        if (_IsAttacking)
        {
            // Check if target has been destroyed.
            if (_Target != null)
            {
                DoAttack();            
            }
            else
            {
                _IsAttacking = false;
            }
        }
    }

    private void DoTargetCheck()
    {
        GameObject player = GameManager.Instance.Player;

        // If this enemy is chasing the player and the player gets far enough away, revert to the previous target.
        if (_Target == player &&
            Vector3.Distance(transform.position, player.transform.position) >= AI_MaxPlayerChaseDistance)
        {
            SetTarget(_PrevTarget);
        }
    }

    private void CheckIfInAttackRange()
    {
        if (Vector3.Distance(transform.position, _Target.transform.position) <= AI_AttackRange)
        {
            _IsAttacking = true;

            // Setting destination to the monster's current position prevents them from moving around the target as they bump into each other.
            _NavMeshAgent.destination = transform.position;
        }
        else
        {
            _IsAttacking = false;

            _NavMeshAgent.destination = _Target.transform.position;
        }
    }

    public void DoAttack()
    {
        if (Time.time - _LastAttackTime >= AttackCooldownTime)
        {
            // Debug.Log($"Attacking \"{_Target.name}\"");

            _Target.GetComponent<Health>().TakeDamage(AttackPower);
            _LastAttackTime = Time.time;
        }
    }

    public void SetTarget (GameObject target)
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

    private void OnDeath(GameObject sender)
    {
        Destroy(gameObject);
    }
}
