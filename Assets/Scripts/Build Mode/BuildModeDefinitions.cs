using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class BuildingDefinition
{
    public string BuildingName;
    public string Category;
    public int MaxHealth;
    public int Tier;

    public List<MaterialCost> ConstructionCosts = new List<MaterialCost>();
    public float PercentageOfResourcesRecoveredOnDestruction;

    public uint PopulationCapBoost; // The number of villagers that will spawn after the building is constructed.

    // Collider data (used ESPECIALLY for the build mode construction ghost)
    public bool IsRound;
    public float Radius; // This value controls the size of the construction ghost's collider. It can't use a MeshCollider like the building prefabs
                         // do (see the comments for the _BoxCollider and _CapsuleCollider member variables in the BuildingConstructionGhost.cs file).

    public GameObject Prefab;
    public Mesh ConstructionGhostMesh;
}

public struct MaterialCost
{
    public ResourceTypes Resource;
    public int Amount;
}


public static class BuildModeDefinitions
{
    private static BuildModeManager _BuildModeManager;
    private static Dictionary<string, BuildingDefinition> _BuildingDefinitions;
    private static List<BuildingDefinition> _Buildings_Defense;
    private static List<BuildingDefinition> _Buildings_Farming;
    private static List<BuildingDefinition> _Buildings_Housing;
    private static Dictionary<string, List<BuildingDefinition>> _BuildingCategories;

    private static float _MaterialRecoveryRate;


    static BuildModeDefinitions()
    {
        _BuildingDefinitions = new Dictionary<string, BuildingDefinition>();

        _Buildings_Defense = new List<BuildingDefinition>();
        _Buildings_Farming = new List<BuildingDefinition>();
        _Buildings_Housing = new List<BuildingDefinition>();

        _BuildingCategories = new Dictionary<string, List<BuildingDefinition>>();
    }


    public static void InitBuildingDefinitionLookupTables()
    {
        _BuildModeManager = GameManager.Instance.BuildModeManager;
        _MaterialRecoveryRate = _BuildModeManager.PercentageOfMaterialsRecoveredOnBuildingDestruction;

        InitBuildingCategories();
        InitBuildingDefinitions();
        LoadBuildingPrefabs();
    }

    private static void InitBuildingCategories()
    {
        _BuildingCategories.Add("Defense", _Buildings_Defense);
        _BuildingCategories.Add("Farming", _Buildings_Farming);
        _BuildingCategories.Add("Housing", _Buildings_Housing);
    }

    private static void InitBuildingDefinitions()
    {
        InitDefenseBuildings();
        InitFarmingBuildings();
        InitHousingBuildings();
    }


