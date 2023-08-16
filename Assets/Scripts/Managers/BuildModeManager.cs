using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using Cinemachine;
using UnityEngine.InputSystem.Editor;

public class BuildModeManager : MonoBehaviour
{
    [Tooltip("The base length of player build phases in the game (in seconds).")]
    public float BuildPhaseBaseLength = 60.0f;

    [Range(0, 1)]
    [Tooltip("The percentage of construction materials the player gets back when they destroy a building.")]
    public float PercentageOfMaterialsRecoveredOnBuildingDestruction = 1.0f;

    public AudioClip BuildingConstructionSound;
    [Range(0,1)]
    public float BuildingConstructionSoundVolume = 1.0f;
    public AudioClip BuildingDestructionSound;
    [Range(0, 1)]
    public float BuildingDestructionSoundVolume = 1.0f;


    private GameManager _GameManager;
    private PlayerController _Player;

    private CameraManager _CameraManager;
    private ICinemachineCamera _BuildModeCam;
    
    private InputManager _InputManager;
    private InputMapManager_BuildMode _InputManager_BuildMode;
    private InputMapManager_Player _InputManager_Player;

    private ResourceManager _ResourceManager;
    private VillageManager_Buildings _VillageManager_Buildings;

    private BuildingConstructionGhost _BuildingConstructionGhost;

    private float _LastBuildTime;
    private float _LastExitTime;

    private string _SelectedBuildingName;
    private string _SelectedBuildingCategory;
    private GameObject _SelectedBuildingPrefab;

    private RadialMenuDialog _RadialMenu;
    private string _TempCategory; // Tracks the selected category while the second building selection menu is open.

    private List<MaterialCost> _TotalBuildCostsInCurrentBuildModeSession; // Tracks the total resources the player spent constructing buildings in the current build mode session.
    private int _TotalBuildingsCreatedInCurrentBuildModeSession; // The number of buildings the player constructed in the current build mode session.

    private Dictionary<string, Sprite> _CategoryThumbnails;



    private void Awake()
    {
        _TotalBuildCostsInCurrentBuildModeSession = new List<MaterialCost>();
        foreach (ResourceTypes resource in Enum.GetValues(typeof(ResourceTypes)))
        {
            MaterialCost mc = new MaterialCost();

            mc.Resource = resource;
            mc.Amount = 0;

            _TotalBuildCostsInCurrentBuildModeSession.Add(mc);
        }


        BuildModeDefinitions.InitBuildingDefinitionLookupTables();
    }

    // Start is called before the first frame update
    void Start()
    {
        _CategoryThumbnails = new Dictionary<string, Sprite>();
        CacheCategoryThumbnailReferences();


        // This is using the ?. operator, because the player reference will be null if we are not in a gameplay scene (like the main menu for example).
        _Player = GameManager.Instance.Player?.GetComponentInChildren<PlayerController>();

        _GameManager = GameManager.Instance;
        _CameraManager = _GameManager.CameraManager;

        _InputManager = _GameManager.InputManager;
        _InputManager_BuildMode = (InputMapManager_BuildMode)_InputManager.GetInputMapManager((uint)InputActionMapIDs.BuildMode);
        _InputManager_Player = (InputMapManager_Player)_InputManager.GetInputMapManager((uint)InputActionMapIDs.Player);


        _ResourceManager = _GameManager.ResourceManager;
        _VillageManager_Buildings = _GameManager.VillageManager_Buildings;

        _RadialMenu = _GameManager.RadialMenuDialog;

        InitBuildingGhost();


        _BuildModeCam = GameObject.Find("CM Build Mode Camera").GetComponent<ICinemachineCamera>();
        _BuildModeCam.Follow = _BuildingConstructionGhost.transform;
        _BuildModeCam.LookAt = _BuildingConstructionGhost.transform;
        _CameraManager.RegisterCamera((int) CameraIDs.BuildMode,
                                      _BuildModeCam);

        _CameraManager.OnCameraTransitionStarted += OnCameraTransitionStarted;
        _CameraManager.OnCameraTransitionEnded += OnCameraTransitionEnded;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the player can enter/exit build mode.
        // 
        if (!_GameManager.PlayerIsDead &&
            _GameManager.GameState == GameStates.PlayerBuildPhase &&
            !_CameraManager.IsTransitioning &&
            !_GameManager.GamePhaseTextIsVisible)
        {
            // Check if player is entering build mode.
            bool input = _InputManager_Player.EnterBuildMode;

            if (input && !IsBuildModeActive)
            {
                StartCoroutine(EnableBuildMode(input));
            }
            else if (!input && IsBuildModeActive && !_CameraManager.IsTransitioning)
            {
                DoBuildModeChecks();
            }

        }

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
        bool input = _InputManager_BuildMode.ExitBuildMode;
        if (input && Time.time - _LastExitTime > 0.25f)
        {
            _LastExitTime = Time.time;

            StartCoroutine(EnableBuildMode(false));

            return;
        }


        // If the player pressed the select building button, and we are not already in the process of selecting a building, then open the buildings menu.
        if (_InputManager_BuildMode.SelectBuilding && !IsSelectingBuilding && !Dialog_Base.AreAnyDialogsOpen())
        {
            StartCoroutine(DisplaySelectBuildingMenu());
            return;
        }


        // Did the player press the build button?
        if (_InputManager_BuildMode.ConstructBuilding && !IsSelectingBuilding)
            DoBuildAction();

    }

