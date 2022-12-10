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

        GenerateVillagersTechGroup();
    }


    private static void GenerateVillagersTechGroup()
    {
        string groupName = "Villagers";


        List<TechTreeTileData> tilesData = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { Title = "Repair Damaged Buildings", DescriptionText = "Villagers gain the tools to repair damaged buildings.", XPCost = 10 },
        };


        _TechTreeDialog.AddResearchGroup(groupName, tilesData);
    }

}
