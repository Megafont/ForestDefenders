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
    public float AttackFrequency = 2.0f;

    public float MaxChaseDistance = 10.0f;

    public float TargetCheckFrequency = 5.0f;
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


        if (_Target == null ||
            Time.time - _LastTargetCheckTime >= TargetCheckFrequency)
        {
            _LastTargetCheckTime = Time.time;

            DoTargetCheck();
        }
    }

    public override bool SetTarget(GameObject target, bool discardCurrentTarget = false)
    {
        _IsAttacking = false;

        return base.SetTarget(target, discardCurrentTarget);
    }

    protected override void InteractWithTarget()
    {
        // Check if the target is still alive.
        if (_Target)
        {
            _IsAttacking = true;

            // If the AI is attacking, has the attack cooldown period fully elapsed yet?
            if (Time.time - _LastAttackTime >= AttackFrequency)
                DoAttack();
        }
        else // _Target is null.
        {
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
