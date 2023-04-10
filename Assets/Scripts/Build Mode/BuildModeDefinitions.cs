using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;


public static class BuildModeDefinitions
{
    public const string BUILDING_PREFAB_TAG = "Building Prefab";


    private static BuildModeManager _BuildModeManager;

    private static Dictionary<string, BuildingDefinition> _BuildingDefinitions;
    private static Dictionary<string, List<BuildingDefinition>> _BuildingCategories;
    
    private static List<BuildingDefinition> _Buildings_Defense;
    private static List<BuildingDefinition> _Buildings_Farming;
    private static List<BuildingDefinition> _Buildings_Housing;
    private static List<BuildingDefinition> _Buildings_Other;
    private static List<BuildingDefinition> _Buildings_Walls;

    private static float _MaterialRecoveryRate;



    static BuildModeDefinitions()
    {
        _BuildingDefinitions = new Dictionary<string, BuildingDefinition>();

        _Buildings_Defense = new List<BuildingDefinition>();
        _Buildings_Farming = new List<BuildingDefinition>();
        _Buildings_Housing = new List<BuildingDefinition>();
        _Buildings_Other = new List<BuildingDefinition>();
        _Buildings_Walls = new List<BuildingDefinition>();

        _BuildingCategories = new Dictionary<string, List<BuildingDefinition>>();
    }


    public static void InitBuildingDefinitionLookupTables()
    {
        if (IsInitialized)
            return;


        _BuildModeManager = GameManager.Instance.BuildModeManager;
        _MaterialRecoveryRate = _BuildModeManager.PercentageOfMaterialsRecoveredOnBuildingDestruction;

        InitBuildingDefinitions();
        LoadBuildingPrefabs();

        IsInitialized = true;
    }

    private static void InitBuildingDefinitions()
    {
        InitBuildDefinitions_Bridges();
        InitBuildDefinitions_Defense();
        InitBuildDefinitions_Farming();
        InitBuildDefinitions_Housing();
        InitBuildDefinitions_Walls();
    }

