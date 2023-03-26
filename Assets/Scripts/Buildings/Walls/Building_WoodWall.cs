using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Building_WoodWall : Building_Base
{
    protected override void InitBuilding()
    {
        ConfigureBasicBuildingSetup("Walls", "Wood Wall");
    }
}
