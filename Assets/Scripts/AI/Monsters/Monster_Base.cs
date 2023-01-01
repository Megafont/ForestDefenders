using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

using Random = UnityEngine.Random;


/// <summary>
/// This is the base class for all villagers.
/// </summary>
public abstract class Monster_Base : AI_WithAttackBehavior, IMonster
{
    [Tooltip("The number of points earned when this monster is killed.")]
    public int ScoreValue = 10;
    [Tooltip("The tier this monster is in. Determines which tiers of buildings it can target and destroy.")]
    public int Tier = 1;


    protected MonsterTargetDetector _NearbyTargetDetector;

    protected GameObject _Player;




    protected override void InitAI()
    {
        _NearbyTargetDetector = transform.GetComponentInChildren<MonsterTargetDetector>();

        _GameManager.OnGameOver += OnGameOver;


        // If we are running in the Unity Editor, display the villager's path.
        if (DISPLAY_AI_PATHS && Application.isPlaying)
        {
            AI_Debug_DrawAIPath debugPathDrawer = gameObject.AddComponent<AI_Debug_DrawAIPath>();
            debugPathDrawer.SetColorAndWidth(Color.red, 0.05f);
        }


        _Player = GameManager.Instance.Player;

        base.InitAI();
    }

    protected override void UpdateAI()
    {
        if (_GameManager.GameState != GameStates.GameOver)
            base.UpdateAI();
    }

    protected override void DoTargetCheck()
    {
        //Debug.Log($"AI: {name}    Target: {_Target}    Attackable: {TargetIsAttackable()}    InRange: {TargetIsWithinAttackRange()}    Path: {_NavMeshAgent.pathStatus}   Destination: {_NavMeshAgent.destination}");

        if (_Target == null)
        {
            // This monster is not chasing a target. So try to find a building to target.
            GameObject newTarget = Utils_AI.FindNearestBuildingAtOrBelowTier(gameObject, Tier);


            // If no building was found, then try to find a villager.           
            if (newTarget == null)
                newTarget = Utils_AI.FindNearestObjectOfType(gameObject, typeof(Villager_Base));


            // If no villager was found, then target the player.
            if (newTarget == null)
                newTarget = _Player;


            if (newTarget)
                SetTarget(newTarget);
        }
        
        // If this monster is chasing a target and the target gets far enough away, revert to the previous target.
        else if (_Target.CompareTag("Player") || _Target.CompareTag("Villager")) // Is the target the player or a villager?
        {
            float distanceToTarget = GetDistanceToTarget();
            if (distanceToTarget > MaxChaseDistance)
            {
                SetTarget(_PrevTarget);
            }
            else if (distanceToTarget > _InteractionRange)
            {
                // The target is outside of our interaction range, so enable movement again so we can chase it!
                StartMoving();
                _IsAttacking = false;
            }
            
        }


        UpdateNearbyTargetDetectorState();
    }

    public override bool ValidateTarget(GameObject newTarget)
    {
        if (!base.ValidateTarget(newTarget))
        {
            return false;
        }
        else if (newTarget != null && newTarget.CompareTag("Villager") && // If we are targeting a villager and the new target is another villager, then return false so we don't switch the target.
                 _Target != null && _Target.CompareTag("Villager")) 
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    protected override void AnimateAttack()
    {
        int n = Random.Range(1, 3);

        string trigger = $"Attack {n}";
        _Animator.ResetTrigger(trigger);
        _Animator.SetTrigger(trigger);

        _HashOfPlayingAttackAnim = _AttackAnimationNameHashes[n];
    }

    protected override bool TargetIsAttackable()
    {
        if (_Target == null)
            return false;


        if (_Target.CompareTag("Building") || _Target.CompareTag("Player") || _Target.CompareTag("Villager"))
            return true;
        else
            return false;
    }

    protected override void DoAttack()
    {
        // Do not start another attack if one is already in progress.
        if (_HashOfPlayingAttackAnim < 0)
            base.DoAttack();
    }

    public float GetDangerValue()
    {
        // We don't use the HealthComponent property here, because we need this method to
        // also work when called on a prefab. On a prefab, that property returns null
        // since it is not an instance. That means Start() was never called on it so
        // the property is uninitialized.
        return AttackPower + GetComponent<Health>().MaxHealth;
    }

    public int GetScoreValue()
    {
        return ScoreValue;
    }

    protected override void UpdateNearbyTargetDetectorState()
    {
        bool state = true;
        if (_Target == null)
            state = true;
        else if (_Target.CompareTag("Player") || _Target.CompareTag("Villager"))
            state = false;
        else
            state = true;


        if (_NearbyTargetDetector)
            _NearbyTargetDetector.Enable(state);
    }

    protected void OnGameOver()
    {
        if (_Animator)
            _Animator.SetTrigger("Victory");
    }


    // These are a couple extra methods needed to satisfy the IMonster interface since you can't have fields in an interface.
    public float GetAttackPower() { return AttackPower; }
    public int GetTier() { return Tier; }

    public string MonsterTypeName { get { return this.GetType().Name; } }

}