    private static void InitBuildDefinitions_Bridges()
    {
        string category = "Bridges";
        _BuildingCategories.Add(category, _Buildings_Other);

        CreateBuildingDefinition(category, "Wood Bridge (10m)", 250, 1, TechDefinitionIDs.Buildings_WoodBridge_10m, _MaterialRecoveryRate, 0, new Vector3(10, 2.5f, 6), false, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 500), 
              CreateMaterialCost(ResourceTypes.Stone, 500) });
        CreateBuildingDefinition(category, "Wood Bridge (20m)", 500, 2, TechDefinitionIDs.Buildings_WoodBridge_20m, _MaterialRecoveryRate, 0, new Vector3(20, 5.0f, 6), false, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 1000),
              CreateMaterialCost(ResourceTypes.Stone, 1000) });
    }

    private static void InitBuildDefinitions_Defense()
    {
        string category = "Defense";
        _BuildingCategories.Add(category, _Buildings_Defense);

        CreateBuildingDefinition(category, "Spike Tower", 100, 1, TechDefinitionIDs.NOT_APPLICABLE, _MaterialRecoveryRate, 0, new Vector3(4, 5, 4), true, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 200),
              CreateMaterialCost(ResourceTypes.Stone, 200) });
        CreateBuildingDefinition(category, "Turret", 200, 2, TechDefinitionIDs.Buildings_Turret, _MaterialRecoveryRate, 0, new Vector3(1.6f, 0.6f, 1.6f), true, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 300),
              CreateMaterialCost(ResourceTypes.Stone, 300) });
        CreateBuildingDefinition(category, "Ice Turret", 200, 2, TechDefinitionIDs.Buildings_IceTurret, _MaterialRecoveryRate, 0, new Vector3(1.2f, 2.5f, 1.2f), true, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 500),
              CreateMaterialCost(ResourceTypes.Stone, 500) });
        CreateBuildingDefinition(category, "Mage Tower", 500, 3, TechDefinitionIDs.Buildings_MageTower, _MaterialRecoveryRate, 0, new Vector3(4.6f, 6f, 4.6f), true, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 1000),
              CreateMaterialCost(ResourceTypes.Stone, 1000) });        
    }

    private static void InitBuildDefinitions_Farming()
    {
        string category = "Farming";
        _BuildingCategories.Add(category, _Buildings_Farming);

        CreateBuildingDefinition(category, "Small Garden", 50, 1, TechDefinitionIDs.NOT_APPLICABLE, _MaterialRecoveryRate, 0, new Vector3(2.1f, 0.5f, 2.1f), false, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 100),
              CreateMaterialCost(ResourceTypes.Stone, 100) });
        CreateBuildingDefinition(category, "Large Garden", 100, 2, TechDefinitionIDs.Buildings_LargeGarden, _MaterialRecoveryRate, 0, new Vector3(2.1f, 0.5f, 4.1f), false, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 500) });
        CreateBuildingDefinition(category, "Farm", 500, 3, TechDefinitionIDs.Buildings_Farm, _MaterialRecoveryRate, 0, new Vector3(4, 1.0f, 4), false, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 1000),
              CreateMaterialCost(ResourceTypes.Stone, 1000) });
    }

    private static void InitBuildDefinitions_Housing()
    {
        string category = "Housing";
        _BuildingCategories.Add(category, _Buildings_Housing);

        CreateBuildingDefinition(category, "Small House", 100, 1, TechDefinitionIDs.NOT_APPLICABLE, _MaterialRecoveryRate, 1, new Vector3(5.2f, 4.3f , 5.2f), false, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 200),
              CreateMaterialCost(ResourceTypes.Stone, 200), });
        CreateBuildingDefinition(category, "Medium House", 200, 2, TechDefinitionIDs.Buildings_MediumHouse, _MaterialRecoveryRate, 2, new Vector3(10.8f, 6.6f, 4.5f), false, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 500),
              CreateMaterialCost(ResourceTypes.Stone, 500), });
        CreateBuildingDefinition(category, "Large House", 400, 3, TechDefinitionIDs.Buildings_LargeHouse, _MaterialRecoveryRate, 4, new Vector3(10.0f, 8.6f, 9.2f), false, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 1000),
              CreateMaterialCost(ResourceTypes.Stone, 1000), });
    }

    private static void InitBuildDefinitions_Walls()
    {
        string category = "Walls";
        _BuildingCategories.Add(category, _Buildings_Walls);

        CreateBuildingDefinition(category, "Simple Fence", 20, 1, TechDefinitionIDs.NOT_APPLICABLE, _MaterialRecoveryRate, 0, new Vector3(4, 1, 0.2f), false, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 25) });
        CreateBuildingDefinition(category, "Wood Wall", 50, 2, TechDefinitionIDs.Buildings_WoodWall, _MaterialRecoveryRate, 0, new Vector3(4, 2.3f, 0.5f), false, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 50) });
        CreateBuildingDefinition(category, "Stone Wall", 100, 3, TechDefinitionIDs.Buildings_StoneWall, _MaterialRecoveryRate, 0, new Vector3(4, 1, 0.5f), false, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 100),
              CreateMaterialCost(ResourceTypes.Stone, 100), });
        CreateBuildingDefinition(category, "Stone Wall With Ramp", 100, 3, TechDefinitionIDs.Buildings_StoneWall, _MaterialRecoveryRate, 0, new Vector3(4, 1, 0.5f), false, new MaterialCost[]
            { CreateMaterialCost(ResourceTypes.Wood, 100),
              CreateMaterialCost(ResourceTypes.Stone, 100), });
    }

    private static BuildingDefinition CreateBuildingDefinition(string category, string buildingName, int maxHealth, uint buildingTier, TechDefinitionIDs techID, float percentageOfMaterialsRecoveredOnDestroy, uint populationBoost, Vector3 size, bool isRound, MaterialCost[] constructionCosts)
    {
        if (constructionCosts == null)
            throw new ArgumentNullException("The passed in construction costs list is null!");
        if (constructionCosts.Length == 0)
            throw new ArgumentException("The passed in construction costs list is empty!");


       
        BuildingDefinition def = new BuildingDefinition()
        {
            Name = buildingName,
            Category = category,
            MaxHealth = maxHealth,
            Tier = (int) buildingTier,

            TechID = techID,

            PercentageOfResourcesRecoveredOnDestruction = percentageOfMaterialsRecoveredOnDestroy,
            ConstructionCosts = new List<MaterialCost>(constructionCosts),

            PopulationCapBoost = populationBoost,

            Size = size,
            IsRound = isRound,

            // The Prefab and ConstructionGhostMesh properties are left out here on purpose, as they are set by the LoadBuildingPrefabs() function.
        };


        _BuildingDefinitions.Add($"{category}/{buildingName}", def);

        // Get the buildings list for the category in question.
        if (_BuildingCategories.TryGetValue(category, out List<BuildingDefinition> buildings))
            buildings.Add(def);
        else
            throw new Exception($"Cannot add the building \"{buildingName}\" to the list for category \"{category}\", because that list is missing!");


        return def;
    }

    private static void LoadBuildingPrefabs()
    {
        foreach (KeyValuePair<string, BuildingDefinition> pair in _BuildingDefinitions)
        {
            
            GameObject prefab = LoadBuildingPrefab(pair.Value.Category, pair.Value.Name);
            prefab.GetComponent<IBuilding>().InitAsPrefab();


            // Create a temperary instance of the prefab.
            GameObject instance = GameObject.Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
            instance.tag = BUILDING_PREFAB_TAG;


            // Get a list of all MeshFilter component's in the prefab.
            List<MeshFilter> meshFilters = new List<MeshFilter>(instance.GetComponentsInChildren<MeshFilter>());
            if (meshFilters == null)
                meshFilters = new List<MeshFilter>();

            MeshFilter parentMeshFilter = instance.GetComponent<MeshFilter>();
            if (parentMeshFilter)
                meshFilters.Add(parentMeshFilter);
               
            // Use it to generate a combined mesh including all parts of the building.
            Mesh combinedMesh = Utils_Meshes.CombineMeshes(meshFilters.ToArray(),
                                                           Resources.Load<Material>("Structures/Prefabs/Build Mode Ghost Material"));
            // Destroy the temporary instance.
            GameObject.Destroy(instance);


            // Store the prefab and construction ghost in the building definition.
            pair.Value.Prefab = prefab;
            pair.Value.ConstructionGhostMesh = combinedMesh;
        }

    }


    private static MaterialCost CreateMaterialCost(ResourceTypes resource, int amount)
    {
        return new MaterialCost { Resource = resource, Amount = amount };
    }

    public static BuildingDefinition GetBuildingDefinition(string category, string buildingName)
    {
        if (_BuildingDefinitions.TryGetValue($"{category}/{buildingName}", out BuildingDefinition def))
            return def;
        else
            throw new Exception($"Cannot retrieve the building definition for unknown building type \"{category}/{buildingName}\"!");
    }

    public static GameObject GetBuildingPrefab(string category, string buildingName)
    {
        BuildingDefinition def = GetBuildingDefinition(category, buildingName);

        return def.Prefab.gameObject;
    }

    public static List<MaterialCost> GetList_BuildingConstructionCosts(string category, string buildingName)
    {
        BuildingDefinition def = GetBuildingDefinition(category, buildingName);

        return def.ConstructionCosts;
    }

    public static string[] GetBuildingCategoriesList()
    {
        return _BuildingCategories.Keys.ToArray();
    }

    public static string[] GetList_BuildingCategoriesContainingResearchedBuildings()
    {
        List<string> categoriesWithResearchedBuildings = new List<string>();


        foreach (string category in _BuildingCategories.Keys) 
        {
            if (GetList_NamesOfResearchedbuildingsInCategory(category).Length > 0)
                categoriesWithResearchedBuildings.Add(category);
        }


        return categoriesWithResearchedBuildings.ToArray();
    }

    public static BuildingDefinition[] GetList_BuildingDefinitionsInCategory(string category)
    {
        if (_BuildingCategories.TryGetValue(category, out List<BuildingDefinition> buildings))
            return buildings.ToArray();
        else
            throw new Exception($"Cannot get menu items for unknown building category: \"{category}\"");
    }

    public static string[] GetList_BuildingNamesInCategory(string category)
    {
        BuildingDefinition[] defs;

        defs = GetList_BuildingDefinitionsInCategory(category);


        List<string> names = new List<string>();
        foreach (BuildingDefinition buildingDef in defs)
            names.Add(buildingDef.Name);

        return names.ToArray();
    }

    public static string[] GetList_NamesOfResearchedbuildingsInCategory(string category)
    {
        BuildingDefinition[] defs;

        defs = GetList_BuildingDefinitionsInCategory(category);


        GameManager gameManager = GameManager.Instance;

        List<string> names = new List<string>();
        foreach (BuildingDefinition buildingDef in defs)
        {
            // Has this building been researched yet?
            TechDefinitionIDs techId = buildingDef.TechID;
            if (techId == TechDefinitionIDs.NOT_APPLICABLE || // Building is not researchable if this is its ID
                gameManager.TechTreeDialog.IsTechnologyResearched(buildingDef.TechID))
            {
                names.Add(buildingDef.Name);
            }
        }

        return names.ToArray();
    }

    public static bool BuildingIsBridge(BuildingDefinition buildingDef)
    {
        if (buildingDef.Category == "Bridges")
            return true;


        return false;
    }

    private static GameObject LoadBuildingPrefab(string category, string buildingName)
    {
        GameObject prefab = Resources.Load<GameObject>($"Structures/Prefabs/{category}/{buildingName}");


        if (prefab)
            Debug.Log($"Loaded building prefab \"Resources/Structures/Prefabs/{category}/{buildingName}\".");
        else
            throw new Exception($"Failed to load building prefab \"Resources/Structures/Prefabs/{category}/{buildingName}\"!");


        IBuilding buildingComponent = prefab.GetComponent<IBuilding>();
        if (buildingComponent != null)
            return prefab;
        else
            throw new Exception($"The loaded building prefab \"Resources/Structures/Prefabs/{category}/{buildingName}\" does not have a building component!");

    }



    public static bool IsInitialized { get; private set; }
}
