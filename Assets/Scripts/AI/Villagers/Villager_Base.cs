using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.AI;



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
    [Tooltip("How often (in seconds) that the villager will check for an available task if not already doing one.")]
    public float TaskCheckFrequency = 5.0f;
    [Tooltip("How often (in seconds) the villager will work on the current task.")]
    public float TaskWorkFrequency = 2.0f;


    public VillagerTasks CurrentTask { get; private set; }
    public object TaskTarget { get; private set; }


    public event VillagerTaskEventHandler OnTaskFinished;
    public event VillagerTaskEventHandler OnTaskStarted;


    private float _LastTaskCheckTime;
    private float _LastTaskWorkTime;

    private VillagerTargetDetector _NearbyTargetDetector;



    protected override void InitAI()
    {
        _NearbyTargetDetector = transform.GetComponentInChildren<VillagerTargetDetector>();

        base.InitAI();

        // I may add in the ability for villagers to run, so set the max speed higher so their animation
        // speed is slower so they look like they're walking. See AI_Base.AnimateAI().
        MaxMovementSpeed = 8.0f;
    }

    protected override void UpdateAI()
    {
        base.UpdateAI();


        if (CurrentTask == VillagerTasks.None && 
            Time.time - _LastTaskCheckTime >= TaskCheckFrequency)
        {
            DoTaskCheck();
        }
        else if (CurrentTask == VillagerTasks.GoToTask)
        {
            DoGoToTask();
        }
        else if (CurrentTask != VillagerTasks.None &&
                 Time.time - _LastTaskWorkTime >= TaskWorkFrequency)
        {
            DoTaskWork();
        }

    }


    protected virtual void DoTaskCheck()
    {

    }

    protected virtual void DoGoToTask()
    {
        if (TaskTarget == null)
            return;

    }

    protected virtual void DoTaskWork()
    {
        if (CurrentTask == VillagerTasks.GatherResource)
            DoGathering();
    }

    protected virtual void DoGathering()
    {

    }


    protected override void DoTargetCheck()
    {
        GameObject player = GameManager.Instance.Player;


        if (_Target == null)
        {
            // This villager is not chasing a target. So if the target check time has elapsed, then do a new target check.            
            /*
            GameObject possibleNewTarget = Utils_AI.FindNearestObjectOfType(gameObject, typeof(Building_Base));
            if (possibleNewTarget)
            {
                SetTarget(possibleNewTarget);
            }
            */
            if (_Target == null)
                _NearbyTargetDetector.Enable(true);

        }
        // If this villager is chasing a target and the target gets far enough away, revert to the previous target.
        else if (_Target.CompareTag("Monster"))
        {
            if (Vector3.Distance(transform.position, _Target.transform.position) >= MaxChaseDistance)
            {
                SetTarget(_PrevTarget);

                UpdateNearbyTargetDetectorState();
            }
        }
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
        base.DoAttack();

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

}
