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

    public List<MaterialCost> ConstructionCosts = new List<MaterialCost>();
    public float PercentageOfResourcesRecoveredOnDestruction;

    public uint PopulationBoost; // The number of villagers that will spawn after the building is constructed.

    // Collider data (used ESPECIALLY for the build mode construction ghost)
    public bool IsRound;
    public float Radius; // This value controls the size of the construction ghost's collider. It can't use a MeshCollider like the building prefabs
                         // do (see the comments for the _BoxCollider and _CapsuleCollider member variables in the BuildingConstructionGhost.cs file).

    public GameObject Prefab;
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

        CreateBuildingDefinition(category, "Barricade", 20, _MaterialRecoveryRate, 0, false, 0.0f, new MaterialCost[] 
            { CreateMaterialCost(ResourceTypes.Wood, 10) });
    }

    private static void InitFarmingBuildings()
    {
        string category = "Farming";

        CreateBuildingDefinition(category, "Small Garden", 50, _MaterialRecoveryRate, 0, false, 0.0f, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 20) });
    }

    private static void InitHousingBuildings()
    {
        string category = "Housing";

        CreateBuildingDefinition(category, "Small House", 100, _MaterialRecoveryRate, 1, false, 0.0f, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 40),
              CreateMaterialCost(ResourceTypes.Stone, 20), });
    }


    private static BuildingDefinition CreateBuildingDefinition(string category, string buildingName, int maxHealth, float percentageOfMaterialsRecoveredOnDestroy, uint populationBoost, bool isRound, float radius, MaterialCost[] constructionCosts)
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

            PercentageOfResourcesRecoveredOnDestruction = percentageOfMaterialsRecoveredOnDestroy,
            ConstructionCosts = new List<MaterialCost>(constructionCosts),

            PopulationBoost = populationBoost,

            IsRound = isRound,
            Radius = radius,

            Prefab = LoadBuildingPrefab(category, buildingName),
        };


        _BuildingDefinitions.Add($"{category}/{buildingName}", def);

        // Get the buildings list for the category in question.
        if (_BuildingCategories.TryGetValue(category, out List<BuildingDefinition> buildings))
            buildings.Add(def);
        else
            throw new Exception($"Cannot add the building \"{buildingName}\" to the list for category \"{category}\", because that list is missing!");


        return def;
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
