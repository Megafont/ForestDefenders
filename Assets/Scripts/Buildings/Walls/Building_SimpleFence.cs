using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Building_SimpleFence : Building_Base
{
    protected override void InitBuilding()
    {
        ConfigureBasicBuildingSetup("Walls", "Simple Fence");
    }
}
