using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using Random = UnityEngine.Random;


public class VillageManager : MonoBehaviour
{
    [Header("Parent Objects")]
    public GameObject BuildingsParent;
    public GameObject VillagersParent;


    [Header("Buildings Settings")]


    [Header("Villager Settings")]

    [Tooltip("The initial population cap of the village.")]
    public int StartingPopulationCap = 3;

    [Tooltip("The amount of food required for a villager to spawn.")]
    public int VillagerFoodCost = 50;
    
    [Tooltip("The delay between spawning queued up villagers (assuming the spawn location is not blocked).")]
    public float VillagerSpawnDelay = 2.0f;

    [Tooltip("The maximum distance a villager can be from a building in distress and still respond to the call for help.")]
    public float BackupCallMaxDistance = 20f;



    private ResourceManager _ResourceManager;

    private Dictionary<string, GameObject> _BuildingCategoryParents;
    private GameObject _TownCenter;


    private Dictionary<string, GameObject> _VillagerTypeParents;
    private Dictionary<string, GameObject> _VillagerPrefabs;
    private List<IVillager> _AllVillagers;
    
    private WaitForSeconds _VillagerSpawnWaitTime;

    private int _PopulationCap;



    // Start is called before the first frame update
    void Start()
    {
        _ResourceManager = GameManager.Instance.ResourceManager;

        _BuildingCategoryParents = new Dictionary<string, GameObject>();

        _VillagerTypeParents = new Dictionary<string, GameObject>();
        _VillagerPrefabs = new Dictionary<string, GameObject>();
        _AllVillagers = new List<IVillager>();

        _PopulationCap = StartingPopulationCap;
        _VillagerSpawnWaitTime = new WaitForSeconds(VillagerSpawnDelay);

        
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Town Center");
        if (objs.Length > 1)
            Debug.LogWarning("There should only be one Town Center in the level!");
        else if (objs.Length < 1)
            throw new Exception("This level does not contain a Town Center!");
        
        _TownCenter = objs[0];
        

        InitVillagerTypes();

        InitBuildingCategoryParentObjects();
        FindPreExistingVillageObjects();

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

        Utils.DestroyAllChildGameObjects(BuildingsParent);
        Utils.DestroyAllChildGameObjects(VillagersParent);
    }



