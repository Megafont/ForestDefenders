using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Building_WoodBridge_10m : Bridge_Base
{

    protected override void InitBuilding()
    {
        BridgeType = BridgeTypes.WoodBridge_10m;

        ConfigureBasicBuildingSetup("Bridges", "Wood Bridge (10m)");
    }

}
