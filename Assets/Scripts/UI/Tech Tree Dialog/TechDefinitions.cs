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

        GenerateTechGroup_Farming();
        GenerateTechGroup_Housing();
        GenerateTechGroup_Walls();
        GenerateTechGroup_Towers();
        GenerateTechGroup_Bridges();
        GenerateTechGroup_Villagers();
    }


    private static void GenerateTechGroup_Farming()
    {
        string groupName = "Farming";


        List<TechTreeTileData> tilesData = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_Farm, Title = "Farm", DescriptionText = "Gain the technology to build Farms.", XPCost = 3 },
        };


        _TechTreeDialog.AddResearchGroup(groupName, tilesData);
    }

    private static void GenerateTechGroup_Housing()
    {
        string groupName = "Housing";


        List<TechTreeTileData> tilesData = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_MediumHouse, Title = "Medium House", DescriptionText = "Gain the technology to build medium size houses.", XPCost = 3 },
        };


        _TechTreeDialog.AddResearchGroup(groupName, tilesData);
    }

    private static void GenerateTechGroup_Walls()
    {
        string groupName = "Walls";


        List<TechTreeTileData> tilesData = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_WoodWall, Title = "Wood Walls", DescriptionText = "Gain the technology to build wooden walls.", XPCost = 3 },
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_StoneWall, Title = "Stone Walls", DescriptionText = "Gain the technology to build stone walls.", XPCost = 5 },
        };


        _TechTreeDialog.AddResearchGroup(groupName, tilesData);
    }

    private static void GenerateTechGroup_Towers()
    {
        string groupName = "Towers";


        List<TechTreeTileData> tilesData = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_SpikeTower, Title = "Spike Tower", DescriptionText = "Gain the technology to build spike towers.", XPCost = 3 },
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_MageTower, Title = "Mage Tower", DescriptionText = "Gain the technology to build mage towers.", XPCost = 5 },
        };


        _TechTreeDialog.AddResearchGroup(groupName, tilesData);
    }

    private static void GenerateTechGroup_Bridges()
    {
        string groupName = "Bridges";


        List<TechTreeTileData> tilesData = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_WoodBridge, Title = "Wood Bridge", DescriptionText = "Gain the technology to build wooden bridges and reach new areas.", XPCost = 3 },
        };


        _TechTreeDialog.AddResearchGroup(groupName, tilesData);
    }

    private static void GenerateTechGroup_Villagers()
    {
        string groupName = "Villagers";


        List<TechTreeTileData> tilesData = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { TechID = TechDefinitionIDs.Villagers_RepairBuildings, Title = "Repair Damaged Buildings", DescriptionText = "Villagers gain the tools to repair damaged buildings.", XPCost = 5 },
        };


        _TechTreeDialog.AddResearchGroup(groupName, tilesData);
    }

}
