using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Windows;

using StarterAssets;
using Unity.VisualScripting;
using System.Text;

public class BuildModeManager : MonoBehaviour
{
    [Tooltip("The base length of player build phases in the game (in seconds).")]
    public float BuildPhaseBaseLength = 60.0f;

    [Range(0, 1)]
    [Tooltip("The percentage of construction materials the player gets back when they destroy a building.")]
    public float PercentageOfMaterialsRecoveredOnBuildingDestruction = 1.0f;


    public bool IsBuildModeActive { get; private set; }
    public bool IsSelectingBuilding { get; private set; }



    private ThirdPersonController _Player;

    private ResourceManager _ResourceManager;
    private StarterAssetsInputs _PlayerInput;

    private BuildingConstructionGhost _BuildingConstructionGhost;

    private float _LastBuildTime;

    private string _SelectedBuildingName;
    private string _SelectedBuildingCategory;
    private GameObject _SelectedBuildingPrefab;

    private float _ConstructionOffsetFromPlayer; // Holds a float value that is how far ahead of the player the building will be built. This is recalculated each time a new building is selected since they are all different sizes.

    private RadialMenu _RadialMenu;
    private string _TempCategory; // Tracks the selected category while the second building selection menu is open.


    private void Awake()
    {
        BuildModeDefinitions.InitBuildingDefinitionLookupTables();
    }

    // Start is called before the first frame update
    void Start()
    {
        _ResourceManager = GameManager.Instance.ResourceManager;

        _Player = GameManager.Instance.Player.GetComponentInChildren<ThirdPersonController>();
        _PlayerInput = GameManager.Instance.PlayerInput;

        _RadialMenu = GameManager.Instance.UI_RadialMenu;

        InitBuildingGhost();
    }

    // Update is called once per frame
    void Update()
    {
        IsBuildModeActive = _PlayerInput.BuildMode;
        
        DoBuildModeChecks();
    }

    private void InitBuildingGhost()
    {
        GameObject buildGhostPrefab = (GameObject)Resources.Load("Structures/Prefabs/Build Mode Ghost");
        GameObject parent = GameObject.Find("Build Mode Manager");
        GameObject ghost = Instantiate(buildGhostPrefab,
                                       Vector3.zero,
                                       Quaternion.identity,
                                       parent.transform);
        _BuildingConstructionGhost = ghost.GetComponent<BuildingConstructionGhost>();

        _BuildingConstructionGhost.gameObject.SetActive(false);
    }



    private void DoBuildModeChecks()
    {
        // Do not allow build mode if the player is in midair.
        if (!_Player.Grounded || _PlayerInput.jump)
        {
            _BuildingConstructionGhost.gameObject.SetActive(false);
            return;
        }


        // If we are in build mode, the player pressed the select building button, and we are not already in the process of selecting a building, then open the buildings menu.
        if (IsBuildModeActive && _PlayerInput.SelectBuilding && !IsSelectingBuilding)
        {
            StartCoroutine(DisplaySelectBuildingMenu());
            return;
        }


        // Check if we entered or exited buildmode.
        if (!IsSelectingBuilding &&
            _BuildingConstructionGhost.gameObject.activeSelf != IsBuildModeActive)
        {
            _BuildingConstructionGhost.gameObject.SetActive(IsBuildModeActive);
        }

        // Are we in build mode?
        if (IsBuildModeActive || IsSelectingBuilding)
        {
            if (_PlayerInput.Build && !IsSelectingBuilding)
                DoBuildAction();

            // Show the building ghost so the player can see where his structure will be built.
            _BuildingConstructionGhost.gameObject.transform.position = _Player.transform.position + (_Player.transform.forward * _ConstructionOffsetFromPlayer) + (Vector3.up * 0.02f); // Position the building ghost in front of the player and add a little
                                                                                                                                                                                        // to y to prevent the ghost from colliding with the ground.
            _BuildingConstructionGhost.gameObject.transform.rotation = _Player.transform.rotation;
        }
    }

    private IEnumerator DisplaySelectBuildingMenu()
    {

        IsSelectingBuilding = true;


        // BUILDING CATEGORIES MENU
        // ----------------------------------------------------------------------------------------------------

        _RadialMenu.BottomBarText = "";
        _RadialMenu.ShowRadialMenu("Select Building Type", BuildModeDefinitions.GetBuildingCategoriesList());

        while (!_RadialMenu.MenuConfirmed && !_RadialMenu.MenuCancelled)
            yield return new WaitForSecondsRealtime(0.1f);

        _TempCategory = _RadialMenu.SelectedItemName;


        if (_RadialMenu.MenuCancelled)
        {
            IsSelectingBuilding = false;
            yield break; // The player cancelled out of the menu, so break out of this coroutine.
        }



        // This prevents the building menu we display in the line after this one from instantly closing, because the input hasn't had time to change yet.
        // So we give this slight delay so the button will be released by the time that menu is displayed.
        yield return new WaitForSeconds(0.1f);



        // BUILDINGS IN CHOSEN CATEGORY MENU
        // ----------------------------------------------------------------------------------------------------

        _RadialMenu.OnSelectionChanged += OnRadialMenuSelectionChangedHandler;
        _RadialMenu.ShowRadialMenu($"Select {_TempCategory} Building", BuildModeDefinitions.GetBuildingNamesListForCategory(_TempCategory));
        OnRadialMenuSelectionChangedHandler(null);

        while (!_RadialMenu.MenuConfirmed && !_RadialMenu.MenuCancelled)
            yield return new WaitForSecondsRealtime(0.1f);

        string building = _RadialMenu.SelectedItemName;

        _RadialMenu.OnSelectionChanged -= OnRadialMenuSelectionChangedHandler;

        if (_RadialMenu.MenuCancelled)
        {
            IsSelectingBuilding = false;

            yield break; // The player cancelled out of the menu, so break out of this coroutine.
        }



        // CLEANUP
        // ----------------------------------------------------------------------------------------------------

        SelectBuilding(_TempCategory, building);

        // This prevents the player character from attacking as soon as you close the menu, because the input hasn't had time to change yet.
        // So we give this slight delay so the button will be released by the time that check happens again.
        yield return new WaitForSecondsRealtime(0.1f);
        IsSelectingBuilding = false;

    }

