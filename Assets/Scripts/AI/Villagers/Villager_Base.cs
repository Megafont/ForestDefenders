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
    private const int MAX_RESOURCE_NODE_ATTEMPTS = 16;



    protected VillagerTargetDetector _NearbyTargetDetector;

    protected Hunger _Hunger;

    protected ResourceManager _ResourceManager;
    protected VillageManager_Buildings _VillageManager_Buildings;
    protected VillageManager_Villagers _VillageManager_Villagers;



    protected override void InitAI()
    {
        _Hunger = GetComponent<Hunger>();

        _NearbyTargetDetector = transform.GetComponentInChildren<VillagerTargetDetector>();


        // If AI path drawing is enabled, then display the villager's path.
        if (_GameManager.DrawAIPaths)
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
        
        //Debug.Log($"Node: \"{(node != null ? node.name : "null")}\"");

        if (node)
        {
            if (!node.IsDepleted)
            {
                int amount = Mathf.RoundToInt(node.Gather(gameObject));
            }
            else
            {
                SetTarget(null, true);
                DoTargetCheck();

                return;
            }
        }


        IBuilding building = _Target != null ? _Target.GetComponent<IBuilding>() : null;
        if (building != null)
        {
            Health bHealth = building.HealthComponent;

            // The villager should heal the building if it needs it AND stockpiles are NOT low.
            if (bHealth.CurrentHealth < bHealth.MaxHealth &&
                !_ResourceManager.IsStockpileLevelLow(ResourceTypes.Stone) &&
                !_ResourceManager.IsStockpileLevelLow(ResourceTypes.Wood))
            {
                float villagerHealAmount = _VillageManager_Villagers.VillagerHealBuildingsAmount;

                float healthAfterHeal = bHealth.CurrentHealth + villagerHealAmount;
                float healAmount = villagerHealAmount;

                if (healthAfterHeal > bHealth.MaxHealth)
                    healAmount = healthAfterHeal - bHealth.MaxHealth;

                int resourcesCost = Mathf.RoundToInt(healAmount * _VillageManager_Villagers.BuildingHealResourceCostMultiplier);


                //Debug.Log($"HealAmnt: {healAmount}    Resources Cost: {resourcesCost}    BCurH: {bHealth.CurrentHealth}    BMaxH: {bHealth.MaxHealth}    vHAmnt: {villagerHealAmount}");

                // Heal the building if there are enough resources!
                if (_ResourceManager.GetStockpileLevel(ResourceTypes.Stone) >= resourcesCost &&
                    _ResourceManager.GetStockpileLevel(ResourceTypes.Wood) >= resourcesCost)
                {
                    // Expend resources from the stockpile.
                    _ResourceManager.TryToExpendFromStockpile(ResourceTypes.Stone, resourcesCost);
                    _ResourceManager.TryToExpendFromStockpile(ResourceTypes.Wood, resourcesCost);

                    TextPopup.ShowTextPopup(TextPopup.AdjustStartPosition(gameObject), 
                                            $"Used {resourcesCost} Stone and Wood", 
                                            TextPopupColors.ExpendedResourceColor);

                    bHealth.Heal(healAmount, gameObject);

                    // Make the villager use some food as a result of working on the building.
                    _Hunger.AddToHunger((uint) healAmount);
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

            // Find all accessable resource nodes that are not depleted.
            LevelAreas villagerCurrentArea = Utils_World.DetectAreaNumberFromPosition(transform.position);
            if (villagerCurrentArea != LevelAreas.Unknown)
            {
                List<ResourceNode> accessableActiveResourceNodes = Utils_World.FindActiveResourceNodesAccessableFromArea(villagerCurrentArea);
                ResourceNode possibleTargetResourceNode = null;

                if (accessableActiveResourceNodes.Count > 0)
                {
                    int resourceNodeAttempts = 0;
                    while (true)
                    {
                        
                        int index = Random.Range(0, accessableActiveResourceNodes.Count);

                        possibleTargetResourceNode = accessableActiveResourceNodes[index];
                        if (possibleTargetResourceNode.VillagersMiningThisNode < 1)
                        {
                            break;
                        }
                        else
                        {
                            resourceNodeAttempts++;
                            if (resourceNodeAttempts >= MAX_RESOURCE_NODE_ATTEMPTS)
                                break;
                        }
                    }


                    // Did we find a non-empty resource node?
                    if (possibleTargetResourceNode && !possibleTargetResourceNode.IsDepleted)
                    {
                        SetTarget(possibleTargetResourceNode.gameObject);
                    }

                }
                else
                {
                    float rand = Random.value;

                    List<ResourceNode> allAccessableResourceNodes = Utils_World.FindAllResourceNodesAccessableFromArea(villagerCurrentArea);
                    //Debug.Log($"RAND: {rand}");

                    
                    // If there are no accessable and undelpleted resource nodes left, then give the villager
                    // a certain chance of going to an empty node anyway so they don't stand around doing nothing so much.
                    if (allAccessableResourceNodes.Count > 0 &&
                        rand <= _VillageManager_Villagers.VillagerChanceToVisitEmptyNodes)
                    {
                        SetTarget(allAccessableResourceNodes[Random.Range(0, allAccessableResourceNodes.Count - 1)].gameObject);
                    }
                    
                }


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
                if (_Target.TryGetComponent(out ResourceNode oldNode))
                    oldNode.RemoveVillagerFromMiningList(this);
            }

            // If the new target is a resource node, then add this villager to it's list of villagers mining it.
            if (newTarget)
            {
                if (newTarget.TryGetComponent(out ResourceNode newNode))
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
            return newTargetDistance < currentTargetDistance;
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
