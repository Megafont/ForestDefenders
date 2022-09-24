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
    public bool IsRound;
    public float Radius;

    public GameObject Prefab;
}


public static class BuildModeDefinitions
{
    private static Dictionary<string, BuildingDefinition> _BuildingDefinitions;
    private static List<BuildingDefinition> _Buildings_Defense;
    private static List<BuildingDefinition> _Buildings_Farming;
    private static List<BuildingDefinition> _Buildings_Housing;
    private static Dictionary<string, List<BuildingDefinition>> _BuildingCategories;

    public const float BuildPhaseBaseLength = 300f;



    static BuildModeDefinitions()
    {
        _BuildingDefinitions = new Dictionary<string, BuildingDefinition>();

        _Buildings_Defense = new List<BuildingDefinition>();
        _Buildings_Farming = new List<BuildingDefinition>();
        _Buildings_Housing = new List<BuildingDefinition>();

        _BuildingCategories = new Dictionary<string, List<BuildingDefinition>>();


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

        CreateBuildingDefinition(category, "Barricade", 20, false);
    }

    private static void InitFarmingBuildings()
    {
        string category = "Farming";

        CreateBuildingDefinition(category, "Small Garden", 50, false);
    }

    private static void InitHousingBuildings()
    {
        string category = "Housing";

        CreateBuildingDefinition(category, "Small House", 100, false);
    }

    private static BuildingDefinition CreateBuildingDefinition(string category, string buildingName, int maxHealth, bool isRound = false, float radius = 0.0f)
    {
        BuildingDefinition def = new BuildingDefinition()
        {
            BuildingName = buildingName,
            Category = category,
            MaxHealth = maxHealth,
            IsRound = isRound,
            Prefab = LoadBuildingPrefab(category, buildingName),
            Radius = radius,
        };

        _BuildingDefinitions.Add($"{category}/{buildingName}", def);

        // Get the buildings list for the category in question.
        if (_BuildingCategories.TryGetValue(category, out List<BuildingDefinition> buildings))
            buildings.Add(def);
        else
            throw new Exception($"Cannot add the building \"{buildingName}\" to the list for category \"{category}\", because that list is missing!");


        return def;
    }


    public static BuildingDefinition GetBuildingDefinition(string category, string buildingName)
    {
        if (_BuildingDefinitions.TryGetValue($"{category}/{buildingName}", out BuildingDefinition def))
            return def;
        else
            throw new Exception($"Cannot retrieve the building definition for unknown building type {category}/{buildingName}!");
    }

    public static GameObject GetBuildingPrefab(string category, string buildingName)
    {
        BuildingDefinition def = GetBuildingDefinition(category, buildingName);

        return def.Prefab.gameObject;
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