    private IEnumerator EnableBuildMode(bool state)
    {
        if (IsSelectingBuilding || state == IsBuildModeActive)
            yield break;


        // NOTE: We do not set the camera height in this function. This is controlled by the follow
        //       offset, which is an inspector property on the "CM Build Mode Camera" game object:
        //       CinemachineVirtualCamera->Body->Follow Offset


        // Wait until the button press that entered/exited build mode has ended.
        // Otherwise, we'll have problems since the two inputs use the same button where
        // as soon as we exit build mode, the Update() method will think we should enter
        // build mode again, because it sees that the button is pressed.
        while (_InputManager_Player.EnterBuildMode || _InputManager_BuildMode.ExitBuildMode)
        {
            yield return null;
        }



        if (state)
        {
            _CameraManager.SwitchToCamera((int) CameraIDs.BuildMode);
            _InputManager.SwitchToActionMap((int) InputActionMapIDs.BuildMode);
        }
        else
        {
            _CameraManager.SwitchToCamera((int) CameraIDs.PlayerFollow);
            _InputManager.SwitchToActionMap((int) InputActionMapIDs.Player);


            if (_TotalBuildingsCreatedInCurrentBuildModeSession > 0)
            {
                StartCoroutine(TextPopup.ShowTextPopupDelayed(2f,
                                                              TextPopup.AdjustStartPosition(_Player.gameObject),
                                                              MaterialCostListToString(_TotalBuildCostsInCurrentBuildModeSession, "Used ", $" on {_TotalBuildingsCreatedInCurrentBuildModeSession} building(s)"),
                                                              TextPopupColors.ExpendedResourceColor,
                                                              _Player.gameObject,
                                                              12f,
                                                              maxMoveSpeed: 1f));
            }


            _TotalBuildingsCreatedInCurrentBuildModeSession = 0;

            ResetTotalMaterialCosts();
        }


        IsBuildModeActive = state;
    }

    private void CacheCategoryThumbnailReferences()
    {
        _CategoryThumbnails.Clear();

        _CategoryThumbnails.Add("Bridges", BuildModeDefinitions.GetBuildingDefinition("Bridges", "Wood Bridge (10m)").Thumbnail);
        _CategoryThumbnails.Add("Defense", BuildModeDefinitions.GetBuildingDefinition("Defense", "Turret").Thumbnail);
        _CategoryThumbnails.Add("Farming", BuildModeDefinitions.GetBuildingDefinition("Farming", "Small Garden").Thumbnail);
        _CategoryThumbnails.Add("Housing", BuildModeDefinitions.GetBuildingDefinition("Housing", "Small House").Thumbnail);
        _CategoryThumbnails.Add("Walls", BuildModeDefinitions.GetBuildingDefinition("Walls", "Wood Wall").Thumbnail);
    }

