using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Building_SpikeTower : Building_Base
{
    protected override void InitBuilding()
    {
        ConfigureBasicBuildingSetup("Defense", "Spike Tower");
    }


    public override Mesh GetMesh()
    {
        GameObject towerBody = transform.Find("Spike Tower Armature/Tower Base Bone/Spike Tower Body").gameObject;

        return towerBody.GetComponent<MeshFilter>().sharedMesh;
    }

}
