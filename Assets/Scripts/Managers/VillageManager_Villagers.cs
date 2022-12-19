using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using Random = UnityEngine.Random;


public class VillageManager_Villagers : MonoBehaviour
{
    public GameObject VillagersParent;


    [Header("Villager Settings")]

    [Tooltip("The initial population cap of the village.")]
    public int StartingPopulationCap = 3;

    [Tooltip("The largest the population is ever allowed to get in the game.")]
    public int MaxPopulationCap = 100;

    [Tooltip("The amount of food required for a villager to spawn.")]
    public int VillagerFoodCost = 50;

    [Tooltip("The delay (in seconds) between spawning queued up villagers (assuming the spawn location is not blocked).")]
    public float VillagerSpawnFrequency = 2.0f;

    [Tooltip("The maximum distance a villager can be from a building in distress and still respond to the call for help.")]
    public float BuildingBackupCallMaxDistance = 20f;


    [Header("Build Phase Heal Building Settings")]

    [Tooltip("Specifies whether or not villagers will heal damaged buildings at the beginning of the player build phase.")]
    public bool VillagersHealBuildings = true;

    [Tooltip("The delay (in seconds) between each time a damaged building check is performed at the beginning of the build phase until all buildings get healed (if there is enough food in the stockpile).")]
    public float VillagerHealBuildingsCheckFrequency = 5.0f;

    [Tooltip("The amount a villager heals a building each time they \"hit\" it.")]
    public float VillagerHealBuildingsAmount = 5.0f;

    [Tooltip("The amount of food expended each time a villager heals a building.")]
    public int VillagerHealBuildingsFoodCost = 5;


    private GameManager _GameManager;
    private ResourceManager _ResourceManager;
    private VillageManager_Buildings _VillageManager_Buildings;

    private Dictionary<string, GameObject> _VillagerCategoryParents;


    private Dictionary<string, GameObject> _VillagerTypeParents;
    private Dictionary<string, GameObject> _VillagerPrefabs;
    private List<IVillager> _AllVillagers;

    private Dictionary<IBuilding, IVillager> _VillagersHealingBuildings; // Tracks villagers that are busy healing buildings.

    private WaitForSeconds _BuildingHealCheckWaitTime;
    private WaitForSeconds _VillagerSpawnWaitTime;

    private int _PopulationCap;

    Coroutine _VillagerHealBuildingsCoroutine;



    private void Awake()
    {
        _GameManager = GameManager.Instance;
        _PopulationCap = StartingPopulationCap;

        _VillageManager_Buildings = _GameManager.VillageManager_Buildings;
        _VillageManager_Buildings.OnBuildingConstructed += OnBuildingConstructed;
        _VillageManager_Buildings.OnBuildingDestroyed += OnBuildingDestroyed;

        _VillagersHealingBuildings = new Dictionary<IBuilding, IVillager>();

        _BuildingHealCheckWaitTime = new WaitForSeconds(VillagerHealBuildingsCheckFrequency);
        _VillagerSpawnWaitTime = new WaitForSeconds(VillagerSpawnFrequency);

        _GameManager.OnGameStateChanged+= OnGameStateChanged;
    }

    // Start is called before the first frame update
    void Start()
    {
        _ResourceManager = GameManager.Instance.ResourceManager;

        _VillagerTypeParents = new Dictionary<string, GameObject>();
        _VillagerPrefabs = new Dictionary<string, GameObject>();
        _AllVillagers = new List<IVillager>();
      

        InitVillagerTypes();

        FindPreExistingVillagers();

        StartCoroutine(SpawnVillagers());


        //Debug.Log("C1: " + GetBuildingCount("Defense", "Barricade"));
        //Debug.Log("C2: " + GetBuildingCountForCategory("Defense"));
        //Debug.Log("C3: " + GetTotalBuildingCount());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        StopAllCoroutines();

        Utils.DestroyAllChildGameObjects(VillagersParent);
    }


