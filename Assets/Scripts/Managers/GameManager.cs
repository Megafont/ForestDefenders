using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Cinemachine;

using TMPro;


public partial class GameManager : MonoBehaviour
{
    [Header("Player")]
    public Transform PlayerSpawnPoint;

    [Header("UI - Elements")]
    public TMP_Text UI_GamePhaseText;

    public TMP_Text UI_TimeToNextWaveText;
    public TMP_Text UI_WaveNumberText;
    public TMP_Text UI_MonstersLeftText;

    public TMP_Text UI_ScoreText;

    public TMP_Text UI_PopulationCountText;
    public TMP_Text UI_FoodCountText;
    public TMP_Text UI_WoodCountText;
    public TMP_Text UI_StoneCountText;

    [Header("UI - Stockpile Text Colors")]
    public Color ResourceStockPilesVeryLowColor = Color.red;
    public Color ResourceStockPilesLowColor = new Color32(255, 128, 0, 255);
    public Color ResourceStockPilesColor = Color.white;

    [Header("UI - Settings")]
    public float GamePhaseTextFadeOutTime = 3.0f;


    public static GameManager Instance;

    public GameObject Player { get; private set; }

    public SceneSwitcher SceneSwitcher { get; private set; }

    public BuildModeManager BuildModeManager { get; private set; }
    public CameraManager CameraManager { get; private set; }
    public InputManager InputManager { get; private set; }
    public MonsterManager MonsterManager { get; private set; }
    public NavMeshManager NavMeshManager { get; private set; }
    public ResourceManager ResourceManager { get; private set; }
    public VillageManager_Buildings VillageManager_Buildings { get; private set; }
    public VillageManager_Villagers VillageManager_Villagers { get; private set; }


    public RadialMenu UI_RadialMenu { get; private set; }


    private float _BuildPhaseLength;

    private float _GameStateStartTime;

    private int _Score;

    private WaitForSeconds _GamePhaseDisplayDelay = new WaitForSeconds(2.5f);
    



    public GameStates GameState { get; private set; } = GameStates.Startup;


    public delegate void GameManager_GameStateChangedHandler(GameStates newGameState);

    public event GameManager_GameStateChangedHandler OnGameStateChanged;



    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            throw new Exception("There is already a GameManager present in the scene!");


        if (PlayerSpawnPoint == null)
            throw new Exception("The player spawn point has not been set in the inspector!");


        SceneSwitcher = FindObjectOfType<SceneSwitcher>();


        SpawnPlayer();

        GetManagerReferences();

        InitUI();

