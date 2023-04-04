using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using Cinemachine;


public class BuildModeManager : MonoBehaviour
{
    [Tooltip("The base length of player build phases in the game (in seconds).")]
    public float BuildPhaseBaseLength = 60.0f;

    [Range(0, 1)]
    [Tooltip("The percentage of construction materials the player gets back when they destroy a building.")]
    public float PercentageOfMaterialsRecoveredOnBuildingDestruction = 1.0f;


    public bool IsBuildModeActive { get; private set; }
    public bool IsSelectingBuilding { get; private set; }


    private GameManager _GameManager;
    private PlayerController _Player;

    private CameraManager _CameraManager;
    private InputManager _InputManager;
    private ResourceManager _ResourceManager;
    private VillageManager_Buildings _VillageManager_Buildings;

    private BuildingConstructionGhost _BuildingConstructionGhost;

    private float _LastBuildTime;

    private string _SelectedBuildingName;
    private string _SelectedBuildingCategory;
    private GameObject _SelectedBuildingPrefab;

    private RadialMenuDialog _RadialMenu;
    private string _TempCategory; // Tracks the selected category while the second building selection menu is open.



    private void Awake()
    {
        BuildModeDefinitions.InitBuildingDefinitionLookupTables();
    }

    // Start is called before the first frame update
    void Start()
    {
        // This is using the ?. operator, because the player reference will be null if we are not in a gameplay scene (like the main menu for example).
        _Player = GameManager.Instance.Player?.GetComponentInChildren<PlayerController>();

        _GameManager = GameManager.Instance;
        _CameraManager = _GameManager.CameraManager;
        _InputManager = _GameManager.InputManager;
        _ResourceManager = _GameManager.ResourceManager;
        _VillageManager_Buildings = _GameManager.VillageManager_Buildings;

        _RadialMenu = _GameManager.RadialMenuDialog;

        InitBuildingGhost();


        ICinemachineCamera buildCam = GameObject.Find("CM Build Mode Camera").GetComponent<ICinemachineCamera>();
        buildCam.Follow = _BuildingConstructionGhost.transform;
        buildCam.LookAt = _BuildingConstructionGhost.transform;
        _CameraManager.RegisterCamera((int) CameraIDs.BuildMode,
                                      buildCam);       
    }

    // Update is called once per frame
    void Update()
    {
        // Check if player is entering build mode.
        bool input = _InputManager.Player.EnterBuildMode;
        if (input && _GameManager.GameState == GameStates.PlayerBuildPhase && !_GameManager.GamePhaseTextIsVisible && !IsBuildModeActive && !Dialog_Base.AreAnyDialogsOpen())
            StartCoroutine(EnableBuildMode(input));
            
        
        // Do build mode checks only when build mode is on.
        if (IsBuildModeActive && !input)
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

        _BuildingConstructionGhost.Init();

        _BuildingConstructionGhost.gameObject.SetActive(false);
    }



    private void DoBuildModeChecks()
    {

        // Check if the player is exiting buildmode.
        bool input = _InputManager.BuildMode.ExitBuildMode;
        if (input)
        {
            StartCoroutine(EnableBuildMode(false));
            return;
        }


        // If the player pressed the select building button, and we are not already in the process of selecting a building, then open the buildings menu.
        if (_InputManager.BuildMode.SelectBuilding && !IsSelectingBuilding && !Dialog_Base.AreAnyDialogsOpen())
        {
            StartCoroutine(DisplaySelectBuildingMenu());
            return;
        }


        // Did the player press the build button?
        if (_InputManager.BuildMode.Build && !IsSelectingBuilding)
            DoBuildAction();

    }

    private IEnumerator EnableBuildMode(bool state)
    {
        if (IsSelectingBuilding)
            yield break;


        // NOTE: We do not set the camera height in this function. This is controlled by the follow
        //       offset, which is an inspector property on the "CM Build Mode Camera" game object:
        //       CinemachineVirtualCamera->Body->Follow Offset


        // Wait until the button press that entered/exited build mode has ended.
        // Otherwise, we'll have problems since the two inputs use the same button where
        // as soon as we exit build mode, the Update() method will think we should enter
        // build mode again, because it sees that the button is pressed.
        while (_InputManager.Player.EnterBuildMode || _InputManager.BuildMode.ExitBuildMode)
        {
            yield return null;
        }


        _BuildingConstructionGhost.gameObject.SetActive(state);


        if (state)
        {
            _CameraManager.SwitchToCamera((int) CameraIDs.BuildMode);
            _InputManager.SwitchToActionMap((int) InputActionMapIDs.BuildMode);

            _BuildingConstructionGhost.ResetTransform();
        }
        else
        { 
            _CameraManager.SwitchToCamera((int) CameraIDs.PlayerFollow);
            _InputManager.SwitchToActionMap((int) InputActionMapIDs.Player);
        }


        IsBuildModeActive = state;
    }

