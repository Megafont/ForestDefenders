using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;



/// <summary>
/// This is the base class for all villagers.
/// </summary>
public abstract class Monster_Base : AI_WithAttackBehavior, IMonster
{
    [Tooltip("The number of points earned when this monster is killed.")]
    public int ScoreValue = 10;
    [Tooltip("The tier this monster is in. Determines which tiers of buildings it can target and destroy.")]
    public int MonsterTier = 0;


    private MonsterTargetDetector _NearbyTargetDetector;

    private GameObject _Player;



    protected override void InitAI()
    {
        _NearbyTargetDetector = transform.GetComponentInChildren<MonsterTargetDetector>();

        GameObject _Player = GameManager.Instance.Player;

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
            GameObject newTarget = Utils_AI.FindNearestBuildingAtOrBelowTier(gameObject, MonsterTier); //Utils_AI.FindNearestObjectOfType(gameObject, typeof(Building_Base));


            // If no building was found, then try to find a villager.           
            if (newTarget == null)
                newTarget = Utils_AI.FindNearestObjectOfType(gameObject, typeof(Villager_Base));


            // If no villager was found, then target the player.
            if (newTarget == null)
                newTarget = _Player;
            
            
            SetTarget(newTarget);
        }
        
        // If this monster is chasing a target and the target gets far enough away, revert to the previous target.
        else if (_Target == _Player || _Target.CompareTag("Villager")) // Is the target the player or a villager?
        {
            if (Vector3.Distance(transform.position, _Target.transform.position) >= MaxChaseDistance)
            {
                SetTarget(_PrevTarget);
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
        else if (target != null && target.tag == "Villager" && // If we are targeting a villager and the new target is a villager, then return false so we don't switch the target.
                 _Target != null && _Target.tag == "Villager") 
        {
            return false;
        }
        /*else if (target != null && target == _Player && // If we are targeting the player and the new target already is the player, then return false so we don't try to switch the target.
                 _Target != null && _Target == _Player)
        {
            return false;
        }*/
        else
        {
            return true;
        }
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
        else if (_Target && _Target.tag == "Villager")
            state = false;
        else
            state = true;


        _NearbyTargetDetector.Enable(state);
    }

    WaitForSeconds _FadeOutDelay = new WaitForSeconds(2.0f);
    protected override IEnumerator FadeOutAfterDeath()
    {
        yield return _FadeOutDelay;

        Destroy(gameObject);
    }

}
