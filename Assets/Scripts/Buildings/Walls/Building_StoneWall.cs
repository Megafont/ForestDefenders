using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Building_StoneWall : Building_Base
{
    protected override void InitBuilding()
    {
        ConfigureBasicBuildingSetup("Walls", "Stone Wall");
    }
}
