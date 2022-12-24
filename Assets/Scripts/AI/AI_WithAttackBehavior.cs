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
    public float AttackFrequency = 1.0f;

    public float MaxChaseDistance = 20.0f;

    public float TargetCheckFrequency = 3.0f;
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
        base.UpdateAI();


        // Is it time for the next target check?
        if (Time.time - _LastTargetCheckTime >= TargetCheckFrequency)
        {
            _LastTargetCheckTime = Time.time;

            if (_Target != null && _NavMeshAgent.pathPending == false && _NavMeshAgent.pathStatus != NavMeshPathStatus.PathComplete)
                SetTarget(null, true);
            

            DoTargetCheck();             
        }


        if (_Target)
            DoAttackCheck();
    }

    public override bool SetTarget(GameObject target, bool discardCurrentTarget = false)
    {
        _IsAttacking = false;
        _LastAttackTime = Time.time;

        return base.SetTarget(target, discardCurrentTarget);
    }

    protected override void InteractWithTarget()
    {
        
    }

    protected virtual void DoAttackCheck()
    {
        // Check if the target is still alive.
        if (_Target && TargetIsWithinAttackRange())
        {
            _IsAttacking = true;

            // If the AI is attacking, has the attack cooldown period fully elapsed yet?
            if (Time.time - _LastAttackTime >= AttackFrequency)
                DoAttack();
        }
        else // _Target is null.
        {
            // We set this here to prevent monsters from attacking instantaneously when they first get near the player.
            // We subtract one second so the game will think the last attack was one second ago from now, thus there will only be a one second delay before the monster's first attack, rather than the normal value of AttackFrequency.
            _LastAttackTime = Time.time;

            _IsAttacking = false;
        }

    }
     
    protected abstract void DoTargetCheck();

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

    protected bool TargetIsWithinAttackRange()
    {
        return GetDistanceToTarget() <= _InteractionRange;
    }

    protected bool TargetIsWithinChaseRange()
    {
        return GetDistanceToTarget() <= MaxChaseDistance;
    }

    protected abstract void UpdateNearbyTargetDetectorState();



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


    protected virtual void OnTargetDeath()
    {
        UpdateNearbyTargetDetectorState();
    }



    public bool IsAttacking { get { return _IsAttacking; } }

}
