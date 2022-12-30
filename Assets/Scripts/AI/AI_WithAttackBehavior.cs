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
    public float AttackCheckFrequency = 1.0f;

    public float MaxChaseDistance = 20.0f;

    public float TargetCheckFrequency = 3.0f;
    public float TargetCheckRadius = 5.0f;


    protected bool _IsAttacking;
    protected float _LastAttackTime;
    protected float _LastTargetCheckTime;

    protected int[] _AttackAnimationNameHashes;
    protected int _HashOfPlayingAttackAnim = -1;



    protected override void InitAI()
    {
        InitAttackAnimationsNameHashTable();

        _Health.OnTakeDamage += OnTakeDamage;

        DoTargetCheck();
    }

    protected override void UpdateAI()
    {
        base.UpdateAI();


        // Is an attack animation in progress?
        if (_HashOfPlayingAttackAnim >= 0)
        {
            // Has the attack animation state ended? If so, reset _HashOfPlayingAttackAnim to -1 to indicate that the AI is no longer playing an attack animation.
            if (_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != _HashOfPlayingAttackAnim)
                _HashOfPlayingAttackAnim = -1;
        }


        // Is it time for the next target check?
        if (Time.time - _LastTargetCheckTime >= TargetCheckFrequency)
        {
            _LastTargetCheckTime = Time.time;

            if (!_IsAttacking && _Target == null)
                DoTargetCheck();             
        }


        // This function checks if target is not null, so we don't do so before calling it.
        DoAttackCheck();

    }

    private void InitAttackAnimationsNameHashTable()
    {
        int length = 5;
        _AttackAnimationNameHashes = new int[length];

        for (int i = 0; i < length; i++)
        {
            _AttackAnimationNameHashes[i] = Animator.StringToHash($"Attack {i + 1}");
        }
    }

    public override bool SetTarget(GameObject newTarget, bool discardCurrentTarget = false)
    {
        _IsAttacking = false;
        _LastAttackTime = Time.time;


        bool result = base.SetTarget(newTarget, discardCurrentTarget);

        if (result && _Target != null)
        {
            Health tHealth = _Target.GetComponent<Health>();
            if (tHealth)
                tHealth.OnDeath += OnTargetDeath;
        }


        return result;
    }

    // This is needed to satisfy the interface.
    protected override void InteractWithTarget()
    {
        if (_IsAttacking)
            return;
    }

    protected virtual void DoAttackCheck()
    {
        // Debug.Log($"AI: {name}    Target: {_Target}    Attackable: {TargetIsAttackable()}    InRange: {TargetIsWithinAttackRange()}    Path: {_NavMeshAgent.pathStatus}");

        // Check if the target is still alive.
        if (_Target && TargetIsAttackable() && TargetIsWithinAttackRange())
        {
            _IsAttacking = true;

            // If the AI is attacking, has the attack cooldown period fully elapsed yet?
            if (Time.time - _LastAttackTime >= AttackCheckFrequency)
            {
                DoAttack();
            }
        }
        else
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

    protected abstract bool TargetIsAttackable();

    protected bool TargetIsWithinAttackRange()
    {
        return GetDistanceToTarget() <= _InteractionRange * 2.0f;
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


        // Don't allow the target to be changed unless the current target is null or NOT a monster.
        if (attacker && 
            (_Target == null || !_Target.CompareTag("Monster")))
        {
            if (attacker.CompareTag("Monster") || attacker.CompareTag("Player") || attacker.CompareTag("Villager"))
            {
                SetTarget(attacker);
            }
        }

    }

    protected abstract void AnimateAttack();


    protected virtual void OnTargetDeath(GameObject sender, GameObject attacker)
    {
        _IsAttacking = false;

        //string name = _Target != null ? _Target.name : "<null>";
        //Debug.Log($"Target \"{name}\" has died!");


        UpdateNearbyTargetDetectorState();

        SetTarget(null, false);
    }



    public bool IsAttacking { get { return _IsAttacking; } }

}
