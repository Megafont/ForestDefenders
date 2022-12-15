using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Building_WoodBridge : Building_Base
{
    protected override void InitBuilding()
    {
        ConfigureBasicBuildingSetup("Bridges", "Wood Bridge");
    }

}
