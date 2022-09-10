using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public struct BuildingDefinition
{
    public string Name;
    public bool IsRound;
    public float Radius;
}


public static class BuildModeDefinitions
{

    public const float BuildPhaseBaseLength = 300f;


    public static string[] BuildCategoriesMenu = new string[]
    {
        "Defense",
        "Farming",
        "Housing",
    };


    public static BuildingDefinition[] Buildings_Defense = new BuildingDefinition[]
    {
        CreateBuildingDefinition("Barricade", false),
    };


    public static BuildingDefinition? GetBuildingDefinition(string category, string buildingName)
    {
        BuildingDefinition[] defs;

        defs = GetBuildingDefinitionsList(category);

        foreach (BuildingDefinition buildingDef in defs)
        {
            if (buildingDef.Name == buildingName)
                return buildingDef;
        }


        return null;
    }

    public static BuildingDefinition[] GetBuildingDefinitionsList(string category)
    {
        switch (category)
        {
            case "Defense": // Defense
                return Buildings_Defense;

            default:
                throw new Exception($"Cannot get menu items for unknown building category: \"{category}\"");
        }

    }    

    public static string[] GetBuildingNamesList(string category)
    {
        BuildingDefinition[] defs;

        defs = GetBuildingDefinitionsList(category);


        List<string> names = new List<string>();
        foreach (BuildingDefinition buildingDef in defs)
            names.Add(buildingDef.Name);

        return names.ToArray();
    }


    private static BuildingDefinition CreateBuildingDefinition(string name, bool isRound = false, float radius = 0.0f)
    {
        return new BuildingDefinition()
        {
            Name = name,
            IsRound = isRound,
            Radius = radius,
        };
    }

}