    private void FindPreExistingVillageObjects()
    {
        GameObject[] objects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in objects)
        {
            IBuilding building = obj.GetComponent<IBuilding>();
            if (building != null)
            {
                AddBuilding(building);
                //Debug.Log($"Found pre-existing building: {building.BuildingCategory}/{building.BuildingName}");
            }

            IVillager villager = obj.GetComponent<IVillager>();
            if (villager != null)
            {
                AddVillager(villager);
                //Debug.Log($"Found pre-existing villager: {villager.VillagerType}");
            }

        } // end foreach obj

    }



    // BUILDING RELATED METHODS
    // ========================================================================================================================================================================================================

    public int GetBuildingCountOfBuildingType(string category, string buildingName)
    {
        // Get the parent game object for the specified building type.
        _BuildingCategoryParents.TryGetValue($"{category}/{buildingName}", out GameObject parent);

        if (parent == null)
            throw new Exception($"Could not get the building count for unkown building type \"{category}/{buildingName}\"!");

        return parent.transform.childCount;
    }

    public int GetBuildingCountOfBuildingCategory(string category)
    {
        // Get the parent game object for the specified building type.
        _BuildingCategoryParents.TryGetValue($"{category}", out GameObject parent);

        if (parent == null)
            throw new Exception($"Could not get the building count for unkown category \"{category}\"!");


        int count = 0;
        foreach (Transform child in parent.transform)
            count += child.childCount;

        return count;
    }

    public int GetTotalBuildingCount()
    {
        int count = 0;
        foreach (string category in BuildModeDefinitions.GetBuildingCategoriesList())
            count += GetBuildingCountOfBuildingCategory(category);

        return count;
    }
    
    public List<IBuilding> GetBuildingsOfType(string category)
    {
        List<IBuilding> buildings = new List<IBuilding>();


        // Get the parent game object for the specified building type.
        _BuildingCategoryParents.TryGetValue($"{category}", out GameObject parent);

        if (parent == null)
            throw new Exception($"Could not get the buildings list for unkown category \"{category}\"!");


        foreach (Transform building in parent.transform)
            buildings.Add((IBuilding)building);


        return buildings;
    }

    public List<Building_MageTower> GetUnoccupiedMageTowers()
    {
        List<Building_MageTower > mageTowers = new List<Building_MageTower>();


        // Get the parent game object for the specified building type.
        _BuildingCategoryParents.TryGetValue("Defense/Mage Tower", out GameObject parent);

        if (parent == null)
            throw new Exception($"Could not get the buildings list for category \"Mage Towers\"!");


        foreach (Transform building in parent.transform)
        {
            Building_MageTower mageTower = building.GetComponent<Building_MageTower>();

            if (mageTower && !mageTower.IsOccupied)
                mageTowers.Add(mageTower);
        }


        return mageTowers;
    }

    public Building_MageTower FindNearestUnoccupiedMageTower(Vector3 callerPosition)
    {
        Building_MageTower closestMageTower = null;
        float minDistance = float.MaxValue;


        foreach (Building_MageTower mageTower in GetUnoccupiedMageTowers())
        {
            if (mageTower == null)
                continue;

            float distance = Vector3.Distance(callerPosition, mageTower.transform.position);
            if (distance < minDistance)
            {       
                closestMageTower = mageTower;
                minDistance = distance;
            }

        } // end foreach node


        return closestMageTower;
    }

    public GameObject SpawnBuilding(GameObject buildingPrefab, string buildingCategory, string buildingName, Vector3 position, Quaternion rotation)
    {
        GameObject newBuilding = Instantiate(buildingPrefab, position, rotation);
        
        IBuilding building = newBuilding.GetComponent<IBuilding>();
        AddBuilding(building);
        
        return newBuilding;
    }

    private void InitBuildingCategoryParentObjects()
    {
        Utils.DestroyAllChildGameObjects(BuildingsParent);


        // Create a parent game object for each building category.
        foreach (string category in BuildModeDefinitions.GetBuildingCategoriesList())
        {
            GameObject buildingCategoryParent = new GameObject(category);

            buildingCategoryParent.transform.parent = BuildingsParent.transform;
            _BuildingCategoryParents.Add(category, buildingCategoryParent);


            // Create a parent game object for each building type in the category.
            foreach (string buildingName in BuildModeDefinitions.GetBuildingNamesListForCategory(category))
            {
                GameObject buildingTypeParent = new GameObject($"{buildingName}s");

                buildingTypeParent.transform.parent = buildingCategoryParent.transform;
                _BuildingCategoryParents.Add($"{category}/{buildingName}", buildingTypeParent);
            }
        }


        // Create a parent object for unknown buildings.
        GameObject unknown = new GameObject("Unknown");
        unknown.transform.parent = BuildingsParent.transform;
        _BuildingCategoryParents.Add("None/None", unknown);
    }

    private void AddBuilding(IBuilding building)
    {
        _BuildingCategoryParents.TryGetValue($"{building.BuildingCategory}/{building.BuildingName}", out GameObject parent);

        // Set the parent of the building.
        // If the building returns "None" for both category and building name, then it will be added to the "Unknown" parent object.
        // If the building's category and name are not found in the dictionary, then it will be added to the "Unknown" parent object.
        if (parent)
            building.gameObject.transform.parent = parent.transform;
        else
            building.gameObject.transform.parent = _BuildingCategoryParents["None/None"].transform;


        building.gameObject.GetComponent<Health>().OnDeath += OnBuildingDestroyed;

        _PopulationCap += (int) building.GetBuildingDefinition().PopulationCapBoost;


        // If the building has a resource node (like farms do), then add it to the resource manager.
        ResourceNode node = building.gameObject.GetComponent<ResourceNode>();
        if (node)
            _ResourceManager.AddResourceNode(node);
    }



    // VILLAGER RELATED METHODS
    // ========================================================================================================================================================================================================

    public int GetVillagerCountByType(string villagerType)
    {
        // Get the parent game object for the specified building type.
        _BuildingCategoryParents.TryGetValue($"villagerType", out GameObject parent);

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
            if (Vector3.Distance(attacker.transform.position, villager.transform.position) <= BackupCallMaxDistance &&
                Vector3.Distance(buildingInDistress.transform.position, villager.transform.position) > 5)
            {
                // This check excludes villagers that are occupying Mage Towers.
                if (villager.gameObject.activeSelf && !villager.IsAttacking)
                    villager.SetTarget(attacker);
            }

        } // end foreach

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
        if (_TownCenter == null)
            throw new Exception("Cannot spawn a villager. The town center is null!");


        WaitForSeconds delay = new WaitForSeconds(2.0f);
        yield return delay;

        while (true)
        {
            if (_AllVillagers.Count < _PopulationCap &&
                _ResourceManager.Stockpiles[ResourceTypes.Food] >= VillagerFoodCost)
            {
                GameObject prefab = SelectVillagerPrefab();
                _ResourceManager.Stockpiles[ResourceTypes.Food] -= VillagerFoodCost;

                
                GameObject newVillager = Instantiate(prefab,
                                                     _TownCenter.transform.position,
                                                     _TownCenter.transform.rotation,
                                                     _VillagerTypeParents[prefab.name].transform);

                AddVillager(newVillager.GetComponent<IVillager>());
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

    private void OnVillagerDeath(GameObject sender)
    {
        sender.GetComponent<Health>().OnDeath -= OnVillagerDeath;
       
        _AllVillagers.Remove(sender.GetComponent<IVillager>());
    }

    private void OnBuildingDestroyed(GameObject sender)
    {
        sender.GetComponent<Health>().OnDeath -= OnBuildingDestroyed;

        IBuilding building = sender.GetComponent<IBuilding>();
        _PopulationCap -= (int) building.GetBuildingDefinition().PopulationCapBoost;

        Debug.Log("Building destroyed: " + building.BuildingName);

        // If the building has a resource node (like farms do), then remove it from the resource manager.
        ResourceNode node = sender.GetComponent<ResourceNode>();
        if (node)
            _ResourceManager.RemoveResourceNode(node);
    }



    public int Population { get { return _AllVillagers.Count; } }
    public int PopulationCap {  get { return _PopulationCap; } }

}