        ChangeGameState(GameStates.PlayerBuildPhase);
    }

    private void Start()
    {
        InitInput();


        _BuildPhaseLength = BuildModeManager.BuildPhaseBaseLength;


        // Register main camera.
        ICinemachineCamera mainCam = GameObject.Find("PlayerFollowCamera").GetComponent<ICinemachineCamera>();
        CameraManager.RegisterCamera((int)CameraIDs.PlayerFollow, mainCam);

        // Make sure the main camera is active.
        CameraManager.SwitchToCamera((int) CameraIDs.PlayerFollow);


        // Fade in the scene.
        SceneSwitcher.FadeIn();
    }

    // Update is called once per frame
    void Update()
    {
        UI_PopulationCountText.text = $"Population: {VillageManager_Villagers.Population} / {VillageManager_Villagers.PopulationCap}";

        UpdateResourceStockpileCounterUI(UI_FoodCountText, "Food: ", ResourceManager.Stockpiles[ResourceTypes.Food]);
        UpdateResourceStockpileCounterUI(UI_WoodCountText, "Wood: ", ResourceManager.Stockpiles[ResourceTypes.Wood]);
        UpdateResourceStockpileCounterUI(UI_StoneCountText, "Stone: ", ResourceManager.Stockpiles[ResourceTypes.Stone]);


        switch (GameState)
        {
            case GameStates.PlayerBuildPhase:
                GameState_PlayerBuildPhase();
                break;

            case GameStates.MonsterAttackPhase:
                GameState_MonsterAttackPhase();
                break;

            case GameStates.GameOver:
                GameState_GameOver();
                break;

        } // end switch GameState
    }

    private void SpawnPlayer()
    {
        GameObject playerPrefab = Resources.Load<GameObject>("Player/Player_Male");
        GameObject playerObj = null;

        float xPos = PlayerSpawnPoint.position.x;
        float zPos = PlayerSpawnPoint.position.z;

        // Detect the ground height at the player spawn point.
        if (Utils_Math.DetectGroundHeightAtPos(xPos,
                                               zPos,
                                               LayerMask.GetMask(new string[] { "Ground" }),
                                               out float groundHeight))
        {
            playerObj = Instantiate(playerPrefab,
                                    new Vector3(xPos, groundHeight, zPos),
                                    Quaternion.identity);
        }
        else
        {
            throw new Exception("Failed to spawn the player, as there is no ground at the player spawn point!");
        }


        Player = playerObj.transform.Find("Player Armature").gameObject;
        Player.GetComponent<Health>().OnDeath += OnPlayerDeath;
    }

    private void UpdateResourceStockpileCounterUI(TMP_Text textUI, string labelText, int currentAmount)
    {
        textUI.text = $"{labelText}{currentAmount}";


        Color startColor = Color.white;
        Color endColor = Color.white;
        float colorBlendAmount = 0f;

        float lowThreshold = ResourceManager.ResourceStockpilesLowThreshold;
        float normalThreshold = ResourceManager.ResourceStockpilesOkThreshold;


        if (currentAmount >= normalThreshold)
        {
            textUI.color = ResourceStockPilesColor;
            return;
        }
        else if (currentAmount < lowThreshold)
        {
            startColor = ResourceStockPilesVeryLowColor;
            endColor = ResourceStockPilesLowColor;
            colorBlendAmount = currentAmount / lowThreshold;
        }
        else if (currentAmount < normalThreshold)
        {
            startColor = ResourceStockPilesLowColor;
            endColor = ResourceStockPilesColor;
            colorBlendAmount = (currentAmount - lowThreshold) / (normalThreshold - lowThreshold);
        }


        textUI.color = Color.Lerp(startColor, endColor, colorBlendAmount);
    }

    private void GetManagerReferences()
    {
        BuildModeManager = GameObject.Find("Build Mode Manager").GetComponent<BuildModeManager>();
        CameraManager = GameObject.Find("Camera Manager").GetComponent<CameraManager>();
        InputManager = GameObject.Find("Input Manager").GetComponent<InputManager>();
        MonsterManager = GameObject.Find("Monster Manager").GetComponent<MonsterManager>();
        NavMeshManager = GameObject.Find("Nav Mesh Manager").GetComponent<NavMeshManager>();
        ResourceManager = GameObject.Find("Resource Manager").GetComponent<ResourceManager>();
        VillageManager_Buildings = GameObject.Find("Village Manager").GetComponent<VillageManager_Buildings>();
        VillageManager_Villagers = GameObject.Find("Village Manager").GetComponent<VillageManager_Villagers>();
    }

    private void InitInput()
    {
        InputManager.RegisterInputActionMap((int) InputActionMapIDs.Player, "Player", true);
        InputManager.RegisterInputActionMap((int) InputActionMapIDs.BuildMode, "Build Mode", true);
        InputManager.RegisterInputActionMap((int) InputActionMapIDs.UI, "UI", true);
    }

    private void InitUI()
    {
        UI_MonstersLeftText.enabled = false;
        UI_TimeToNextWaveText.enabled = false;
        UI_WaveNumberText.enabled = false;

        UI_GamePhaseText.enabled = false;

        UI_ScoreText.text = UI_ScoreText.text = "Score: 0000000000";


        InitRadialMenu();
    }

    public void AddToScore(int amount)
    {
        if (amount <= 0)
            Debug.LogError("The amount to add to the score must be positive!");

        _Score += amount;
        UI_ScoreText.text = $"Score: {_Score:0000000000}";
    }

    public bool CheckIfGameIsOver()
    {
        // NOTE: We don't check if the player is dead here, as we receive an event when that happens via the OnPlayerDeath() method below, which instantly switches us to the GameOver game state;
        if (VillageManager_Buildings.GetTotalBuildingCount() == 0 &&
            VillageManager_Villagers.Population == 0 &&
            Player == null)
        {
            ChangeGameState(GameStates.GameOver);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void InitRadialMenu()
    {
        GameObject obj = GameObject.Find("Radial Menu Canvas");
        if (!obj)
            throw new Exception("The radial menu GameObject was not found!");
        else
        {
            UI_RadialMenu = obj.GetComponent<RadialMenu>();
            if (UI_RadialMenu == null)
                throw new Exception("The radial menu GameObject does not have a RadialMenu component!");
        }
    }

    private void ChangeGameState(GameStates newGameState)
    {
        MonsterManager.ResetComboStreak();

        bool prevGameStateWasStartup = GameState == GameStates.Startup;
        GameState = newGameState;
        
        _GameStateStartTime = Time.time;


        switch (GameState)
        {
            case GameStates.PlayerBuildPhase:
                ResourceManager.RestoreResourceNodes();
                UI_TimeToNextWaveText.enabled = true;
                
                if (!prevGameStateWasStartup)
                    StartCoroutine(ShowGamePhaseText("Monster Attack Defeated!", Color.green));

                break;

            case GameStates.MonsterAttackPhase:
                UI_MonstersLeftText.enabled = true;
                UI_WaveNumberText.enabled = true;
                StartCoroutine(ShowGamePhaseText("Monsters Are Attacking!", Color.red));

                MonsterManager.BeginNextWave();
                break;

            case GameStates.GameOver:
                DisableHUD();
                break;

        } // end switch GameState


        OnGameStateChanged?.Invoke(newGameState);
    }

    private void GameState_PlayerBuildPhase()
    {
        float timeToNextWave = _BuildPhaseLength - (Time.time - _GameStateStartTime);

        if (timeToNextWave <= 0 || 
            (InputManager.Player.EnterBuildMode == false && InputManager.Player.EndBuildPhase)) // Is the player pressing the end build phase button while NOT in build mode?
        {
            timeToNextWave = 0;
            UI_TimeToNextWaveText.enabled = false;

            ChangeGameState(GameStates.MonsterAttackPhase);

            return;
        }

        UpdateWaveTimer(timeToNextWave);
    }

    private void GameState_MonsterAttackPhase()
    {
        CheckIfGameIsOver();


        int monstersLeft = MonsterManager.MonstersLeft;

        UI_MonstersLeftText.text = $"Monsters Left: {monstersLeft} of {MonsterManager.CurrentWaveSize}";
        UI_WaveNumberText.text = $"Wave #{MonsterManager.CurrentWaveNumber} Incoming!";

        if (MonsterManager.WaveComplete)
        {
            UI_MonstersLeftText.enabled = false;
            UI_WaveNumberText.enabled = false;

            // Give player a scoring bonus for clearing the wave.
            AddToScore(MonsterManager.CurrentWaveNumber * 100);

            ChangeGameState(GameStates.PlayerBuildPhase);
        }

    }

    private void GameState_GameOver()
    {
        Debug.LogError("Game Over!");
    }

    private void UpdateWaveTimer(float timeToNextWave)
    {
        int minutes = Mathf.FloorToInt(timeToNextWave / 60f);
        int seconds = Mathf.FloorToInt(timeToNextWave - minutes * 60);

        UI_TimeToNextWaveText.text = $"Next Wave In: {minutes:0}:{seconds:00}";

    }

    private void OnPlayerDeath(GameObject sender)
    {
        ChangeGameState(GameStates.GameOver);
    }

    private void DisableHUD()
    {
        UI_MonstersLeftText.enabled = false;
        UI_ScoreText.enabled = false;
        UI_TimeToNextWaveText.enabled = false;
        UI_WaveNumberText.enabled = false;

        UI_WoodCountText.enabled = false;
        UI_StoneCountText.enabled = false;
    }

    private IEnumerator ShowGamePhaseText(string text, Color32 color)
    {
        UI_GamePhaseText.text = text;
        UI_GamePhaseText.fontMaterial.SetColor("_FaceColor", color);
        UI_GamePhaseText.outlineColor = Color.black;

        UI_GamePhaseText.enabled = true;


        yield return _GamePhaseDisplayDelay;


        float fadeStartTime = Time.time;
        float elapsedTime = 0;

        while(elapsedTime < GamePhaseTextFadeOutTime)
        {
            elapsedTime += Time.deltaTime;

            UI_GamePhaseText.fontMaterial.SetColor("_FaceColor", Color.Lerp(color, Color.clear, elapsedTime));
            UI_GamePhaseText.outlineColor = Color.Lerp(Color.black, Color.clear, elapsedTime);

            yield return null;
        }

        UI_GamePhaseText.enabled = false;
    }
}