    private void OnRadialMenuSelectionChangedHandler(GameObject sender)
    {
        // The user selected "Cancel" rather than a building, so return (otherwise we'd try to find a building called "Cancel").
        if (_RadialMenu.SelectedItemName == "Cancel" || _RadialMenu.SelectedItemName == "Unused")
        {
            _RadialMenu.BottomBarText = "";
            return;
        }


        StringBuilder b = new StringBuilder("Construction Cost:  ");

        List<MaterialCost> constructionCosts = BuildModeDefinitions.GetBuildingConstructionCosts(_TempCategory, _RadialMenu.SelectedItemName);

        for (int i = 0; i < constructionCosts.Count; i++)
        {
            b.Append(" ");
            b.Append($"{constructionCosts[i].Amount} {constructionCosts[i].Resource}");

            if (i < constructionCosts.Count - 1)
                b.Append(",  ");
        }

        _RadialMenu.BottomBarText = b.ToString();
    }

    public void SelectBuilding(string category, string buildingName)
    {
        string buildingKey = $"{category}/{buildingName}";

        BuildingDefinition buildingDef = BuildModeDefinitions.GetBuildingDefinition(category, buildingName);
        if (buildingDef == null)
        {
            Debug.LogError($"Could not retrieve data for the building \"{buildingKey}\"!");
            return;
        }
        else
        {
            // The selected building already was selected, so do nothing.
            if (_SelectedBuildingName == category)
                return;


            _SelectedBuildingName = buildingName;
            _SelectedBuildingCategory = category;

            _SelectedBuildingPrefab = buildingDef.Prefab.gameObject;
            
            
            if (buildingDef == null)
            {
                Debug.LogError($"Building definition not found for \"{buildingKey}\"!");
            }
            else
            {
                Mesh mesh = buildingDef.Prefab.GetComponent<MeshFilter>().sharedMesh;
                _BuildingConstructionGhost.ChangeMesh(mesh, buildingDef);
                CalculateConstructionOffsetFromPlayer(mesh);
            }
        }
    }

    private void CalculateConstructionOffsetFromPlayer(Mesh mesh)
    {
        // Calculate the construction offset distance. This is how far to offset the ghost from the player so that all buildings always
        // appear the same distance ahead of the player in build mode.
        // We want to construct each building ahead of the player by half it's own size on the Z-axis (since that one is aligned to the player),
        // and add 1 so all buildings will appear the same distance ahead of the player in build mode.
        _ConstructionOffsetFromPlayer = (mesh.bounds.size.z / 2) + _BuildingConstructionGhost.DistanceInFrontOfPlayer;
    }

    private void DoBuildAction()
    {
        if (_BuildingConstructionGhost.CanBuild &&
            Time.time - _LastBuildTime >= 0.1f)
        {
            ApplyBuildCosts();

            GameManager.Instance.VillageManager.SpawnBuilding(_SelectedBuildingPrefab,
                                                              _SelectedBuildingCategory,
                                                              _SelectedBuildingName,
                                                              _BuildingConstructionGhost.transform.position,
                                                              _BuildingConstructionGhost.transform.rotation);

            _LastBuildTime = Time.time;
        }
        else
        {
            //Debug.LogError("Can't build. Something's in the way!");
        }
    }

    public bool CanAffordBuilding(List<MaterialCost> constructionCosts)
    {
        bool result = true;


        foreach (MaterialCost cost in constructionCosts)
        {
            if (_ResourceManager.Stockpiles[cost.Resource] < cost.Amount)
            {
                result = false;
                break;
            }
        } // end foreach cost


        return result;

    }

    /// <summary>
    /// Called to deduct the construction costs of a building from the player's resource stockpiles.
    /// </summary>
    private void ApplyBuildCosts()
    {
        BuildingDefinition def = BuildModeDefinitions.GetBuildingDefinition(_SelectedBuildingCategory, _SelectedBuildingName);

        foreach (MaterialCost cost in def.ConstructionCosts)
        {
            _ResourceManager.Stockpiles[cost.Resource] -= cost.Amount;
        }
    }

    /// <summary>
    /// Called to give back materials to the player when he destroys a building.
    /// </summary>
    public void RestoreBuildingMaterials(string buildingCategory, string buildingName)
    {
        BuildingDefinition def = BuildModeDefinitions.GetBuildingDefinition(buildingCategory, buildingName);

        foreach (MaterialCost cost in def.ConstructionCosts)
        {
            _ResourceManager.Stockpiles[cost.Resource] += (int) (cost.Amount * def.PercentageOfResourcesRecoveredOnDestruction);
        }

    }

}
