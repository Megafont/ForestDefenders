using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IBuilding
{
    BuildingDefinition GetBuildingDefinition();

    string BuildingCategory { get; }
    string BuildingName { get; }

    GameObject gameObject { get; }
    Health HealthComponent { get; }

}