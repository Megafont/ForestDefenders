using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Building_Barricade : Building_Base
{
    protected override void InitBuilding()
    {
        ConfigureBasicBuildingSetup("Defense", "Barricade");
    }
}