    private IEnumerator DisplaySelectBuildingMenu()
    {

        IsSelectingBuilding = true;

        string building = null;


        while (true)
        {
            // BUILDING CATEGORIES MENU
            // ----------------------------------------------------------------------------------------------------

            _RadialMenu.BottomBarText = "";
            _RadialMenu.SetMenuParams("Select Building Type", BuildModeDefinitions.GetList_BuildingCategoriesContainingResearchedBuildings());
            _RadialMenu.OpenDialog();



            if (!_RadialMenu.IsOpen())
            {
                IsSelectingBuilding = false;
                yield break;
            }


            while (!_RadialMenu.MenuConfirmed && !_RadialMenu.MenuCancelled)
                yield return null;


            _TempCategory = _RadialMenu.SelectedItemName;


            if (_RadialMenu.MenuCancelled)
            {
                IsSelectingBuilding = false;
                yield break; // The player cancelled out of the menu, so break out of this coroutine.
            }



            // Wait until the menu has closed before trying to open a new one.
            while (_RadialMenu.IsOpen())
                yield return null;



            // BUILDINGS IN CHOSEN CATEGORY MENU
            // ----------------------------------------------------------------------------------------------------

            _RadialMenu.OnSelectionChanged += OnRadialMenuSelectionChangedHandler;
            _RadialMenu.SetMenuParams($"Select {_TempCategory} Building", BuildModeDefinitions.GetList_NamesOfResearchedbuildingsInCategory(_TempCategory));
            _RadialMenu.OpenDialog();


            if (!_RadialMenu.IsOpen())
            {
                IsSelectingBuilding = false;
                yield break;
            }


            OnRadialMenuSelectionChangedHandler(null);

            while (!_RadialMenu.MenuConfirmed && !_RadialMenu.MenuCancelled)
                yield return null;


            building = _RadialMenu.SelectedItemName;

            _RadialMenu.OnSelectionChanged -= OnRadialMenuSelectionChangedHandler;


            if (_RadialMenu.MenuConfirmed)
            {
                break;
            }
            else // The menu was cancelled.
            {
                // Wait until the menu has closed before trying to open the first one again.
                while (_RadialMenu.IsOpen())
                    yield return null;

                continue;
            }

        } // end while



        // FINISH UP
        // ----------------------------------------------------------------------------------------------------

        SelectBuilding(_TempCategory, building);


        // Wait until the menu has closed before returning. This prevents the player character from instantly attacking
        // because the input for that button hasn't had time to turn back off again after the button press.
        while (_RadialMenu.IsOpen())
            yield return null;


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

        List<MaterialCost> constructionCosts = BuildModeDefinitions.GetList_BuildingConstructionCosts(_TempCategory, _RadialMenu.SelectedItemName);

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
                //Mesh mesh = buildingDef.Prefab.GetComponent<IBuilding>().GetMesh();
                _BuildingConstructionGhost.ChangeMesh(buildingDef.ConstructionGhostMesh, buildingDef);
            }
        } // end if

    }

    private void DoBuildAction()
    {
        if (_BuildingConstructionGhost.CanBuild &&
            Time.time - _LastBuildTime >= 0.1f)
        {
            if (!_GameManager.ConstructionIsFree)
                ApplyBuildCosts();


            GameObject buildingObj = _VillageManager_Buildings.SpawnBuilding(_SelectedBuildingPrefab,
                                                                             _SelectedBuildingCategory,
                                                                             _SelectedBuildingName,
                                                                             _BuildingConstructionGhost.BuildPosition,
                                                                             _BuildingConstructionGhost.transform.rotation);

            // If the bridge is a building, we need to add it to the list in the parent bridge construction zone,
            // to keep track of whether the zone is bridged or not.
            IBuilding building = buildingObj.GetComponent<IBuilding>();
            if (BuildModeDefinitions.BuildingIsBridge(building.GetBuildingDefinition()))
                _BuildingConstructionGhost.ParentBridgeConstructionZone.AddBridge(building);


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
            _ResourceManager.Stockpiles[cost.Resource] += (int) (cost.Amount * PercentageOfMaterialsRecoveredOnBuildingDestruction);
        }

    }

}
