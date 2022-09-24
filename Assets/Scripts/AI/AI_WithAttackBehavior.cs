using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// This is the base class for all monsters.
/// </summary>
public abstract class AI_WithAttackBehavior : AI_Base
{
    public float AttackRange = 1.0f;
    public float MaxChaseDistance = 10.0f;
    public float TargetCheckFrequency = 10.0f;
    public float TargetCheckRadius = 5.0f;


    protected bool _IsAttacking;
    protected float _LastAttackTime;
    protected float _LastTargetCheckTime;



    protected override void InitAI()
    {
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
            {
                DoAttack();
            }
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
            _NavMeshAgent.destination = _Target.transform.position;
        }
        else                
        {
            result = false;

            if (_Target == null && _PrevTarget != null)
            {
                _Target = _PrevTarget;
                _PrevTarget = null;
                _NavMeshAgent.destination = _Target.transform.position;
            }
            else if (_Target == null)
            {
                _Target = null;
                _PrevTarget = null;
                StopMoving();
            }

        }


        return result;
    }

    protected abstract void DoTargetCheck();

    protected virtual void CheckIfInAttackRange()
    {
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

            _NavMeshAgent.destination = _Target.transform.position;
        }
    }

    protected virtual void DoAttack()
    {
        // Debug.Log($"Attacking \"{_Target.name}\"");

        _Target.GetComponent<Health>().TakeDamage(AttackPower);
        _LastAttackTime = Time.time;        
    }

    protected abstract void UpdateNearbyTargetDetectorState();

    protected virtual void OnTargetDeath()
    {
        UpdateNearbyTargetDetectorState();
    }



}
