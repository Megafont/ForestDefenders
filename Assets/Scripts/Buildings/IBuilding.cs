using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IBuilding
{
    public string BuildingCategory { get; }
    public string BuildingName { get; }

    public GameObject gameObject { get; }
    public Health HealthComponent { get; }

}
