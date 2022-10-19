using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor.Animations;
using UnityEngine;
using Random = UnityEngine.Random;


public class VillageManager : MonoBehaviour
{
    public GameObject BuildingsParent;
    public GameObject VillagersParent;

    [Tooltip("The delay between spawning queued up villagers (assuming the spawn location is not blocked).")]
    public float VillagerSpawnDelay = 2.0f;


    private Dictionary<string, GameObject> _BuildingCategoryParents;
    
    private Dictionary<string, GameObject> _VillagerTypeParents;
    private Dictionary<string, GameObject> _VillagerPrefabs;
    private List<IVillager> _AllVillagers;

    private GameObject _TownCenter;

    private Queue<GameObject> _VillagerSpawningQueue;
    private WaitForSeconds _VillagerSpawnWaitTime;



    // Start is called before the first frame update
    void Start()
    {
        _BuildingCategoryParents = new Dictionary<string, GameObject>();

        _VillagerTypeParents = new Dictionary<string, GameObject>();
        _VillagerPrefabs = new Dictionary<string, GameObject>();
        _AllVillagers = new List<IVillager>();

        _VillagerSpawningQueue = new Queue<GameObject>();
        _VillagerSpawnWaitTime = new WaitForSeconds(VillagerSpawnDelay);

        
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Town Center");
        if (objs.Length > 1)
            Debug.LogWarning("There should only be one Town Center in the level!");
        else if (objs.Length < 1)
            throw new Exception("This level does not contain a Town Center!");
        
        _TownCenter = objs[0];
        

        InitVillagerTypes();

        InitBuildingCategoryParentObjects();
        FindPreExistingBuildings();

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
    
    public GameObject SpawnBuilding(GameObject buildingPrefab, string buildingCategory, string buildingName, Vector3 position, Quaternion rotation)
    {
        GameObject newBuilding = Instantiate(buildingPrefab, position, rotation);
        
        Transform parent = _BuildingCategoryParents[$"{buildingCategory}/{buildingName}"].transform;
        newBuilding.transform.parent = parent;


        IBuilding building = newBuilding.GetComponent<IBuilding>();           
        for (int i = 0; i < building.GetBuildingDefinition().PopulationBoost; i++)
            _VillagerSpawningQueue.Enqueue(SelectVillagerPrefab());


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

    private void FindPreExistingBuildings()
    {
        GameObject[] objects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in objects)
        {
            IBuilding building = obj.GetComponent<IBuilding>();
            if (building != null)
            {
                _BuildingCategoryParents.TryGetValue($"{building.BuildingCategory}/{building.BuildingName}", out GameObject parent);

                // Set the parent of the building.
                // If the building returns "None" for both category and building name, then it will be added to the "Unknown" parent object.
                // If the building's category and name are not found in the dictionary, then it will be added to the "Unknown" parent object.
                if (parent)
                    obj.transform.parent = parent.transform;
                else
                    obj.transform.parent = _BuildingCategoryParents["None/None"].transform;


                for (int i = 0; i < building.GetBuildingDefinition().PopulationBoost; i++)
                    _VillagerSpawningQueue.Enqueue(SelectVillagerPrefab());


                //Debug.Log($"Found pre-existing building: {building.BuildingCategory}/{building.BuildingName}");
            }

        } // end foreach obj

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
                GameObject parent = new GameObject(t.Name);
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
            if (_VillagerSpawningQueue.Count > 0)
            {
                GameObject prefab = _VillagerSpawningQueue.Dequeue();

                GameObject newVillager = Instantiate(prefab,
                                                     _TownCenter.transform.position,
                                                     _TownCenter.transform.rotation,
                                                     _VillagerTypeParents[prefab.name].transform);

                IVillager villagerComponent = newVillager.GetComponent<IVillager>();
                _AllVillagers.Add(villagerComponent);
                villagerComponent.HealthComponent.OnDeath += OnVillagerDeath;
            }

            yield return _VillagerSpawnWaitTime;

        } // end while

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

}
