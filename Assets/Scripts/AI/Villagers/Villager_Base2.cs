using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

using Random = UnityEngine.Random;



/// <summary>
/// This is the base class for all villagers.
/// </summary>
public abstract class Villager_Base2 : AI_WithAttackBehavior //, IVillager
{
    protected VillagerTargetDetector _NearbyTargetDetector;

    protected ResourceManager _ResourceManager;
    protected VillageManager_Buildings _VillageManager_Buildings;
    protected VillageManager_Villagers _VillageManager_Villagers;



    protected override void InitAI()
    {
        _NearbyTargetDetector = transform.GetComponentInChildren<VillagerTargetDetector>();

        
        // If we are running in the Unity Editor, display the villager's path.
        if (DISPLAY_AI_PATHS && Application.isPlaying)
        {
            AI_Debug_DrawAIPath debugPathDrawer = gameObject.AddComponent<AI_Debug_DrawAIPath>();
            debugPathDrawer.SetColorAndWidth(Color.blue, 0.05f);
        }
        

        _ResourceManager = _GameManager.ResourceManager;
        _VillageManager_Buildings = _GameManager.VillageManager_Buildings;
        _VillageManager_Villagers = _GameManager.VillageManager_Villagers;


        base.InitAI();

        // I may add in the ability for villagers to run, so set the max speed higher so their animation
        // speed is slower so they look like they're walking. See AI_Base.AnimateAI().
        MaxMovementSpeed = 8.0f;
    }

    protected override void UpdateAI()
    {
    
    }

    // Remove this AI method entirely later?
    protected override void DoTargetCheck()
    {
        
    }

    protected override bool TargetIsAttackable()
    {
        if (_Target.CompareTag("Monster") || _Target.CompareTag("Player"))
            return true;
        else
            return false;
    }

    protected override void AnimateAttack()
    {
        int n = Random.Range(1, 4);

        string trigger = $"Attack {n}";
        _Animator.ResetTrigger(trigger);
        _Animator.SetTrigger(trigger);

        _HashOfPlayingAttackAnim = _AttackAnimationNameHashes[n];
    }

    protected override void UpdateNearbyTargetDetectorState()
    {
        bool state = true;
        if (_Target == null)
            state = true;
        else if (_Target && _Target.CompareTag("Monster")) // Don't target a monster if already targeting one.
            state = false;


        if (_NearbyTargetDetector)
            _NearbyTargetDetector.Enable(state);
    }



    public string VillagerTypeName { get { return this.GetType().Name; } }
}