    private Sprite[] GetCategoryThumbnails(string[] categories)
    {
        List<Sprite> thumbnails = new List<Sprite>();


        foreach (string category in categories)
        {
            thumbnails.Add(_CategoryThumbnails[category]);
        }


        return thumbnails.ToArray();
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
            string[] categories = BuildModeDefinitions.GetList_BuildingCategoriesContainingResearchedBuildings();
            _RadialMenu.SetMenuParams("Select Building Type",
                                      categories,
                                      GetCategoryThumbnails(categories));
            _RadialMenu.OpenDialog();



            if (!_RadialMenu.IsOpen())
            {
                IsSelectingBuilding = false;
                yield break;
            }


            while (!_RadialMenu.MenuConfirmed && !_RadialMenu.MenuCancelled)
                yield return null;


            _TempCategory = _RadialMenu.SelectedMenuItemName;


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
            _RadialMenu.SetMenuParams($"Select {_TempCategory} Building", 
                                      BuildModeDefinitions.GetList_NamesOfResearchedbuildingsInCategory(_TempCategory),
                                      BuildModeDefinitions.GetList_ThumbnailsOfResearchedbuildingsInCategory(_TempCategory));
            _RadialMenu.OpenDialog();

            if (!_RadialMenu.IsOpen())
            {
                IsSelectingBuilding = false;
                yield break;
            }


            while (!_RadialMenu.MenuConfirmed && !_RadialMenu.MenuCancelled)
                yield return null;


            building = _RadialMenu.SelectedMenuItemName;

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
        if (_RadialMenu.SelectedMenuItemName == "Cancel" || _RadialMenu.SelectedMenuItemName == "Unused")
        {
            _RadialMenu.BottomBarText = "";
            return;
        }

        UpdateBuildingCostDisplay();
    }

    private void UpdateBuildingCostDisplay()
    {
        _RadialMenu.BottomBarText = GetBuildingCostString();
    }

    private string GetBuildingCostString()
    {
        StringBuilder b = new StringBuilder("Construction Cost:  ");

        List<MaterialCost> constructionCosts = BuildModeDefinitions.GetList_BuildingConstructionCosts(_TempCategory, _RadialMenu.SelectedMenuItemName);

        for (int i = 0; i < constructionCosts.Count; i++)
        {
            b.Append(" ");
            b.Append($"{constructionCosts[i].Amount} {constructionCosts[i].Resource}");

            if (i < constructionCosts.Count - 1)
                b.Append(",  ");
        }

        return b.ToString();
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

            _SelectedBuildingPrefab = buildingDef.Prefab;
            
            
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
            _LastBuildTime = Time.time;


            if (!_GameManager.ConstructionIsFree)
                ApplyBuildCosts();


            GameObject buildingObj = _VillageManager_Buildings.SpawnBuilding(_SelectedBuildingPrefab,
                                                                             _SelectedBuildingCategory,
                                                                             _SelectedBuildingName,
                                                                             _BuildingConstructionGhost.BuildPosition,
                                                                             _BuildingConstructionGhost.transform.rotation);

            // If the building is a bridge, we need to add it to the list in the parent bridge construction zone,
            // to keep track of whether the zone is bridged or not.
            IBuilding building = buildingObj.GetComponent<IBuilding>();
            if (BuildModeDefinitions.BuildingIsBridge(building.GetBuildingDefinition()))
                _BuildingConstructionGhost.ParentBridgeConstructionZone.AddBridge(building);


            building.AudioSource.clip = BuildingConstructionSound;
            building.AudioSource.volume = BuildingConstructionSoundVolume;
            building.AudioSource.Play();

            _TotalBuildingsCreatedInCurrentBuildModeSession++;
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
            if (_ResourceManager.GetStockpileLevel(cost.Resource) < cost.Amount)
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
      

        for (int i = 0; i < def.ConstructionCosts.Count; i++)
        {            
            MaterialCost cost = def.ConstructionCosts[i];

            // Add this cost to the total build costs of this build mode session.
            AddToTotalMaterialCosts(cost);

            if (!_ResourceManager.TryToExpendFromStockpile(cost.Resource, cost.Amount))
            {
                // NOTE: This code should NEVER run, as the game already checked if the player has enough resources before this
                //       function was ever called, because the BuildingConstructionGhost's CanBuild property includes a resources check.
                Debug.LogWarning($"Could not expend {cost.Amount} {cost.Resource}! The stockpile somehow did not have enough!");
            }

        } // end for i

    }

