using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.AI;

using Random = UnityEngine.Random;



public enum VillagerTasks
{
    None = 0,
    GoToTask,
    ConstructBuilding,
    GatherResource,
}


public delegate void VillagerTaskEventHandler(object sender, VillagerTaskEventArgs e);


/// <summary>
/// This is the base class for all villagers.
/// </summary>
public abstract class Villager_Base : AI_WithAttackBehavior, IVillager
{
    private VillagerTargetDetector _NearbyTargetDetector;

    private ResourceManager _ResourceManager;



    protected override void InitAI()
    {
        _NearbyTargetDetector = transform.GetComponentInChildren<VillagerTargetDetector>();

        _ResourceManager = GameManager.Instance.ResourceManager;

        base.InitAI();

        // I may add in the ability for villagers to run, so set the max speed higher so their animation
        // speed is slower so they look like they're walking. See AI_Base.AnimateAI().
        MaxMovementSpeed = 8.0f;
    }

    protected override void UpdateAI()
    {
        base.UpdateAI();

    }


    protected virtual void DoTaskWork()
    {
        ResourceNode node = _Target.GetComponent<ResourceNode>();
        if (node)
        {
            node.Gather();

            if (node.AmountAvailable == 0)
            {
                SetTarget(null);
                DoTargetCheck();
            }

            return;
        }


        IBuilding building = _Target.GetComponent<IBuilding>();
        if (building != null)
        {
            throw new NotImplementedException();
        }

    }


    protected override void DoTargetCheck()
    {
        GameObject player = GameManager.Instance.Player;


        if (_Target == null)
        {
            // This villager is not chasing a target. So if the target check time has elapsed, then do a new target check.                        

            // Find a non-empty resource node of the same type as the lowest resource stockpile.
            ResourceTypes lowest = _ResourceManager.GetLowestResourceStockpileType();
            ResourceNode possibleTargetResourceNode = _ResourceManager.FindNearestResourceNode(transform.position, lowest);
            
            // If we didn't find a resource node of the same type as the lowest resource stockpile, then find a non-empty one of any type.
            if (possibleTargetResourceNode == null)
                possibleTargetResourceNode = _ResourceManager.FindNearestResourceNode(transform.position);
            
            // Did we find a non-empty resource node?
            if (possibleTargetResourceNode)
                SetTarget(possibleTargetResourceNode.gameObject);

        }
        // If this villager is chasing a target and the target gets far enough away, revert to the previous target.
        else if (_Target.CompareTag("Monster"))
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
        else if (target != null && target.tag == "Monster" &&
                 _Target != null && _Target.tag == "Monster")
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    protected override void DoAttack()
    {
        // NOTE: We don't need to call the base class method in here.


        // Check if the villager is doing a work task.
        if (_Target.tag != "Monster")
        {
            _LastAttackTime = Time.time;

            AnimateAttack();
            DoTaskWork();

            return;
        }
        else // The villager is not doing a work task, so apply damage to the target.
        {
            base.DoAttack();
        }

    }

    protected override void AnimateAttack()
    {
        int n = Random.Range(1, 4);

        string trigger = $"Attack {n}";
        _Animator.ResetTrigger(trigger);
        _Animator.SetTrigger(trigger);
    }

    protected override void UpdateNearbyTargetDetectorState()
    {
        bool state = true;
        if (_Target == null)
            state = true;
        else if (_Target && _Target.tag == "Monster") // Don't target a monster if already targeting one.
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



    public string VillagerType { get { return this.GetType().Name; /* return MethodBase.GetCurrentMethod().DeclaringType.Name; */ } }
}
