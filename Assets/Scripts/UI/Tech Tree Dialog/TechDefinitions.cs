using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public static class TechDefinitions
{
    private static TechTreeDialog _TechTreeDialog;


    public static void GetTechDefinitions(TechTreeDialog techTreeDialog)
    {
        if (techTreeDialog == null)
            throw new Exception("The passed in TechTreeDialog is null!");


        _TechTreeDialog = techTreeDialog;

        GenerateTechGroup_Bridges();
        GenerateTechGroup_Defense();
        GenerateTechGroup_Farming();
        GenerateTechGroup_Housing();
        GenerateTechGroup_Walls();
        GenerateTechGroup_Villagers();
    }


    private static void GenerateTechGroup_Bridges()
    {
        string groupName = "Bridges";


        List<TechTreeTileData> tilesData = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_WoodBridge_10m, Title = "Wood Bridge (10m)", DescriptionText = "Gain the technology to build 10-meter long wooden bridges and reach new areas.", XPCost = 20 },
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_WoodBridge_20m, Title = "Wood Bridge (20m)", DescriptionText = "Gain the technology to build 20-meter long wooden bridges and reach new areas.", XPCost = 40 },
        };


        _TechTreeDialog.AddResearchGroup(groupName, tilesData);
    }

    private static void GenerateTechGroup_Defense()
    {
        string groupName = "Defense";


        List<TechTreeTileData> tilesData = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_Turret, Title = "Turret", DescriptionText = "Gain the technology to build simple turrets.", XPCost = 20 },
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_IceTurret, Title = "Ice Turret", DescriptionText = "Gain the technology to build ice turrets that can slow enemies.", XPCost = 30 },
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_MageTower, Title = "Mage Tower", DescriptionText = "Gain the technology to build mage towers.", XPCost = 40 },
        };


        _TechTreeDialog.AddResearchGroup(groupName, tilesData);
    }

    private static void GenerateTechGroup_Farming()
    {
        string groupName = "Farming";


        List<TechTreeTileData> tilesData = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_LargeGarden, Title = "Large Garden", DescriptionText = "Gain the technology to build large guardens.", XPCost = 20 },
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_Farm, Title = "Farm", DescriptionText = "Gain the technology to build farms.", XPCost = 40 },
        };


        _TechTreeDialog.AddResearchGroup(groupName, tilesData);
    }

    private static void GenerateTechGroup_Housing()
    {
        string groupName = "Housing";


        List<TechTreeTileData> tilesData = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_MediumHouse, Title = "Medium House", DescriptionText = "Gain the technology to build medium size houses, which increase max population by 2.", XPCost = 20 },
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_LargeHouse, Title = "Large House", DescriptionText = "Gain the technology to build large size houses, which increase max population by 4.", XPCost = 40 },
        };


        _TechTreeDialog.AddResearchGroup(groupName, tilesData);
    }

    private static void GenerateTechGroup_Walls()
    {
        string groupName = "Walls";


        List<TechTreeTileData> tilesData = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_WoodWall, Title = "Wood Walls", DescriptionText = "Gain the technology to build wooden walls.", XPCost = 10 },          
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_StoneWall, Title = "Stone Walls", DescriptionText = "Gain the technology to build stone walls.", XPCost = 20 },
        };


        _TechTreeDialog.AddResearchGroup(groupName, tilesData);
    }

    private static void GenerateTechGroup_Villagers()
    {
        string groupName = "Villagers";


        List<TechTreeTileData> tilesData = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { TechID = TechDefinitionIDs.Villagers_RepairBuildings, Title = "Repair Damaged Buildings", DescriptionText = "Villagers gain the tools to repair damaged buildings. The hard work makes them hungry, though!", XPCost = 40 },
        };


        _TechTreeDialog.AddResearchGroup(groupName, tilesData);
    }

}