    /// <summary>
    /// Called to give back materials to the player when he destroys a building.
    /// </summary>
    public void RestoreBuildingMaterials(string buildingCategory, string buildingName)
    {
        BuildingDefinition def = BuildModeDefinitions.GetBuildingDefinition(buildingCategory, buildingName);


        for (int i = 0; i < def.ConstructionCosts.Count; i++)
        {
            MaterialCost cost = def.ConstructionCosts[i];

            _ResourceManager.AddToStockpile(cost.Resource, cost.Amount * PercentageOfMaterialsRecoveredOnBuildingDestruction);

        } // end for i


        TextPopup.ShowTextPopup(TextPopup.AdjustStartPosition(_Player.gameObject), 
                                MaterialCostListToString(def.ConstructionCosts, "Recovered ", ""), 
                                TextPopupColors.RecoveredResourceColor,
                                _Player.gameObject,
                                12f,
                                maxMoveSpeed: 1f);
    }

    private string MaterialCostListToString(List<MaterialCost> costList, string prefix, string suffix)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(prefix);


        // Filter out resource types that have a total cost of 0.
        List<MaterialCost> filteredList = new List<MaterialCost>();
        foreach (MaterialCost cost in costList)
        {
            if (cost.Amount > 0)
                filteredList.Add(cost);
        }


        // Construct the string.
        for (int i = 0; i < filteredList.Count; i++)
        {
            MaterialCost cost = filteredList[i];


            if (filteredList.Count > 1 && i == filteredList.Count - 1)
                builder.Append("and ");

            builder.Append($"{cost.Amount} {cost.Resource}");

            if (i < filteredList.Count - 1)
                builder.Append(", ");

        } // end for i

        
        builder.Append(suffix);

        return builder.ToString();
    }

    private void ResetTotalMaterialCosts()
    {
        for (int i = 0; i < _TotalBuildCostsInCurrentBuildModeSession.Count; i++)
        {
            MaterialCost mc = _TotalBuildCostsInCurrentBuildModeSession[i];
            
            mc.Amount = 0;
            _TotalBuildCostsInCurrentBuildModeSession[i] = mc;
        }
    }

    /// <summary>
    /// This function is used for tracking the total resource costs the player has amassed during the
    /// current build mode session.
    /// </summary>
    /// <param name="cost">The material cost to add to the running total for the corresponding resource type.</param>
    private void AddToTotalMaterialCosts(MaterialCost cost)
    {
        MaterialCost totalCost = _TotalBuildCostsInCurrentBuildModeSession[(int) cost.Resource];
        
        totalCost.Amount += cost.Amount;
        _TotalBuildCostsInCurrentBuildModeSession[(int) cost.Resource] = totalCost;
    }

    private void OnCameraTransitionStarted(ICinemachineCamera startCam, ICinemachineCamera endCam)
    {
        bool showConstructionGhost = (endCam.VirtualCameraGameObject.name == "CM Build Mode Camera");


        if (showConstructionGhost)
        {
            _BuildingConstructionGhost.gameObject.SetActive(true);
            _BuildingConstructionGhost.ResetTransform();
        }

        _BuildingConstructionGhost.gameObject.SetActive(showConstructionGhost);

    }

    private void OnCameraTransitionEnded(ICinemachineCamera startCam, ICinemachineCamera endCam)
    {
        // Show the build mode construction ghost only if we are transitioning to the build mode cam.
        bool showConstructionGhost = (endCam.VirtualCameraGameObject.name == "CM Build Mode Camera");
        
        _BuildingConstructionGhost.gameObject.SetActive(showConstructionGhost);
        
    }



    public bool IsBuildModeActive { get; private set; }
    public bool IsSelectingBuilding { get; private set; }

}
