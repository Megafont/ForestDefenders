using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IBuilding
{
    BuildingDefinition GetBuildingDefinition();
    Mesh GetMesh();

    string BuildingCategory { get; }
    string BuildingName { get; }

    GameObject gameObject { get; }
    Health HealthComponent { get; }

}
