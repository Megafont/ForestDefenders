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
    public int ScoreValue = 10;


    private MonsterTargetDetector _NearbyTargetDetector;



    protected override void InitAI()
    {
        _NearbyTargetDetector = transform.GetComponentInChildren<MonsterTargetDetector>();

        base.InitAI();
    }

    protected override void UpdateAI()
    {
        base.UpdateAI();
    }


    protected override void DoTargetCheck()
    {
        GameObject player = GameManager.Instance.Player;


        if (_Target == null)
        {
            // This monster is not chasing a target. So if the target check time has elapsed, then do a new target check.
            GameObject possibleNewTarget = Utils_AI.FindNearestObjectOfType(gameObject, typeof(Building_Base));
            if (possibleNewTarget)
                SetTarget(possibleNewTarget);
        }
        // If this monster is chasing a target and the target gets far enough away, revert to the previous target.
        else if (_Target == player || _Target.CompareTag("Villager")) // Is the target the player or a villager?
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
        else if (target != null && target.tag == "Villager" &&
                 _Target != null && _Target.tag == "Villager") 
        {
            return false;
        }
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
