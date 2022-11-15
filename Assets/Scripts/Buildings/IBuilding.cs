using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IBuilding
{
    BuildingDefinition GetBuildingDefinition();
    Mesh GetMesh();

    string Category { get; }
    string Name { get; }

    GameObject gameObject { get; }
    Health HealthComponent { get; }

}
