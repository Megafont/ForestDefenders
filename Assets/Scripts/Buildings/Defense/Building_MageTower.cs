using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Building_MageTower : Building_Base
{
    public GameObject MageSpawnPoint;
    

    private MageTowerOccupant _ActiveMageInstance;

    private IVillager _VillagerOccupant;

    private bool _VillagerArrived = false;

    private MageTowerOccupant _MaleMageInstance;
    private MageTowerOccupant _FemaleMageInstance;


    private static GameObject _FemaleMagePrefab;
    private static GameObject _MaleMagePrefab;



    protected override void InitBuilding()
    {
        ConfigureBasicBuildingSetup("Defense", "Mage Tower");


        if (MageSpawnPoint == null)
            throw new System.Exception($"The mage spawn point GameObject for the mage tower at {transform.position} is missing!");


        LoadMagePrefabs();
        CreateMageInstances();

    }

    protected override void UpdateBuilding()
    {
        base.UpdateBuilding();


        if (IsOccupied)
        {
            // Draw a vertical line on the tower to show that it is occupied even if the villager hasn't arrived yet.
            Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + Vector3.up * 10, Color.yellow, 0.1f);

            if (_ActiveMageInstance == null)
            {
                // Check if the villager has arrived.
                if (!_VillagerArrived &&
                    Vector3.Distance(transform.position, _VillagerOccupant.transform.position) <= _BuildingDefinition.Radius)
                {
                    _VillagerArrived = true;

                    // Hide the villager and display a mage on top of this mage tower.
                    _VillagerOccupant.gameObject.SetActive(false);

                    // Unhide the appropriate mage prefab.
                    EnableMageInstance(true);

                }
            }
            else // _ActiveMageInstance != null
            {
                // Make the villager come back out of the tower if we're back in the player build phase.
                if (GameManager.Instance.GameState == GameStates.PlayerBuildPhase)
                {
                    EnableMageInstance(false);

                    if (_VillagerOccupant != null)
                    {
                        _VillagerOccupant.gameObject.SetActive(true);
                        _VillagerOccupant.SetTarget(null);
                        
                        _VillagerOccupant = null;
                        _VillagerArrived = false;
                    }
                }
            }    

        }

    }



    public void ClaimOccupancy(IVillager caller)
    {
        if (caller == null)
            throw new System.Exception("The passed in caller is null!");


        _VillagerArrived = false;
        _VillagerOccupant = caller;
    }


    private void EnableMageInstance(bool state)
    {
        // Bail out if the mage instances aren't initialized yet.
        if (_FemaleMageInstance == null || _MaleMageInstance == null)
            return;


        if (state)
        {
            if (_VillagerOccupant is Villager_Female)
                _ActiveMageInstance = _FemaleMageInstance;
            else if (_VillagerOccupant is Villager_Male)
                _ActiveMageInstance = _MaleMageInstance;

            _ActiveMageInstance.HealthComponent.ResetHealthToMax();
            _ActiveMageInstance.gameObject.SetActive(true);
        }
        else
        {
            if (_ActiveMageInstance)
            {
                _ActiveMageInstance.gameObject.SetActive(false);
                _ActiveMageInstance = null;
            }
        }
    }

    private void LoadMagePrefabs()
    {
        if (_FemaleMagePrefab == null)
        {
            _FemaleMagePrefab = Resources.Load<GameObject>($"Structures/Prefabs/Defense/Mage Tower - Female Occupant");

            if (_FemaleMagePrefab == null)
                throw new System.Exception($"Failed to load the female mage prefab!");
        }

        if (_MaleMagePrefab == null)
        {
            _MaleMagePrefab = Resources.Load<GameObject>($"Structures/Prefabs/Defense/Mage Tower - Male Occupant");

            if (_MaleMagePrefab == null)
                throw new System.Exception($"Failed to load the male mage prefab!");
        }
    }

    private void CreateMageInstances()
    {
        if (_FemaleMageInstance == null)
        {
            GameObject female = Instantiate(_FemaleMagePrefab, MageSpawnPoint.transform.position, Quaternion.identity, gameObject.transform);
            _FemaleMageInstance = female.GetComponent<MageTowerOccupant>();
            _FemaleMageInstance.gameObject.SetActive(false);

            _FemaleMageInstance.HealthComponent.OnTakeDamage += OnOccupantTakeDamage;
            _FemaleMageInstance.HealthComponent.OnDeath += OnOccupantDeath;

            if (_FemaleMageInstance == null)
                throw new System.Exception($"Failed to create a female mage instance for the mage tower at position ({transform.position})!");
        }

        if (_MaleMageInstance == null)
        {
            GameObject male = Instantiate(_MaleMagePrefab, MageSpawnPoint.transform.position, Quaternion.identity, gameObject.transform);
            _MaleMageInstance = male.GetComponent<MageTowerOccupant>();
            _MaleMageInstance.gameObject.SetActive(false);

            _MaleMageInstance.HealthComponent.OnTakeDamage += OnOccupantTakeDamage;
            _MaleMageInstance.HealthComponent.OnDeath += OnOccupantDeath;

            if (_MaleMageInstance == null)
                throw new System.Exception($"Failed to create a male mage instance for the mage tower at position ({transform.position})!");
        }


        // Start the mage instances disabled and hidden.
        EnableMageInstance(false);
    }

    private void OnOccupantTakeDamage(GameObject sender, GameObject attacker, float amount, DamageTypes damageType)
    {
        if (_VillagerOccupant != null)
        {
            _VillagerOccupant.HealthComponent.DealDamage(amount, damageType, attacker);
        }
    }

    private void OnOccupantDeath(GameObject sender, GameObject attacker)
    {
        EnableMageInstance(false);
    }



    public bool IsOccupied { get { return _VillagerOccupant != null; } }


}
