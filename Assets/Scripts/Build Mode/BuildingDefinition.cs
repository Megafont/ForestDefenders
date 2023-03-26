using System.Collections.Generic;

using UnityEngine;


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

    public Vector3 Size; // Dimensions of the building.
    // Collider data (used ESPECIALLY for the build mode construction ghost)
    public bool IsRound;

    public GameObject Prefab;
    public Mesh ConstructionGhostMesh;
}
