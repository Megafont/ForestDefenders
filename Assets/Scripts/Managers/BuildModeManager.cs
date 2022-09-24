using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Windows;

using StarterAssets;



public class BuildModeManager : MonoBehaviour
{
    [Tooltip("The base length of player build phases in the game (in seconds).")]
    public float BuildPhaseBaseLength = 60.0f;


    public bool IsBuildModeActive { get; private set; }
    public bool IsSelectingBuilding { get; private set; }



    private ThirdPersonController _Player;
    private StarterAssetsInputs _PlayerInput;

    private BuildingConstructionGhost _BuildingConstructionGhost;

    private float _LastBuildTime;

    private string _SelectedBuildingName;
    private string _SelectedBuildingCategory;
    private GameObject _SelectedBuildingPrefab;

    private float _ConstructionOffsetFromPlayer; // Holds a float value that is how far ahead of the player the building will be built. This is recalculated each time a new building is selected since they are all different sizes.

    private RadialMenu _RadialMenu;



    // Start is called before the first frame update
    void Start()
    {
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

        _RadialMenu.ShowRadialMenu("Select Building Type", BuildModeDefinitions.GetBuildingCategoriesList());

        while (!_RadialMenu.MenuConfirmed && !_RadialMenu.MenuCancelled)
            yield return new WaitForSecondsRealtime(0.1f);

        string category = _RadialMenu.SelectedItemName;


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

        _RadialMenu.ShowRadialMenu($"Select {category} Building", BuildModeDefinitions.GetBuildingNamesListForCategory(category));

        while (!_RadialMenu.MenuConfirmed && !_RadialMenu.MenuCancelled)
            yield return new WaitForSecondsRealtime(0.1f);

        string building = _RadialMenu.SelectedItemName;


        if (_RadialMenu.MenuCancelled)
        {
            IsSelectingBuilding = false;
            yield break; // The player cancelled out of the menu, so break out of this coroutine.
        }



        // CLEANUP
        // ----------------------------------------------------------------------------------------------------

        SelectBuilding(category, building);

        // This prevents the player character from attacking as soon as you close the menu, because the input hasn't had time to change yet.
        // So we give this slight delay so the button will be released by the time that check happens again.
        yield return new WaitForSecondsRealtime(0.1f);
        IsSelectingBuilding = false;

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
                Debug.LogError($"Build definition not found for \"{buildingKey}\"!");
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




}
