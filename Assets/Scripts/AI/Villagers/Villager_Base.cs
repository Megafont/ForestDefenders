using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

using Random = UnityEngine.Random;



/// <summary>
/// NOTE: This is currently unused, but is referenced by the VillagerTaskEventArgs class.
/// </summary>
public enum VillagerTasks
{
    None = 0,
    GoToTask,
    ConstructBuilding,
    GatherResource,
}



/// <summary>
/// This is the base class for all villagers.
/// </summary>
public abstract class Villager_Base : AI_WithAttackBehavior, IVillager
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
        _MaxMovementSpeed = 8.0f;
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
            if (!node.IsDepleted)
            {
                node.Gather(gameObject);
            }
            else
            {
                SetTarget(null, true);
                DoTargetCheck();

                return;
            }
        }


        IBuilding building = _Target.GetComponent<IBuilding>();

        if (building != null)
        {
            Health bHealth = building.HealthComponent;

            // The villager should heal the building if it needs it AND stockpiles are NOT low.
            if (bHealth.CurrentHealth < bHealth.MaxHealth &&
                _ResourceManager.Stockpiles[ResourceTypes.Food] >= _ResourceManager.ResourceStockpilesLowThreshold)
            {
                float villagerHealAmount = _VillageManager_Villagers.VillagerHealBuildingsAmount;

                float healthAfterHeal = bHealth.CurrentHealth + villagerHealAmount;
                float healAmount = villagerHealAmount;

                if (healthAfterHeal > bHealth.MaxHealth)
                    healAmount = healthAfterHeal - bHealth.MaxHealth;

                int foodAmount = Mathf.CeilToInt(healAmount * _VillageManager_Villagers.BuildingHealFoodCostMultiplier);


                //Debug.Log($"HealAmnt: {healAmount}    Food: {foodAmount}    BCurH: {bHealth.CurrentHealth}    BMaxH: {bHealth.MaxHealth}    vHAmnt: {villagerHealAmount}");

                // Don't heal the building if there isn't enough food!
                if (_ResourceManager.Stockpiles[ResourceTypes.Food] >= _ResourceManager.ResourceStockpilesOkThreshold)
                {
                    bHealth.Heal(healAmount, gameObject);

                    // Use some food from the stockpile.
                    _ResourceManager.Stockpiles[ResourceTypes.Food] -= foodAmount;
                }
                else
                {
                    SetTarget(null, true);
                    DoTargetCheck();
                }

                // Is the building fully healed?
                if (bHealth.CurrentHealth == bHealth.MaxHealth)
                {
                    SetTarget(null, true);
                    DoTargetCheck();
                }

            } // end if building needs to be healed            

        }

    }


    protected override void DoTargetCheck()
    {
        if (_GameManager.GameState == GameStates.PlayerBuildPhase)
            DoTargetCheck_BuildPhase();
        else if (_GameManager.GameState == GameStates.MonsterAttackPhase)
            DoTargetCheck_MonsterAttackPhase();
        else // Use the normal build phase behavior in game states like menu and game over.
            DoTargetCheck_BuildPhase();


        UpdateNearbyTargetDetectorState();
    }

    protected void DoTargetCheck_BuildPhase()
    {
        if (_Target == null)
        {
            // This villager is not chasing a target. So if the target check time has elapsed, then do a new target check.                        

            // Find a non-empty resource node of the same type as the lowest resource stockpile.
            ResourceTypes lowest = _ResourceManager.GetLowestResourceStockpileType();
            ResourceNode possibleTargetResourceNode = _ResourceManager.FindNearestResourceNode(transform.position, lowest);

            // If another villager is already mining the nearest node, randomly choose a different one that is not depleted.
            if (possibleTargetResourceNode == null || possibleTargetResourceNode.VillagersMiningThisNode > 0)            
                possibleTargetResourceNode = _ResourceManager.GetRandomActiveResourceNode();


            // Did we find a non-empty resource node?
            if (possibleTargetResourceNode && !possibleTargetResourceNode.IsDepleted)
            {
                SetTarget(possibleTargetResourceNode.gameObject);
            }

        }
        // If this villager is chasing a target and the target gets far enough away, revert to the previous target.
        else if (_Target.CompareTag("Monster") || _Target.CompareTag("Player"))
        {
            if (!TargetIsWithinChaseRange() && !_VillageManager_Villagers.VillagerIsOnBuildingHealCall(this))
            {
                SetTarget(_PrevTarget);
            }
        }

    }

    protected void DoTargetCheck_MonsterAttackPhase()
    {
        // Check if there is an unoccupied Mage Tower.
        Building_MageTower closestUnnoccupiedTower = _VillageManager_Buildings.FindNearestUnoccupiedMageTower(transform.position);
        if (closestUnnoccupiedTower)
        {
            // Claim occupancy of the tower so it knows this AI is coming.
            closestUnnoccupiedTower.ClaimOccupancy(this);

            // Start the AI heading to the tower.
            SetTarget(closestUnnoccupiedTower.gameObject);           
        }
        else
        {
            DoTargetCheck_BuildPhase();
        }
    }

    public override bool SetTarget(GameObject newTarget, bool discardCurrentTarget = false)
    {
        if (ValidateTarget(newTarget))
        {
            // If the current target is a resource node, then remove this villager from it's list of villagers currently mining it.
            if (_Target)
            {
                ResourceNode oldNode = _Target.GetComponent<ResourceNode>();
                if (oldNode != null)
                    oldNode.RemoveVillagerFromMiningList(this);
            }

            // If the new target is a resource node, then add this villager to it's list of villagers mining it.
            if (newTarget)
            {
                ResourceNode newNode = newTarget.GetComponent<ResourceNode>();
                if (newNode != null)
                    newNode.AddVillagerToMiningList(this);
            }
        }


        return base.SetTarget(newTarget, discardCurrentTarget);
    }

    public override bool ValidateTarget(GameObject newTarget)
    {
        if (!base.ValidateTarget(newTarget))
        {
            return false;
        }
        else if (TargetIsMonster && newTarget != null)
        {
            //return false;

            float currentTargetDistance = Vector3.Distance(_Target.transform.position, transform.position);
            float newTargetDistance = Vector3.Distance(newTarget.transform.position, transform.position);

            // If the new target and the current target are both monsters, then only allow changing targets if the new one is closer.
            return newTargetDistance < currentTargetDistance ? true : false;
        }
        else
        {
            return true;
        }
    }

    protected override void InteractWithTarget()
    {
        if (_IsAttacking)
            return;


        AnimateAttack(); // We're just using the attack animation for when they're doing work, too.
        DoTaskWork();

        return;
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
