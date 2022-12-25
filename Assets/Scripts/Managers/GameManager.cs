using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Cinemachine;
using TMPro;


public class GameManager : MonoBehaviour
{
    [Header("Player")]
    public bool EnablePlayerSpawn = true;
    public bool PlayerCanDamageVillagers = true;
    public Transform PlayerSpawnPoint;
    public float FallOutOfWorldDeathHeight = -32;
    public float PlayerStartingAttackPower = 20f;
    public float PlayerStartingMaxHealth = 50f;
    public float PlayerStartingGatherRate = 3f;

    [Header("Village")]
    public float PlayerHealFoodCostMultiplier = 2f;
    public int StartingXP = 0;
    public int XP_EarnedPerWave = 2;

    [Header("Village - Level Up Settings")]
    public float PlayerAttackBuffAmountPerLevelUp = 2f;
    public float PlayerGatheringBuffAmountPerLevelUp = 1f;
    public float PlayerHealthBuffAmountPerLevelUp = 5f;
    public float VillagersAttackBuffAmountPerLevelUp = 2f;
    public float VillagersGatheringBuffAmountPerLevelUp = 1f;
    public float VillagersHealthBuffAmountPerLevelUp = 5f;
    public float VillagersStartingAttackPower = 5f;
    public float VillagersStartingMaxHealth = 25f;
    public float VillagersStartingGatherRate = 3f;
    public float MaxAttackPowerCap = 100f;
    public float MaxHealthCap = 200f;
    [Tooltip("The maximum amount of resource that can be collected in a single punch.")]
    public float MaxGatheringCap = 20f;

    [Header("Scoring")]
    public int PlayerGatheringScoreMultiplier = 2;
    public int PlayerResearchScoreMultiplier = 100;


    [Header("Sound & Music")]
    public MusicParams MusicParams;
    public SoundParams SoundParams;

    [Header("UI Elements")]
    public float GamePhaseTextFadeOutTime = 3.0f;

    [Header("UI Elements (Bottom Bar)")]
    public Color ResourceStockPilesVeryLowColor = Color.red;
    public Color ResourceStockPilesLowColor = new Color32(255, 128, 0, 255);
    public Color ResourceStockPilesColor = Color.white;



    private TMP_Text _UI_GamePhaseText;
    private HighScoreNameEntryDialog _UI_HighScoreNameEntryDialog;
    private LevelUpDialog _UI_LevelUpDialog;
    private RadialMenuDialog _UI_RadialMenuDialog;
    private TechTreeDialog _UI_TechTreeDialog;

    private Image _UI_TopBar;
    private TMP_Text _UI_TimeToNextWaveText;
    private TMP_Text _UI_WaveNumberText;
    private TMP_Text _UI_MonstersLeftText;
    private TMP_Text _UI_VillagersLostCountText;
    private TMP_Text _UI_MonstersKilledCountText;
    private TMP_Text _UI_SurvivalTimeText;
    private TMP_Text _UI_ScoreText;

    private PlayerHealthBar _UI_PlayerHealthBar;
    private TMP_Text _UI_KillComboCountText;

    private Image _UI_BottomBar;
    private TMP_Text _UI_VillagerCountText;
    private TMP_Text _UI_BuildingCountText;
    private TMP_Text _UI_XPCountText;
    private TMP_Text _UI_FoodCountText;
    private TMP_Text _UI_WoodCountText;
    private TMP_Text _UI_StoneCountText;




    public static GameManager Instance;


    private float _BuildPhaseLength;

    private float _GameStateStartTime;

    
    private WaitForSeconds _GamePhaseDisplayDelay = new WaitForSeconds(2.5f);



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


        if (EnablePlayerSpawn)
            SpawnPlayer();

        GetManagerReferences();

        InitUI();


        if (StartingXP > 0)
            _UI_TechTreeDialog.AddXP(StartingXP);


