using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Building_SmallHouse : Building_Base
{
    protected override void InitBuilding()
    {
        ConfigureBasicBuildingSetup("Housing", "Small House");
    }
}
