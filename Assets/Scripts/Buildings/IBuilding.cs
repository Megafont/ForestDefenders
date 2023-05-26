using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IBuilding
{
    BuildingDefinition GetBuildingDefinition();
    Mesh GetMesh();
    void InitAsPrefab();
    void Deconstruct(GameObject sender);


    string Category { get; }
    string Name { get; }

    GameObject gameObject { get; }
    Health HealthComponent { get; }

    AudioSource AudioSource { get; }

    bool IsDeconstructing { get; }

}
