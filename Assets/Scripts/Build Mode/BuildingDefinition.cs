using System.Collections.Generic;

using UnityEngine;


public struct MaterialCost
{
    public ResourceTypes Resource;
    public int Amount;
}


public class BuildingDefinition
{
    public string Name;
    public string Category;
    public int MaxHealth;
    public int Tier;
    public TechDefinitionIDs TechID;

    public List<MaterialCost> ConstructionCosts = new List<MaterialCost>();
    public float PercentageOfResourcesRecoveredOnDestruction;

    public uint PopulationCapBoost; // The number of villagers that will spawn after the building is constructed.

    public float Height; // Height of the building.
    // Collider data (used ESPECIALLY for the build mode construction ghost)
    public bool IsRound;
    public float Radius; // This value controls the size of the construction ghost's collider. It can't use a MeshCollider like the building prefabs
                         // do (see the comments for the _BoxCollider and _CapsuleCollider member variables in the BuildingConstructionGhost.cs file).

    public GameObject Prefab;
    public Mesh ConstructionGhostMesh;
}
