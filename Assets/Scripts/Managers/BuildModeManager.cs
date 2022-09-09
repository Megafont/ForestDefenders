using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Windows;

using StarterAssets;



public class BuildModeManager : MonoBehaviour
{
    public bool IsBuildModeActive { get; private set; }
    public bool IsSelectingBuilding { get; private set; }



    private Dictionary<string, GameObject> _BuildingPrefabs;

    private ThirdPersonController _Player;
    private StarterAssetsInputs _PlayerInput;

    private GameObject _BuildingConstructionGhostObject;
    private BuildingConstructionGhost _BuildingConstructionGhost;
    private float _LastBuildTime;

    private string _SelectedBuildingName;
    private GameObject _SelectedBuilding;


    private RadialMenu _RadialMenu;



    // Start is called before the first frame update
    void Start()
    {
        _BuildingPrefabs = new Dictionary<string, GameObject>();

        _Player = GameManager.Instance.Player.GetComponentInChildren<ThirdPersonController>();
        _PlayerInput = GameManager.Instance.PlayerInput;

        _RadialMenu = GameManager.Instance.UI_RadialMenu;

        InitBuildingGhost();
        LoadBuildingModels();
    }

    // Update is called once per frame
    void Update()
    {
        DoBuildModeChecks();
    }

    private void InitBuildingGhost()
    {
        GameObject buildGhostPrefab = (GameObject)Resources.Load("Structures/Prefabs/Build Mode Ghost");
        GameObject parent = GameObject.Find("Build Mode Manager");
        _BuildingConstructionGhostObject = Instantiate(buildGhostPrefab,
                                       Vector3.zero,
                                       Quaternion.identity,
                                       parent.transform);

        _BuildingConstructionGhost = _BuildingConstructionGhostObject.GetComponent<BuildingConstructionGhost>();

        _BuildingConstructionGhostObject.SetActive(false);
    }



    private void DoBuildModeChecks()
    {
        // Do not allow build mode if the player is in midair.
        if (!_Player.Grounded || _PlayerInput.jump)
        {
            _BuildingConstructionGhostObject.SetActive(false);
            return;
        }


        // If we are in build mode, the player pressed the select building button, and we are not already in the process of selecting a building, then open the buildings menu.
        if (_PlayerInput.BuildMode && _PlayerInput.SelectBuilding && !IsSelectingBuilding)
        {
            StartCoroutine(DisplaySelectBuildingMenu());
            return;
        }


        // Check if we entered or exited buildmode.
        if (!IsSelectingBuilding &&
            _BuildingConstructionGhostObject.activeSelf != _PlayerInput.BuildMode)
        {
            _BuildingConstructionGhostObject.SetActive(_PlayerInput.BuildMode);
        }

        // Are we in build mode?
        if (_PlayerInput.BuildMode || IsSelectingBuilding)
        {
            if (_PlayerInput.Build && !IsSelectingBuilding)
                DoBuildAction();

            // Show the building ghost so the player can see where his structure will be built.
            _BuildingConstructionGhostObject.transform.position = _Player.transform.position + (_Player.transform.forward * 2f) + (Vector3.up * 0.02f); // Position the building ghost in front of the player and add a little to y to prevent the ghost from colliding with the ground.
            _BuildingConstructionGhostObject.transform.rotation = _Player.transform.rotation;
        }
    }

    private IEnumerator DisplaySelectBuildingMenu()
    {

        IsSelectingBuilding = true;


        // BUILDING CATEGORIES MENU
        // ----------------------------------------------------------------------------------------------------

        _RadialMenu.ShowRadialMenu("Select Building Type", BuildModeDefinitions.BuildCategoriesMenu);

        while (!_RadialMenu.MenuConfirmed && !_RadialMenu.MenuCancelled)
            yield return new WaitForSecondsRealtime(0.1f);

        string category = _RadialMenu.SelectedItemName;
        if (_RadialMenu.MenuConfirmed)
            Debug.Log($"Building category selected: \"{category}\"");
        else if (_RadialMenu.MenuCancelled)
            Debug.Log("Building category selection cancelled.");

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

        _RadialMenu.ShowRadialMenu($"Select {category} Building", BuildModeDefinitions.GetBuildingNamesList(category));

        while (!_RadialMenu.MenuConfirmed && !_RadialMenu.MenuCancelled)
            yield return new WaitForSecondsRealtime(0.1f);

        string building = _RadialMenu.SelectedItemName;
        if (_RadialMenu.MenuConfirmed)
            Debug.Log($"Building selection confirmed. Selected \"{building}\"");
        else if (_RadialMenu.MenuCancelled)
            Debug.Log("Building selection cancelled.");

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

    public void SelectBuilding(string category, string building)
    {
        if (_BuildingPrefabs == null)
            return;

        string buildingKey = $"{category}/{building}";


        if (!_BuildingPrefabs.TryGetValue(buildingKey, out GameObject prefab))
        {
            Debug.LogError($"Could not retrieve building prefab \"{name}\"!");
            return;
        }
        else
        {
            // The selected building already was selected, so do nothing.
            if (_SelectedBuildingName == name)
                return;

            _SelectedBuildingName = name;
            _SelectedBuilding = prefab;

            BuildingDefinition? buildingDef = BuildModeDefinitions.GetBuildingDefinition(category, building);
            if (buildingDef == null)
            {
                Debug.LogError($"Build definition not found for \"{buildingKey}\"!");
            }
            else
            {
                _BuildingConstructionGhost.ChangeMesh(prefab.GetComponent<MeshFilter>().sharedMesh, (BuildingDefinition) buildingDef);
            }
        }
    }

    private void DoBuildAction()
    {
        if (_BuildingConstructionGhostObject.GetComponent<BuildingConstructionGhost>().CanBuild &&
            Time.time - _LastBuildTime >= 0.1f)
        {
            Instantiate(_SelectedBuilding, _BuildingConstructionGhost.transform.position, _BuildingConstructionGhost.transform.rotation, transform);
            _LastBuildTime = Time.time;
        }
        else
        {
            //Debug.LogError("Can't build. Something's in the way!");
        }
    }

    private void LoadBuildingModels()
    {
        LoadDefenseModels();
    }

    private void LoadDefenseModels()
    {
        string[] defenseBuildings = BuildModeDefinitions.GetBuildingNamesList("Defense");

        foreach (string building in defenseBuildings)
        {
            GameObject prefab = Resources.Load<GameObject>($"Structures/Prefabs/Defense/{building}");
            _BuildingPrefabs.Add($"Defense/{building}", prefab);

            Debug.Log($"Loaded building prefab \"Defense/{building}\".");

            if (prefab == null)
                Debug.LogError($"Failed to load mesh \"Resources/Structures/Meshes/Defense/{building}\"!");
        }

    }

}