        if (MusicPlayer.Instance)
            MusicPlayer = MusicPlayer.Instance;
        else
        {
            MusicPlayer prefab = Resources.Load<GameObject>("Music Player").GetComponent<MusicPlayer>();
            MusicPlayer = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }
    }

    private void Start()
    {
        InitInput();


        _BuildPhaseLength = BuildModeManager.BuildPhaseBaseLength;


        SceneSwitcher = SceneSwitcher.Instance;

        if (PlayerIsInGame())
            ChangeGameState(GameStates.PlayerBuildPhase);
        else
            ChangeGameState(GameStates.Menu);


        InitCameras();



        // Fade in the scene.        
        SceneSwitcher.FadeIn();
    }

    bool _PrintedWarning = false;
    // Update is called once per frame
    void Update()
    {
        if (GameState != GameStates.GameOver)
            SurvivalTime += Time.deltaTime;

        UpdateCommonHUDStats();


        switch (GameState)
        {
            case GameStates.Menu:
                GameState_Menu();
                break;

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


        if (PlayerIsInGame()) // Are we in a level?
        {
            if (GameState != GameStates.GameOver)
                CheckIfGameIsOver();
        }
        else
        {
            if (!_PrintedWarning)
            {
                Debug.LogWarning($"GameManager.CheckIfGameIsOver() is disabled in scene (\"{SceneSwitcher.ActiveSceneName}\"). Make sure this is intentional.");
                _PrintedWarning = true;
            }
        }

    }

    /// <summary>
    /// Updates all HUD stats that are not specific to a given GameState.
    /// </summary>
    private void UpdateCommonHUDStats()
    {
        // Update the top bar stats.
        _UI_VillagersLostCountText.text = $"Villagers Lost: {VillageManager_Villagers.TotalVillagersLost:n0}";
        _UI_MonstersKilledCountText.text = $"Monsters Killed: {MonsterManager.TotalMonstersKilled:n0}";
        _UI_SurvivalTimeText.text = $"Survival Time: {HighScores.TimeValueToString(SurvivalTime):n0}";
        _UI_ScoreText.text = $"Score: {Score:n0}";


        float comboCount = MonsterManager.CurrentComboStreak;
        if (comboCount > 0)
            _UI_KillComboCountText.text = $"Kill Combo: {comboCount}";
        else
            _UI_KillComboCountText.text = null;


        // Update the bottom bar stats.
        _UI_VillagerCountText.text = $"Population: {VillageManager_Villagers.Population:n0} / {VillageManager_Villagers.PopulationCap:n0}";
        _UI_BuildingCountText.text = $"Buildings: {VillageManager_Buildings.TotalBuildingCount:n0}";
        _UI_XPCountText.text = $"XP: {_UI_TechTreeDialog.AvailableXP:n0}";

        UpdateResourceStockpileCounterUI(_UI_FoodCountText, "Food: ", ResourceManager.Stockpiles[ResourceTypes.Food]);
        UpdateResourceStockpileCounterUI(_UI_WoodCountText, "Wood: ", ResourceManager.Stockpiles[ResourceTypes.Wood]);
        UpdateResourceStockpileCounterUI(_UI_StoneCountText, "Stone: ", ResourceManager.Stockpiles[ResourceTypes.Stone]);

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

    private void UpdateResourceStockpileCounterUI(TMP_Text textUI, string labelText, float currentAmount)
    {
        textUI.text = $"{labelText}{currentAmount:n0}";


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

    private void InitCameras()
    {
        switch (SceneSwitcher.ActiveSceneName)
        {
            case "Main Menu":
                InitCameras_MenuScene();
                break;

            case "Test":
                InitCameras_TestScene();
                break;


            default:
                throw new Exception($"No camera initialization function has been implemented for this scene (\"{SceneSwitcher.ActiveSceneName}\")!");
        } // end switch

    }

    private void InitCameras_MenuScene()
    {
        // Register game over camera.
        ICinemachineCamera gameOverCam = GameObject.Find("CM Game Over Cam").GetComponent<ICinemachineCamera>();
        gameOverCam.Follow = VillageManager_Buildings.TownCenter.transform;
        gameOverCam.LookAt = VillageManager_Buildings.TownCenter.transform;
        CameraManager.RegisterCamera((int)CameraIDs.GameOver, gameOverCam);


        // Switch to the main camera for this scene.
        CameraManager.SwitchToCamera((int)CameraIDs.GameOver);
    }

    private void InitCameras_TestScene()
    {
        // Register player follow camera.
        ICinemachineCamera mainCam = GameObject.Find("CM Player Follow Camera").GetComponent<ICinemachineCamera>();
        CameraManager.RegisterCamera((int)CameraIDs.PlayerFollow, mainCam);

        // Register game over camera.
        ICinemachineCamera gameOverCam = GameObject.Find("CM Game Over Cam").GetComponent<ICinemachineCamera>();
        gameOverCam.Follow = Player.transform;
        gameOverCam.LookAt = Player.transform;
        CameraManager.RegisterCamera((int)CameraIDs.GameOver, gameOverCam);


        // Switch to the main camera for this scene.
        CameraManager.SwitchToCamera((int)CameraIDs.PlayerFollow);
    }

    private void InitInput()
    {
        InputManager.RegisterInputActionMap((int)InputActionMapIDs.Player, "Player", true);
        InputManager.RegisterInputActionMap((int)InputActionMapIDs.BuildMode, "Build Mode", true);
        InputManager.RegisterInputActionMap((int)InputActionMapIDs.UI, "UI", true);
    }

    private void InitUI()
    {
        GetUIReferences();
       
        _UI_MonstersLeftText.enabled = false;
        _UI_WaveNumberText.enabled = false;

        _UI_GamePhaseText.enabled = false;
    }

    private void GetUIReferences()
    {
        _UI_GamePhaseText = GameObject.Find("UI/HUD/Game Phase Text Canvas/Game Phase Text (TMP)").GetComponent<TMP_Text>();
        _UI_HighScoreNameEntryDialog = GameObject.Find("UI/High Score Name Entry Dialog").GetComponent<HighScoreNameEntryDialog>();
        _UI_LevelUpDialog = GameObject.Find("UI/Level Up Dialog").GetComponent<LevelUpDialog>();
        _UI_RadialMenuDialog = GameObject.Find("UI/Radial Menu Dialog").GetComponent<RadialMenuDialog>();
        _UI_TechTreeDialog = GameObject.Find("UI/Tech Tree Dialog").GetComponent<TechTreeDialog>();

        _UI_TopBar = GameObject.Find("UI/HUD/Top Bar").GetComponent<Image>();
        _UI_TimeToNextWaveText = GameObject.Find("UI/HUD/Top Bar/Time To Next Wave Text (TMP)").GetComponent<TMP_Text>();
        _UI_WaveNumberText = GameObject.Find("UI/HUD/Top Bar/Wave Number Text (TMP)").GetComponent<TMP_Text>();
        _UI_MonstersLeftText = GameObject.Find("UI/HUD/Top Bar/Monsters Left Text (TMP)").GetComponent<TMP_Text>();
        _UI_VillagersLostCountText = GameObject.Find("UI/HUD/Top Bar/Villagers Lost Count Text (TMP)").GetComponent<TMP_Text>();
        _UI_MonstersKilledCountText = GameObject.Find("UI/HUD/Top Bar/Monsters Killed Count Text (TMP)").GetComponent<TMP_Text>();
        _UI_SurvivalTimeText = GameObject.Find("UI/HUD/Top Bar/Survival Time Text (TMP)").GetComponent<TMP_Text>();
        _UI_ScoreText = GameObject.Find("UI/HUD/Top Bar/Score Text (TMP)").GetComponent<TMP_Text>();

        _UI_PlayerHealthBar = GameObject.Find("Player Health Bar").GetComponent<PlayerHealthBar>();
        _UI_KillComboCountText = GameObject.Find("UI/HUD/Kill Combo Count Text (TMP)").GetComponent<TMP_Text>();

        _UI_BottomBar = GameObject.Find("UI/HUD/Bottom Bar").GetComponent<Image>();
        _UI_VillagerCountText = GameObject.Find("UI/HUD/Bottom Bar/Villager Count Text (TMP)").GetComponent<TMP_Text>();
        _UI_BuildingCountText = GameObject.Find("UI/HUD/Bottom Bar/Building Count Text (TMP)").GetComponent<TMP_Text>();
        _UI_XPCountText = GameObject.Find("UI/HUD/Bottom Bar/XP Count Text (TMP)").GetComponent<TMP_Text>();
        _UI_FoodCountText = GameObject.Find("UI/HUD/Bottom Bar/Food Count Text (TMP)").GetComponent<TMP_Text>();
        _UI_WoodCountText = GameObject.Find("UI/HUD/Bottom Bar/Wood Count Text (TMP)").GetComponent<TMP_Text>();
        _UI_StoneCountText = GameObject.Find("UI/HUD/Bottom Bar/Stone Count Text (TMP)").GetComponent<TMP_Text>();
    }

    public void AddToScore(int amount)
    {
        // If the player is dead, simply return. This ignores any additional points being earned by villagers killing monsters.
        if (Player == null || (Player != null && Player.GetComponent<Health>().CurrentHealth == 0))
            return;


        if (amount <= 0)
            Debug.LogError("The amount to add to the score must be positive!");

        Score += amount;
    }

    public bool CheckIfGameIsOver()
    {
        // NOTE: We don't check if the player is dead here, as we receive an event when that happens via the OnPlayerDeath() method below, which instantly switches us to the GameOver game state;
        if (VillageManager_Buildings.GetTotalBuildingCount() == 0 &&
            VillageManager_Villagers.Population == 0 &&
            Player == null ||
            Player.transform.position.y <= FallOutOfWorldDeathHeight)
        {

            ICinemachineCamera gameOvercam = CameraManager.GetCameraWithID((int) CameraIDs.GameOver);
            if (Player.transform.position.y <= FallOutOfWorldDeathHeight)
            {
                Transform townCenterTransform = VillageManager_Buildings.TownCenter.transform;
                gameOvercam.LookAt = townCenterTransform;
                gameOvercam.Follow = townCenterTransform;
            }
            else
            {
                gameOvercam.LookAt = Player.transform;
                gameOvercam.Follow = Player.transform;
            }


            ChangeGameState(GameStates.GameOver);


            return true;
        }
        else
        {
            return false;
        }
    }

    public bool PlayerIsInGame()
    {
        return SceneSwitcher.ActiveSceneName == "Test";
    }

    public void TogglePauseGameState()
    {
        if (!GameIsPaused)
        {
            GameIsPaused = true;
            Time.timeScale = 0.0f;

            ShowGamePhaseText("Game Paused!", Color.cyan);
        }
        else
        {
            _UI_GamePhaseText.enabled = false;

            Time.timeScale = 1.0f;
            GameIsPaused = false;
        }
    }

    private void ChangeGameState(GameStates newGameState)
    {
        PreviousGameState = GameState;
        bool prevGameStateWasStartup = GameState == GameStates.Startup;
        GameState = newGameState;

        _GameStateStartTime = Time.time;


        switch (GameState)
        {
            case GameStates.Menu:
                MusicPlayer.FadeToTrack(MusicParams.PlayerBuildPhaseMusic, false, MusicParams.PlayerBuildPhaseMusicVolume);

                EnableHUD(false);
                break;

            case GameStates.PlayerBuildPhase:
                MusicPlayer.FadeToTrack(MusicParams.PlayerBuildPhaseMusic, false, MusicParams.PlayerBuildPhaseMusicVolume);

                ResourceManager.RestoreResourceNodes();
                _UI_TimeToNextWaveText.enabled = true;

                if (!prevGameStateWasStartup)
                    StartCoroutine(ShowGamePhaseTextAndFadeOut("Monster Attack Defeated!", Color.green));

                break;

            case GameStates.MonsterAttackPhase:
                MusicPlayer.FadeToTrack(MusicParams.MonsterAttackPhaseMusic, true, MusicParams.MonsterAttackPhaseMusicVolume);

                _UI_MonstersLeftText.enabled = true;
                _UI_WaveNumberText.enabled = true;
                StartCoroutine(ShowGamePhaseTextAndFadeOut("Monsters Are Attacking!", Color.red));

                MonsterManager.BeginNextWave();
                break;

            case GameStates.GameOver:
                if (PreviousGameState != GameStates.MonsterAttackPhase)
                    MusicPlayer.FadeToTrack(MusicParams.MonsterAttackPhaseMusic, true, MusicParams.MonsterAttackPhaseMusicVolume);

                // Disable HUD.
                //EnableHUD(false);

                // Disable player input.
                InputManager.EnableInputActionMap((int)InputActionMapIDs.Player, false);

                // Switch to game over camera.
                CameraManager.SwitchToCamera((int)CameraIDs.GameOver);

                // Fade the screen to somewhat red.
                SceneSwitcher.FadeOut(new Color32(128, 0, 0, 180), 2.5f);

                ShowGamePhaseText("Game Over!", Color.red);


                if (HighScores.IsNewHighScore(Score, SurvivalTime))
                {
                    // Activate the parent canvas of the name entry dialog to show the dialog.
                    _UI_HighScoreNameEntryDialog.OpenDialog();
                }


                break;

        } // end switch GameState


        OnGameStateChanged?.Invoke(newGameState);
    }

    private void GameState_Menu()
    {
        float timeToNextWave = _BuildPhaseLength - (Time.time - _GameStateStartTime);

        if (timeToNextWave <= 0)
        {
            // Reset the start time of the current game state to force the build mode timer to restart.
            // This is because the menu game state does not switch to monster attack phase, so we have to do this manually here.
            _GameStateStartTime = Time.time;

            // Instead of switching to a monster attack phase, we just restore resource nodes so the villagers can stay busy.
            ResourceManager.RestoreResourceNodes();

            return;
        }

        UpdateWaveTimer(timeToNextWave);
    }

    private void GameState_PlayerBuildPhase()
    {
        float timeToNextWave = _BuildPhaseLength - (Time.time - _GameStateStartTime);

        if (timeToNextWave <= 0 ||
            (InputManager.Player.EndBuildPhase && !BuildModeManager.IsBuildModeActive &&                    // Is the player pressing the end build phase button while not in build mode
             !Dialog_Base.AreAnyDialogsOpen() && !GamePhaseTextIsVisible &&                                 // With no dialogs or game phase text currently open
             !SceneSwitcher.IsFading && !SceneSwitcher.IsTransitioningToScene && !MusicPlayer.IsFading))    // And no transitions taking place?
        {
            timeToNextWave = 0;
            _UI_TimeToNextWaveText.enabled = false;

            ChangeGameState(GameStates.MonsterAttackPhase);

            return;
        }

        UpdateWaveTimer(timeToNextWave);
    }

    private void GameState_MonsterAttackPhase()
    {
        int monstersLeft = MonsterManager.MonstersLeft;

        _UI_MonstersLeftText.text = $"Monsters Left: {monstersLeft} of {MonsterManager.CurrentWaveSize}";
        _UI_WaveNumberText.text = $"Wave #{MonsterManager.CurrentWaveNumber} Incoming!";
        

        if (MonsterManager.WaveComplete)
        {
            _UI_TechTreeDialog.AddXP(XP_EarnedPerWave);

            _UI_MonstersLeftText.enabled = false;
            _UI_WaveNumberText.enabled = false;

            // Give player a scoring bonus for clearing the wave.
            AddToScore(MonsterManager.CurrentWaveNumber * 100);

            ChangeGameState(GameStates.PlayerBuildPhase);
        }

    }

    private void GameState_GameOver()
    {        
        //Debug.LogError("Game Over!");
    }

    private void UpdateWaveTimer(float timeToNextWave)
    {
        _UI_TimeToNextWaveText.text = $"Next Wave In: {HighScores.TimeValueToString(timeToNextWave, false)}";
    }

    private void OnPlayerDeath(GameObject sender, GameObject attacker)
    {
        ChangeGameState(GameStates.GameOver);
    }

    private void EnableHUD(bool state = true)
    {
        if (_UI_PlayerHealthBar)
            _UI_PlayerHealthBar.gameObject.SetActive(state);


        _UI_TopBar.gameObject.SetActive(state);
        /*
        UI_TopBar.enabled = state;
        UI_MonstersLeftText.enabled = state;
        UI_ScoreText.enabled = state;
        UI_TimeToNextWaveText.enabled = state;
        UI_WaveNumberText.enabled = state;
        */

        _UI_BottomBar.gameObject.SetActive(state);
        /*
        UI_BottomBar.enabled = state;        
        UI_PopulationCountText.enabled = state;
        UI_FoodCountText.enabled = state;
        UI_WoodCountText.enabled = state;
        UI_StoneCountText.enabled = state;
        */
    }

    private IEnumerator ShowGamePhaseTextAndFadeOut(string text, Color32 color)
    {
        _UI_GamePhaseText.text = text;
        _UI_GamePhaseText.fontMaterial.SetColor("_FaceColor", color);
        _UI_GamePhaseText.outlineColor = Color.black;

        _UI_GamePhaseText.enabled = true;


        yield return _GamePhaseDisplayDelay;


        float fadeStartTime = Time.time;
        float elapsedTime = 0;


        if (GameState == GameStates.PlayerBuildPhase &&
            PreviousGameState == GameStates.MonsterAttackPhase)
        {
            StartCoroutine(ShowLevelUpDialogAfter(GamePhaseTextFadeOutTime - 1.25f));
        }


        while (elapsedTime < GamePhaseTextFadeOutTime)
        {
            elapsedTime += Time.deltaTime;

            _UI_GamePhaseText.fontMaterial.SetColor("_FaceColor", Color.Lerp(color, Color.clear, elapsedTime));
            _UI_GamePhaseText.outlineColor = Color.Lerp(Color.black, Color.clear, elapsedTime);

            yield return null;
        }


        _UI_GamePhaseText.enabled = false;
    }

    private IEnumerator ShowLevelUpDialogAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        _UI_LevelUpDialog.OpenDialog();
    }

    private void ShowGamePhaseText(string text, Color32 color)
    {
        _UI_GamePhaseText.text = text;
        _UI_GamePhaseText.fontMaterial.SetColor("_FaceColor", color);
        _UI_GamePhaseText.outlineColor = Color.black;

        _UI_GamePhaseText.enabled = true;
    }



    public LevelUpDialog LevelUpDialog
    {
        get { return _UI_LevelUpDialog; }
    }

    public RadialMenuDialog RadialMenuDialog
    {
        get { return _UI_RadialMenuDialog; }
    }

    public TechTreeDialog TechTreeDialog
    {
        get { return _UI_TechTreeDialog; }
    }



    public GameStates PreviousGameState { get; private set; } = GameStates.Startup;
    public GameStates GameState { get; private set; } = GameStates.Startup;
    public bool GameIsPaused { get; private set; }
    public bool GamePhaseTextIsVisible {  get { return _UI_GamePhaseText.enabled; } }

    public GameObject Player { get; private set; }

    public MusicPlayer MusicPlayer { get; private set; }
    public SceneSwitcher SceneSwitcher { get; private set; }

    public BuildModeManager BuildModeManager { get; private set; }
    public CameraManager CameraManager { get; private set; }
    public InputManager InputManager { get; private set; }
    public MonsterManager MonsterManager { get; private set; }
    public NavMeshManager NavMeshManager { get; private set; }
    public ResourceManager ResourceManager { get; private set; }

    public int TotalMonstersKilled { get; private set; }

    public VillageManager_Buildings VillageManager_Buildings { get; private set; }
    public VillageManager_Villagers VillageManager_Villagers { get; private set; }


    public int Score { get; private set; }
    public float SurvivalTime { get; private set; } // The total amount of time the player has survived so far.


}
