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

        GenerateWallsTechGroup();
        GenerateVillagersTechGroup();
    }


    private static void GenerateWallsTechGroup()
    {
        string groupName = "Walls";

        List<TechTreeTileData> tilesData = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_WoodWall, Title = "Wood Walls", DescriptionText = "Gain the technology to build wooden walls.", XPCost = 10 },
            new TechTreeTileData() { TechID = TechDefinitionIDs.Buildings_StoneWall, Title = "Stone Walls", DescriptionText = "Gain the technology to build stone walls.", XPCost = 10 },
        };


        _TechTreeDialog.AddResearchGroup(groupName, tilesData);
    }

    private static void GenerateVillagersTechGroup()
    {
        string groupName = "Villagers";


        List<TechTreeTileData> tilesData = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { TechID = TechDefinitionIDs.Villagers_RepairBuildings, Title = "Repair Damaged Buildings", DescriptionText = "Villagers gain the tools to repair damaged buildings.", XPCost = 10 },
        };


        _TechTreeDialog.AddResearchGroup(groupName, tilesData);
    }

}
