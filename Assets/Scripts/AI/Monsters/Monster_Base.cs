using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public int Tier = 0;


    protected MonsterTargetDetector _NearbyTargetDetector;

    protected GameObject _Player;



    protected override void InitAI()
    {
        _NearbyTargetDetector = transform.GetComponentInChildren<MonsterTargetDetector>();


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
        base.UpdateAI();
    }


    protected override void DoTargetCheck()
    {
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
        else if (_Target == _Player || _Target.CompareTag("Villager")) // Is the target the player or a villager?
        {
            float distanceToTarget = Vector3.Distance(transform.position, _Target.transform.position);
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

    public override bool ValidateTarget(GameObject target)
    {
        if (!base.ValidateTarget(target))
        {
            return false;
        }
        else if (target != null && target.CompareTag("Villager") && // If we are targeting a villager and the new target is another villager, then return false so we don't switch the target.
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



    // These are a couple extra methods needed to satisfy the IMonster interface since you can't have fields in an interface.
    public float GetAttackPower() { return AttackPower; }
    public int GetTier() { return Tier; }

}
