using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Building_WoodBridge_20m : Bridge_Base
{

    protected override void InitBuilding()
    {
        BridgeType = BridgeTypes.WoodBridge_20m;

        ConfigureBasicBuildingSetup("Bridges", "Wood Bridge (20m)");
    }

}
