using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// This is the base class for all monsters.
/// </summary>
public abstract class AI_WithAttackBehavior : AI_Base
{
    public float AttackPower = 3;
    public float AttackCooldownTime = 2.0f;
    public float AttackRange = 1.0f;

    public float MaxChaseDistance = 10.0f;

    public float TargetCheckFrequency = 10.0f;
    public float TargetCheckRadius = 5.0f;


    protected bool _IsAttacking;
    protected float _LastAttackTime;
    protected float _LastTargetCheckTime;



    protected override void InitAI()
    {
        _Health.OnTakeDamage += OnTakeDamage;

        DoTargetCheck();
    }

    protected override void UpdateAI()
    {

        if (_Target == null ||
            Time.time - _LastTargetCheckTime >= TargetCheckFrequency)
        {
            _LastTargetCheckTime = Time.time;

            DoTargetCheck();
        }


        // Check if the target is still alive.
        if (_Target)
        {
            CheckIfInAttackRange();

            // If the AI is attacking, has the attack cooldown period fully elapsed yet?
            if (_IsAttacking && Time.time - _LastAttackTime >= AttackCooldownTime)
                DoAttack();

        }
        else // _Target is null.
        {
            _IsAttacking = false;
        }

    }

    public override bool SetTarget(GameObject target)
    {
        bool result;

        if (ValidateTarget(target))
        {
            result = true;

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
                _IsAttacking = false;
            }
            else
            {
                _Target = null;
                _PrevTarget = null;
                _IsAttacking = false;
                StopMoving();
            }

        }


        if (_Target)
        {
            if (_NavMeshAgent.enabled)
                _NavMeshAgent.destination = _Target.transform.position;

            _IsAttacking = false;
        }
        else
        {
            StopMoving();
        }


        return result;
    }

    protected abstract void DoTargetCheck();

    protected virtual void CheckIfInAttackRange()
    {
        //Debug.Log($"Distance: {GetDistanceToTarget()}    Attack Range: {AttackRange}    Target: {_Target.name}");

        if (GetDistanceToTarget() <= AttackRange)
        {
            _IsAttacking = true;

            StopMoving();

            // Force the AI to face the target since they sometimes end up facing in a somewhat odd direction.
            // We set y to 0 so the AI doesn't tilt to look up if the target (such as the player) is standing on his head, which looks kind of dumb.
            transform.LookAt(new Vector3(_Target.transform.position.x, 
                                         0.0f,
                                         _Target.transform.position.z));
        }
        else
        {
            _IsAttacking = false;

            if (_NavMeshAgent.enabled)
                _NavMeshAgent.destination = _Target.transform.position;
        }
    }

    protected virtual void DoAttack()
    {
        //Debug.Log($"Attacking \"{_Target.name}\"");

        _LastAttackTime = Time.time;

        Health health = _Target != null ? _Target.GetComponent<Health>() : null;
        if (_Target && health == null)
            Debug.LogWarning($"Target \"{_Target.name}\" has no Health component!");

        if (health != null && health.CurrentHealth > 0)
        {
            health.DealDamage(AttackPower, gameObject);
            AnimateAttack();
        }

    }

    protected void OnTakeDamage(GameObject sender, GameObject attacker, float amount)
    {
        if (sender == null)
            return;

        if (attacker.CompareTag("Monster") || attacker.CompareTag("Player") || attacker.CompareTag("Villager"))
        {
            if (attacker)
                SetTarget(attacker);
        }
    }

    protected abstract void AnimateAttack();


    protected abstract void UpdateNearbyTargetDetectorState();

    protected virtual void OnTargetDeath()
    {
        UpdateNearbyTargetDetectorState();
    }



    public bool IsAttacking { get { return _IsAttacking; } }

}