    private static void InitDefenseBuildings()
    {
        string category = "Defense";

        CreateBuildingDefinition(category, "Barricade", 20, 1, _MaterialRecoveryRate, 0, false, 0.0f, new MaterialCost[] 
            { CreateMaterialCost(ResourceTypes.Wood, 25) });
        CreateBuildingDefinition(category, "Wood Wall", 50, 2, _MaterialRecoveryRate, 0, false, 0.0f, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 50) });
        CreateBuildingDefinition(category, "Stone Wall", 100, 3, _MaterialRecoveryRate, 0, false, 0.0f, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 100),
              CreateMaterialCost(ResourceTypes.Stone, 100), });

        
        CreateBuildingDefinition(category, "Spike Tower", 200, 2, _MaterialRecoveryRate, 0, true, 4f, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 250),
              CreateMaterialCost(ResourceTypes.Stone, 250) });
        
    }

    private static void InitFarmingBuildings()
    {
        string category = "Farming";

        CreateBuildingDefinition(category, "Small Garden", 50, 1, _MaterialRecoveryRate, 0, false, 0.0f, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 50) });
        CreateBuildingDefinition(category, "Farm", 100, 2, _MaterialRecoveryRate, 0, false, 0.0f, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 100) });

    }

    private static void InitHousingBuildings()
    {
        string category = "Housing";

        CreateBuildingDefinition(category, "Small House", 100, 1, _MaterialRecoveryRate, 1, false, 0.0f, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 50),
              CreateMaterialCost(ResourceTypes.Stone, 50), });
        CreateBuildingDefinition(category, "Medium House", 150, 2, _MaterialRecoveryRate, 2, false, 0.0f, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 100),
              CreateMaterialCost(ResourceTypes.Stone, 100), });

    }


    private static BuildingDefinition CreateBuildingDefinition(string category, string buildingName, int maxHealth, int buildingTier, float percentageOfMaterialsRecoveredOnDestroy, uint populationBoost, bool isRound, float radius, MaterialCost[] constructionCosts)
    {
        if (constructionCosts == null)
            throw new ArgumentNullException("The passed in construction costs list is null!");
        if (constructionCosts.Length == 0)
            throw new ArgumentException("The passed in construction costs list is empty!");


       
        BuildingDefinition def = new BuildingDefinition()
        {
            BuildingName = buildingName,
            Category = category,
            MaxHealth = maxHealth,
            Tier = buildingTier,

            PercentageOfResourcesRecoveredOnDestruction = percentageOfMaterialsRecoveredOnDestroy,
            ConstructionCosts = new List<MaterialCost>(constructionCosts),

            PopulationCapBoost = populationBoost,

            IsRound = isRound,
            Radius = radius,

            // The Prefab and ConstructionGhostMesh properties are left out here on purpose, as they are set by the LoadBuildingPrefabs() function.
        };


        _BuildingDefinitions.Add($"{category}/{buildingName}", def);

        // Get the buildings list for the category in question.
        if (_BuildingCategories.TryGetValue(category, out List<BuildingDefinition> buildings))
            buildings.Add(def);
        else
            throw new Exception($"Cannot add the building \"{buildingName}\" to the list for category \"{category}\", because that list is missing!");


        return def;
    }

    private static void LoadBuildingPrefabs()
    {
        foreach (KeyValuePair<string, BuildingDefinition> pair in _BuildingDefinitions)
        {
            
            GameObject prefab = LoadBuildingPrefab(pair.Value.Category, pair.Value.BuildingName);


            // Create a temperary instance of the prefab.
            GameObject instance = GameObject.Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);

            // Get a list of all MeshFilter component's in the prefab.
            List<MeshFilter> meshFilters = new List<MeshFilter>(instance.GetComponentsInChildren<MeshFilter>());
            if (meshFilters == null)
                meshFilters = new List<MeshFilter>();

            MeshFilter parentMeshFilter = instance.GetComponent<MeshFilter>();
            if (parentMeshFilter)
                meshFilters.Add(parentMeshFilter);
               
            // Use it to generate a combined mesh including all parts of the building.
            Mesh combinedMesh = Utils_Meshes.CombineMeshes(meshFilters.ToArray(),
                                                           Resources.Load<Material>("Structures/Prefabs/Build Mode Ghost Material"));
            // Destroy the temporary instance.
            GameObject.Destroy(instance);


            // Store the prefab and construction ghost in the building definition.
            pair.Value.Prefab = prefab;
            pair.Value.ConstructionGhostMesh = combinedMesh;
        }

    }


    private static MaterialCost CreateMaterialCost(ResourceTypes resource, int amount)
    {
        return new MaterialCost { Resource = resource, Amount = amount };
    }

    public static BuildingDefinition GetBuildingDefinition(string category, string buildingName)
    {
        if (_BuildingDefinitions.TryGetValue($"{category}/{buildingName}", out BuildingDefinition def))
            return def;
        else
            throw new Exception($"Cannot retrieve the building definition for unknown building type \"{category}/{buildingName}\"!");
    }

    public static GameObject GetBuildingPrefab(string category, string buildingName)
    {
        BuildingDefinition def = GetBuildingDefinition(category, buildingName);

        return def.Prefab.gameObject;
    }

    public static List<MaterialCost> GetBuildingConstructionCosts(string category, string buildingName)
    {
        BuildingDefinition def = GetBuildingDefinition(category, buildingName);

        return def.ConstructionCosts;
    }

    public static string[] GetBuildingCategoriesList()
    {
        return _BuildingCategories.Keys.ToArray();
    }

    public static BuildingDefinition[] GetBuildingDefinitionsListForCategory(string category)
    {
        if (_BuildingCategories.TryGetValue(category, out List<BuildingDefinition> buildings))
            return buildings.ToArray();
        else
            throw new Exception($"Cannot get menu items for unknown building category: \"{category}\"");
    }

    public static string[] GetBuildingNamesListForCategory(string category)
    {
        BuildingDefinition[] defs;

        defs = GetBuildingDefinitionsListForCategory(category);


        List<string> names = new List<string>();
        foreach (BuildingDefinition buildingDef in defs)
            names.Add(buildingDef.BuildingName);

        return names.ToArray();
    }

    private static GameObject LoadBuildingPrefab(string category, string buildingName)
    {
        GameObject prefab = Resources.Load<GameObject>($"Structures/Prefabs/{category}/{buildingName}");


        if (prefab)
            Debug.Log($"Loaded building prefab \"Resources/Structures/Prefabs/{category}/{buildingName}\".");
        else
            throw new Exception($"Failed to load building prefab \"Resources/Structures/Prefabs/{category}/{buildingName}\"!");


        IBuilding buildingComponent = prefab.GetComponent<IBuilding>();
        if (buildingComponent != null)
            return prefab;
        else
            throw new Exception("The loaded building prefab does not have a building component!");

    }


}
