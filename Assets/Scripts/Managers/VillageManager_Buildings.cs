using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Random = UnityEngine.Random;


public class VillageManager_Buildings : MonoBehaviour
{
    public GameObject BuildingsParent;


    [Header("Buildings Settings")]


    private BuildModeManager _BuildModeManager;
    private NavMeshManager _NavMeshManager;
    private ResourceManager _ResourceManager;

    private Dictionary<string, GameObject> _BuildingCategoryParents;


    public delegate void VillageManager_OnBuildingConstructedHandler(IBuilding building);
    public delegate void VillageManager_OnBuildingDestroyedHandler(IBuilding building, bool wasDeconstructedByPlayer);

    public event VillageManager_OnBuildingConstructedHandler OnBuildingConstructed;
    public event VillageManager_OnBuildingDestroyedHandler OnBuildingDestroyed;

    


    private void Awake()
    {
        _BuildingCategoryParents = new Dictionary<string, GameObject>();
        DamagedBuildingsDictionary = new Dictionary<IBuilding, GameObject>();

        _BuildModeManager = GameManager.Instance.BuildModeManager;
        _NavMeshManager = GameManager.Instance.NavMeshManager;
        _ResourceManager = GameManager.Instance.ResourceManager;

        FindTownCenter();

        InitBuildingCategoryParentObjects();
        FindPreExistingBuildings();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("C1: " + GetBuildingCount("Walls", "Simple Fence"));
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
    }



    private void FindTownCenter()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Town Center");
        if (objs.Length > 1)
            Debug.LogWarning("There should only be one Town Center in the level!");
        else if (objs.Length < 1)
            throw new Exception("This level does not contain a Town Center!");

        TownCenter = objs[0];
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
            foreach (string buildingName in BuildModeDefinitions.GetList_BuildingNamesInCategory(category))
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

    private void FindPreExistingBuildings()
    {
        IBuilding[] buildings = FindObjectsOfType<Building_Base>();
        foreach (IBuilding building in buildings)
        {
            if (building != null && building.gameObject.tag != "Building Prefab")
            {
                AddBuilding(building);
                //Debug.Log($"Found pre-existing building: {building.Category}/{building.Name} at {building.gameObject.transform.position}");
            }

        } // end foreach obj

    }



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
        
        
        // If this building provides a resource, clear the node so it doesn't provide any resources until the next player build phase.
        ResourceNode resourceNode = newBuilding.GetComponent<ResourceNode>();
        if (resourceNode)
            resourceNode.ClearNode();


        IBuilding building = newBuilding.GetComponent<IBuilding>();
        AddBuilding(building);
        
        return newBuilding;
    }

    public void DeconstructBuilding(IBuilding building)
    {
        if (building == null)
        {
            Debug.LogError("Cannot deconstruct the building, because it is null!");
            return;
        }


        _BuildModeManager.RestoreBuildingMaterials(building.Category, building.Name);

        OnBuildingDestroyed?.Invoke(building, true);

        RemoveBuilding(building);

        Destroy(building.gameObject);
    }


    private void AddBuilding(IBuilding building)
    {
        TotalBuildingCount++;

        _BuildingCategoryParents.TryGetValue($"{building.Category}/{building.Name}", out GameObject parent);

        // Set the parent of the building.
        // If the building returns "None" for both category and building name, then it will be added to the "Unknown" parent object.
        // If the building's category and name are not found in the dictionary, then it will be added to the "Unknown" parent object.
        if (parent)
            building.gameObject.transform.parent = parent.transform;
        else
            building.gameObject.transform.parent = _BuildingCategoryParents["None/None"].transform;


        building.gameObject.GetComponent<Health>().OnDeath += OnBuildingDestroyedHandler;
        building.gameObject.GetComponent<Health>().OnHeal += OnBuildingHealedHandler;
        building.gameObject.GetComponent<Health>().OnTakeDamage += OnBuildingDamagedHandler;


        // If the building is a bridge, remove it from the NavMeshManager and regenerate the navmesh.
        if (building.Category == "Bridges")
        {
            _NavMeshManager.RegenerateAllNavMeshes();
        }


        OnBuildingConstructed?.Invoke(building);


        // If the building has a resource node (like farms do), then add it to the resource manager.
        ResourceNode node = building.gameObject.GetComponent<ResourceNode>();
        if (node)
            _ResourceManager.AddResourceNode(node);
    }

    private void RemoveBuilding(IBuilding building)
    {
        TotalBuildingCount--;


        // Debug.Log("Building destroyed: " + building.Name);


        building.gameObject.GetComponent<Health>().OnDeath -= OnBuildingDestroyedHandler;
        building.gameObject.GetComponent<Health>().OnHeal -= OnBuildingHealedHandler;
        building.gameObject.GetComponent<Health>().OnTakeDamage -= OnBuildingDamagedHandler;


        // If the building has a resource node (like farms do), then remove it from the resource manager.
        ResourceNode node = building.gameObject.GetComponent<ResourceNode>();
        if (node)
            _ResourceManager.RemoveResourceNode(node);


        // If the building is a bridge, remove it from the NavMeshManager and regenerate the navmesh.
        if (building.Name == "Wood Bridge")
        {
            //_NavMeshManager.RegenerateAllNavMeshes();
            StartCoroutine(WaitForBuildingToDespawn(building.gameObject));
        }

    }

    private IEnumerator WaitForBuildingToDespawn(GameObject building)
    {
        while (building != null)
            yield return null;


        _NavMeshManager.RegenerateAllNavMeshes();
    }



    // EVENT METHODS
    // ========================================================================================================================================================================================================

    private void OnBuildingDestroyedHandler(GameObject sender, GameObject attacker)
    {
        sender.GetComponent<Health>().OnDeath -= OnBuildingDestroyedHandler;

        IBuilding building = sender.GetComponent<IBuilding>();

        RemoveBuilding(building);

        OnBuildingDestroyed?.Invoke(building, false);
    }

    private void OnBuildingDamagedHandler(GameObject sender, GameObject attacker, float amount, DamageTypes damageType)    
    {
        IBuilding building = sender.GetComponent<IBuilding>();

        if (!DamagedBuildingsDictionary.ContainsKey(building))
            DamagedBuildingsDictionary.Add(building, attacker);
    }

    private void OnBuildingHealedHandler(GameObject sender, GameObject healer, float amount)
    {
        IBuilding building = sender.GetComponent<IBuilding>();

        if (building.HealthComponent.CurrentHealth == building.HealthComponent.MaxHealth)
            DamagedBuildingsDictionary.Remove(building);
    }



    // PROPERTIES
    // ========================================================================================================================================================================================================

    public Dictionary<IBuilding, GameObject> DamagedBuildingsDictionary { get; private set; }
    public GameObject TownCenter { get; private set; }
    public int TotalBuildingCount { get; private set; }

}