    private void InitVillagerTypes()
    {
        Utils.DestroyAllChildGameObjects(VillagersParent);


        // Get a list of all types that implement IVillager.
        Type type = typeof(IVillager);
        IEnumerable types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p));


        foreach (Type t in types)
        {
            // Ignore the types IVillager and Villager_Base.
            if (t.Name != "IVillager" && t.Name != "Villager_Base")
            {
                // Create a parent object for this villager type.
                GameObject parent = new GameObject(t.Name.Substring(9)); // Set the category game object to the name of this villager type minus the "Villager_" prefix.
                parent.transform.parent = VillagersParent.transform;
                _VillagerTypeParents.Add(t.Name, parent);


                // Load the prefab for this villager type.
                GameObject prefab = LoadVillagerPrefab(t.Name);
                _VillagerPrefabs.Add(t.Name, prefab);
            }

        } // end foreach Type t

    }

    private void FindPreExistingVillagers()
    {
        IVillager[] objects = FindObjectsOfType<Villager_Base>();
        foreach (IVillager villager in objects)
        {
            if (villager != null)
            {
                AddVillager(villager);
                //Debug.Log($"Found pre-existing villager: {villager.VillagerType}");
            }

        } // end foreach obj

    }



    public int GetVillagerCountByType(string villagerType)
    {
        // Get the parent game object for the specified building type.
        _VillagerCategoryParents.TryGetValue($"villagerType", out GameObject parent);

        if (parent == null)
            throw new Exception($"Could not get the villager count for unkown villager type \"{villagerType}\"!");

        return parent.transform.childCount;
    }

    public int GetTotalVillagerCount()
    {
        int count = 0;
        foreach (string villagerType in _VillagerTypeParents.Keys)
            count += GetVillagerCountByType(villagerType);

        return count;
    }

    public void RequestBackup(GameObject buildingInDistress, GameObject attacker)
    {
        if (attacker == null ||
            attacker.GetComponent<IMonster>() == null)
        {
            return;
        }


        // Find villagers close enough to help.
        foreach (IVillager villager in _AllVillagers)
        {
            // If the villager is close enough to the building in distress and is not already there, then send them there.
            if (Vector3.Distance(attacker.transform.position, villager.transform.position) <= BuildingBackupCallMaxDistance &&
                Vector3.Distance(buildingInDistress.transform.position, villager.transform.position) > 5)
            {
                // This check excludes villagers that are occupying Mage Towers.
                if (villager.gameObject.activeSelf && !villager.IsAttacking ||
                    (villager.IsAttacking && !villager.TargetIsMonster))
                {
                    villager.SetTarget(attacker);
                }
            }

        } // end foreach

    }

    public void BuffVillagers(bool buffMaxHealth)
    {
        for (int i = 0; i < _AllVillagers.Count; i++)
        {
            IVillager villagerComponent = _AllVillagers[i];

            if (buffMaxHealth)
            {
                // Set the villager's max health, and reset their health to max.
                Health vHealth = villagerComponent.HealthComponent;
                vHealth.MaxHealth = _GameManager.LevelUpDialog.CurrentVillagerMaxHealth;
                vHealth.ResetHealthToMax();
            }
            else
            {
                // Set the villager's attack power.
                (villagerComponent as AI_WithAttackBehavior).AttackPower = _GameManager.LevelUpDialog.CurrentVillagerAttackPower;
            }
        } // end for i
    }

    private GameObject LoadVillagerPrefab(string villagerType)
    {
        GameObject prefab = Resources.Load<GameObject>($"Villagers/{villagerType}");


        if (prefab)
            Debug.Log($"Loaded villager prefab \"Resources/Villagers/{villagerType}\".");
        else
            throw new Exception($"Failed to load villager prefab \"Resources/Villagers/{villagerType}\"!");


        IVillager villagerComponent = prefab.GetComponent<IVillager>();
        if (villagerComponent != null)
            return prefab;
        else
            throw new Exception("The loaded villager prefab does not have a villager component!");
    }

    private IEnumerator SpawnVillagers()
    {
        if (_VillageManager_Buildings.TownCenter == null)
            throw new Exception("Cannot spawn a villager. The town center is null!");


        WaitForSeconds delay = new WaitForSeconds(2.0f);
        yield return delay;

        GameObject townCenter = _VillageManager_Buildings.TownCenter;
        while (true)
        {
            if (_AllVillagers.Count < _PopulationCap &&
                _ResourceManager.Stockpiles[ResourceTypes.Food] >= VillagerFoodCost)
            {
                GameObject prefab = SelectVillagerPrefab();
                _ResourceManager.Stockpiles[ResourceTypes.Food] -= VillagerFoodCost;


                GameObject newVillager = Instantiate(prefab,
                                                     townCenter.transform.position,
                                                     townCenter.transform.rotation,
                                                     _VillagerTypeParents[prefab.name].transform);


                IVillager villagerComponent = newVillager.GetComponent<IVillager>();

                // Set the villager's max health, and reset their health to max.
                Health vHealth = villagerComponent.HealthComponent;
                vHealth.MaxHealth = _GameManager.LevelUpDialog.CurrentVillagerMaxHealth;
                vHealth.ResetHealthToMax();

                // Set the villager's attack power.
                (villagerComponent as AI_WithAttackBehavior).AttackPower = _GameManager.LevelUpDialog.CurrentVillagerAttackPower;

                AddVillager(villagerComponent);

                TotalVillagersSpawned++;
            }

            yield return _VillagerSpawnWaitTime;

        } // end while

    }

    private void AddVillager(IVillager villager)
    {
        villager.gameObject.transform.parent = _VillagerTypeParents[villager.VillagerType].transform;

        _AllVillagers.Add(villager);
        villager.HealthComponent.OnDeath += OnVillagerDeath;
    }

    private GameObject SelectVillagerPrefab()
    {
        int index = Random.Range(0, _VillagerPrefabs.Values.Count);

        return _VillagerPrefabs.Values.ToArray()[index];
    }

    private void SendRandomVillagerToHealBuilding(IBuilding building, GameObject attacker)
    {
        int maxTries = 128;
        IVillager villager = null;

        IMonster monster = attacker != null ? attacker.GetComponent<Monster_Base>() : 
                                              null;


        GameObject target = building.gameObject;
        // Is the attacker a monster
        if (monster != null)
            target = monster.gameObject;


        while (maxTries > 0)
        {
            // Pick a random villager.
            villager = _AllVillagers[Random.Range(0, _AllVillagers.Count)];

            if (!_VillagersHealingBuildings.Values.Contains(villager))
                break;
            else
                maxTries--;

        } // end while


        // Add the building and villager into our heal tracking dictionary and tell the villager to head to the building.
        if (villager != null)
        {
            //Debug.Log("Sending villager to heal building!");

            _VillagersHealingBuildings.Add(building, villager);
            villager.SetTarget(target);
        }
        else
        {
            //Debug.Log("Failed to find an available villager to heal the building!");
        }
    }

    public void EnableVillagersHealBuildings()
    {
        VillagersHealBuildings = true;

        if (_VillagerHealBuildingsCoroutine == null && GameManager.Instance.GameState == GameStates.PlayerBuildPhase)
            _VillagerHealBuildingsCoroutine = StartCoroutine(OrderVillagersToHealBuildings());
    }

    private IEnumerator OrderVillagersToHealBuildings()
    {
        Dictionary<IBuilding, GameObject> damagedBuildings = _VillageManager_Buildings.DamagedBuildingsDictionary;



        while (damagedBuildings == null ||
               damagedBuildings.Count == 0 || _AllVillagers.Count == 0)
        {
            if (damagedBuildings == null)
            { 
                damagedBuildings = _VillageManager_Buildings.DamagedBuildingsDictionary;
            }
            else
            {
                //Debug.Log("There are no damaged buildings to heal or no villagers left!");
            }

            yield return _BuildingHealCheckWaitTime;
        }


        //Debug.Log($"Villagers have begun healing {damagedBuildings.Count} buildings...");

        while (damagedBuildings.Count > 0)
        {
            for (int i = 0; i < _VillagersHealingBuildings.Keys.Count; i++)
            {
                KeyValuePair<IBuilding, IVillager> pair = _VillagersHealingBuildings.ElementAt(i);
                IBuilding building = pair.Key;

                bool removeBuilding = false;


                // Was the building destroyed?
                if (building == null)
                { 
                    removeBuilding = true;
                }
                // Check if the building is fully healed.
                else if (building.HealthComponent.CurrentHealth == building.HealthComponent.MaxHealth)
                {
                    //Debug.Log($"Villagers finished healing building \"{building.Category}/{building.Name}\"!");

                    if (_VillagersHealingBuildings.ContainsKey(building))
                    {
                        removeBuilding = true;                        

                        // NOTE: We do NOT remove the building from the damagedBuildings list here.
                        //       This list is a reference to the list managed by the VillageManager_Buildings class,
                        //       which will handle that task on its own.

                    } // end if

                } // end if


                // Remove this building from the list tracking villagers healing buildings.
                if (removeBuilding)
                    _VillagersHealingBuildings.Remove(building);


            } // end foreach villager healing a building


            foreach (KeyValuePair<IBuilding, GameObject> pair in damagedBuildings)
            {
                // Check if the building does NOT have a villager healing it.
                if (!_VillagersHealingBuildings.ContainsKey(pair.Key))
                {
                    SendRandomVillagerToHealBuilding(pair.Key, pair.Value);
                }

            } // end foreach damaged building


            yield return _BuildingHealCheckWaitTime;

        } // end while


    }


    // EVENT METHODS
    // ========================================================================================================================================================================================================

    // These first two events handle events from the VillageManager_Buildings class.

    private void OnBuildingConstructed(IBuilding building)
    {
        int temp = _PopulationCap + (int) building.GetBuildingDefinition().PopulationCapBoost;

        if (temp > MaxPopulationCap)
            _PopulationCap = MaxPopulationCap;
        else
            _PopulationCap = temp;
    }

    private void OnBuildingDestroyed(IBuilding building, bool wasDeconstructedByPlayer)
    {
        _PopulationCap -= (int) building.GetBuildingDefinition().PopulationCapBoost;
    }


    private void OnGameStateChanged(GameStates newGameState)
    {
        //Debug.Log("GameState Changed to: " + newGameState);

        if (newGameState == GameStates.PlayerBuildPhase)
        {
            _VillagersHealingBuildings.Clear();

            if (VillagersHealBuildings)
            {
                if (_VillagerHealBuildingsCoroutine != null)
                    StopCoroutine(_VillagerHealBuildingsCoroutine);

                _VillagerHealBuildingsCoroutine = StartCoroutine(OrderVillagersToHealBuildings());
            }
        }
        else
        {
            // Stop the coroutine if it is still running due to not enough villagers or something.
            if (_VillagerHealBuildingsCoroutine != null)
                StopCoroutine(_VillagerHealBuildingsCoroutine);
        }
    }

    private void OnVillagerDeath(GameObject sender)
    {
        sender.GetComponent<Health>().OnDeath -= OnVillagerDeath;
       
        _AllVillagers.Remove(sender.GetComponent<IVillager>());

        TotalVillagersLost++;
    }



    // PROPERTIES
    // ========================================================================================================================================================================================================

    public int Population { get { return _AllVillagers.Count; } }
    public int PopulationCap {  get { return _PopulationCap; } }

    public int TotalVillagersSpawned { get; private set; }
    public int TotalVillagersLost { get; private set; }

}
