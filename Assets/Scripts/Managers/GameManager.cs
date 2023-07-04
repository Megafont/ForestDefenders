using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityObject = UnityEngine.Object;
using Debug = UnityEngine.Debug;

using Cinemachine;
using TMPro;
using Unity.VisualScripting;
using System.Diagnostics;
using Test;

public class GameManager : MonoBehaviour
{
    public static bool _SpawnBoy = true;


    [Header("Game Manager")]
    [Tooltip("Specifies whether or not this scene is the main menu scene.")]
    public bool IsMainMenuScene = false;

    [Tooltip("When the game starts up, wait this long before doing delayed initialization steps.  WARNING: You may sometimes get a lag spike when this delayed initialization occurs, probably in part due to the nav mesh generation. As such, it is best for this value to be as low as possible so it hopefully occurs before the player starts playing.")]
    [Range(0f, 5f)]
    [SerializeField] private float _StartupInitDelay = 1.0f;


    [Header("Debug Cheats")]
    public bool ConstructionIsFree = false;
    public bool ResearchIsFree = false;
    public bool GodMode = false;
    public bool StartWithAllTechUnlocked = false;
    public bool StartWithAllZonesBridged = false;
    public bool DisableMonstersSpawning = false;
    public bool DrawAIPaths = false;



    [Header("Combat")]
    [Tooltip("The max percentage that the amount of damage an attack does can vary by. If this percentage is 0, then the amount of damage dealt is always equal to the original attack damage.")]
    [Range(0f, 1f)]
    public float AttackDamageVariance = 0.2f;


    [Header("Player")]
    public Transform PlayerSpawnPoint;
    public bool PlayerCanDamageVillagers = true;
    public float PlayerFallOutOfWorldDeathHeight = -32;
    public float PlayerHealFoodCostMultiplier = 2f;
    public int StartingXP = 0;
    public int XP_EarnedPerWave = 5;


    [Header("Initial Stats")]
    public float PlayerStartingAttackPower = 10f;
    public float PlayerStartingMaxHealth = 25f;
    public float PlayerStartingGatherRate = 5f;
    [Space(10)]
    public float VillagersStartingAttackPower = 10f;
    public float VillagersStartingMaxHealth = 25f;
    public float VillagersStartingGatherRate = 5f;


    [Header("Level Up Buffs")]
    public float PlayerAttackBuffAmountPerLevelUp = 2f;
    public float PlayerGatheringBuffAmountPerLevelUp = 1f;
    public float PlayerHealthBuffAmountPerLevelUp = 5f;
    [Space(10)]
    public float VillagersAttackBuffAmountPerLevelUp = 2f;
    public float VillagersGatheringBuffAmountPerLevelUp = 1f;
    public float VillagersHealthBuffAmountPerLevelUp = 5f;


    [Header("Level Up Caps")]
    public float MaxAttackPowerCap = 100f;
    public float MaxHealthCap = 200f;
    [Tooltip("The maximum amount of a resource that can be collected in a single punch.")]
    public float MaxGatheringCap = 20f;


    [Header("Scoring")]
    public int PlayerGatheringScoreMultiplier = 2;
    public int PlayerResearchScoreMultiplier = 100;


    [Header("Sound & Music")]
    public MusicParams MusicParams;
    public SoundParams SoundParams;


    [Header("UI")]
    [Tooltip("When the player closes a dialog, this much time will elapse before player input is re-enabled. This prevents the player character from acting on the last UI button press.")]
    [Range(0.1f, 1.0f)]
    public float DialogCloseInputChangeDelay = 0.2f;
    [Tooltip("This much time in seconds must elapse before the menu will respond to another input event to keep it from moving too fast.")]
    [Range(0.1f, 1.0f)]
    public float GamepadMenuSelectionDelay = 0.2f;


    [Header("UI (Game Phase Text)")]
    public float GamePhaseTextFadeOutTime = 2.5f;


    [Header("UI (Bottom Bar)")]
    public Color ResourceStockPilesLowColor = Color.red;
    public Color ResourceStockPilesOkColor = new Color32(255, 128, 0, 255);
    public Color ResourceStockPilesPlentifulColor = Color.white;




    private TMP_Text _UI_GamePhaseText;
    private GameOverDialog _UI_GameOverDialog;
    private LevelUpDialog _UI_LevelUpDialog;
    private PauseMenuDialog _UI_PauseMenuDialog;
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
    private PlayerHungerBar _UI_PlayerHungerBar;
    private TMP_Text _UI_KillComboCountText;

    private Image _UI_BottomBar;
    private TMP_Text _UI_VillagerCountText;
    private TMP_Text _UI_BuildingCountText;
    private TMP_Text _UI_XPCountText;
    private TMP_Text _UI_FoodCountText;
    private TMP_Text _UI_WoodCountText;
    private TMP_Text _UI_StoneCountText;

    private GameObject _UI_FloatingStatusBarPrefab;
    private Camera _UI_FloatingStatusBarOverviewCam; // The overview cam that draws the floating status bars over all geometry.


    public static GameManager Instance;

    private bool _PlayerDrowned = false;

    private AudioSource _BirdsAudioSource;

    private float _BuildPhaseLength;

    private float _GameStateStartTime;

    
    private WaitForSeconds _GamePhaseDisplayDelay = new WaitForSeconds(2.5f);

    private List<BridgeConstructionZone> _BridgeConstructionZones;

    private Texture2D _AreasMap;

    public delegate void GameManager_GameStateChangedEventHandler(GameStates newGameState);
    public delegate void GameManager_GameOverEventHandler();

    public event GameManager_GameStateChangedEventHandler OnGameStateChanged;
    public event GameManager_GameOverEventHandler OnGameOver;



    void Awake()
    {
        AreasMap = Resources.Load<Texture2D>("Areas Map");
        if (AreasMap == null)
            throw new Exception("Failed to load the areas map!");


        DoDevCheatsCheck();


        if (Instance == null)
            Instance = this;
        else
            throw new Exception("There is already a GameManager present in the scene!");


        if (PlayerSpawnPoint == null)
            throw new Exception("The player spawn point has not been set in the inspector!");


        GameObject terrainObj = GameObject.Find("Level Terrain");
        if (!terrainObj)
            throw new Exception("The GameObject \"Level Terrain\" was not found!");
        
        _BirdsAudioSource = terrainObj.GetComponent<AudioSource>();


        // We use Renderer.bounds since it is in world space. Meanwhile, MeshFilter.bounds and Renderer.localbounds are in local space.                                                       
        GameObject ground = terrainObj.transform.Find("Geometry").gameObject;
        ground = ground.transform.Find("Ground").gameObject;
        TerrainBounds = ground.GetComponent<MeshRenderer>().bounds;


        GetManagerReferences();

        if (!IsMainMenuScene)
            SpawnPlayer();


        InitUI();
        GetBridgeConstructionZoneReferences();


        if (StartingXP > 0)
            _UI_TechTreeDialog.AddXP(StartingXP);


        if (MusicPlayer.Instance)
        {
            MusicPlayer = MusicPlayer.Instance;
        }
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


        if (IsMainMenuScene)
            ChangeGameState(GameStates.MainMenu);
        else
            ChangeGameState(GameStates.PlayerBuildPhase);


        InitCameras();


        // Fade in the scene.        
        SceneSwitcher.FadeIn();

        StartCoroutine("DoDelayedInitialization");
    }

    private void OnDestroy()
    {
        Instance = null;
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
            case GameStates.MainMenu:
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


        if (!IsMainMenuScene) // Are we in a level?
        {
            if (PlayerHasSpawned && GameState != GameStates.GameOver)
            {
                CheckIfGameIsOver();
            }
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
    
    [Conditional("DEBUG")] // This function only needs to be called in a DEBUG build.
    private void DoDevCheatsCheck()
    {
        if (ConstructionIsFree)
            Debug.LogWarning("The dev cheat ConstructionIsFree is on in the game manager.");
        if (ResearchIsFree)
            Debug.LogWarning("The dev cheat ResearchIsFree is on in the game manager.");
        if (GodMode)
            Debug.LogWarning("The dev cheat GodMode is on in the game manager.");
        if (StartWithAllTechUnlocked)
            Debug.LogWarning("The dev cheat StartWithAllTechUnlocked is on in the game manager.");
        if (StartWithAllZonesBridged)
            Debug.LogWarning("The dev cheat StartWithAllZonesBridged is on in the game manager.");
        if (DisableMonstersSpawning)
            Debug.LogWarning("The dev cheat DisableMonstersSpawning is on in the game mamanger.");
    }

    private IEnumerator DoDelayedInitialization()
    {
        yield return new WaitForSeconds(_StartupInitDelay);


        NavMeshManager.RegenerateAllNavMeshes();

        VillageManager_Buildings.DoDelayedInitialization();
        VillageManager_Villagers.DoDelayedInitialization();
    }

    /// <summary>
    /// Updates all HUD stats that are not specific to a given GameState.
    /// </summary>
    private void UpdateCommonHUDStats()
    {
        // Update the top bar stats.
        _UI_VillagersLostCountText.text = $"Villagers Lost: {VillageManager_Villagers.TotalVillagersLost:n0}";
        _UI_MonstersKilledCountText.text = $"Monsters Killed: {MonsterManager.TotalMonstersKilled:n0}";
        _UI_SurvivalTimeText.text = $"Survival Time: {Utils_HighScores.TimeValueToString(SurvivalTime):n0}";
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

        UpdateResourceStockpileCounterUI(_UI_FoodCountText, "Food: ", ResourceManager.GetStockpileLevel(ResourceTypes.Food));
        UpdateResourceStockpileCounterUI(_UI_WoodCountText, "Wood: ", ResourceManager.GetStockpileLevel(ResourceTypes.Wood));
        UpdateResourceStockpileCounterUI(_UI_StoneCountText, "Stone: ", ResourceManager.GetStockpileLevel(ResourceTypes.Stone));

    }

    private void SpawnPlayer()
    {
        GameObject playerPrefab = _SpawnBoy ? Resources.Load<GameObject>("Player/Player_Male") :
                                              Resources.Load<GameObject>("Player/Player_Female");
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

        PlayerHasSpawned = true;
    }

    private void UpdateResourceStockpileCounterUI(TMP_Text textUI, string labelText, float currentAmount)
    {
        textUI.text = $"{labelText}{currentAmount:n0}";


        Color startColor = Color.white;
        Color endColor = Color.white;
        float colorBlendAmount = 0f;

        float lowThreshold = ResourceManager.ResourceStockpilesLowThreshold;
        float okThreshold = ResourceManager.ResourceStockpilesOkThreshold;
        float plentifulThreshold = ResourceManager.ResourceStockpilesPlentifulThreshold;


        if (currentAmount >= plentifulThreshold)
        {
            textUI.color = ResourceStockPilesPlentifulColor;
            return;
        }
        else if (currentAmount <= lowThreshold)
        {
            textUI.color = ResourceStockPilesLowColor;
            return;
        }
        else if (currentAmount > lowThreshold && currentAmount <= okThreshold)
        {
            startColor = ResourceStockPilesLowColor;
            endColor = ResourceStockPilesOkColor;
            colorBlendAmount = (currentAmount - lowThreshold) / (okThreshold - lowThreshold);
        }
        else if (currentAmount > okThreshold && currentAmount <= plentifulThreshold)
        {
            startColor = ResourceStockPilesOkColor;
            endColor = ResourceStockPilesPlentifulColor;
            colorBlendAmount = (currentAmount - okThreshold) / (plentifulThreshold - okThreshold);
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
        CameraManager.OnCameraTransitionStarted += OnCameraTransitionStarted;
        CameraManager.OnCameraTransitionEnded += OnCameraTransitionEnded;


        switch (SceneSwitcher.ActiveSceneName)
        {
            case "Main Menu":
                InitCameras_MenuScene();
                break;

            case "Level 01":
                InitCameras_Level();
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
        ICinemachineCamera gameOverCam = GameObject.Find("CM Game Over Camera").GetComponent<ICinemachineCamera>();
        GameObject townCenter = GameObject.FindGameObjectWithTag("Town Center");
        gameOverCam.Follow = townCenter.transform;
        gameOverCam.LookAt = townCenter.transform;
        CameraManager.RegisterCamera((int) CameraIDs.GameOver, gameOverCam);


        // Switch to the main camera for this scene.
        CameraManager.SwitchToCamera((int) CameraIDs.GameOver);
    }

    private void InitCameras_Level()
    {
        // Find the world space UI overlay camera.
        GameObject cam = GameObject.Find("World Space UI Camera");
        if (cam)
            _UI_FloatingStatusBarOverviewCam = cam.GetComponent<Camera>();


        // Register player follow camera.
        ICinemachineCamera mainCam = GameObject.Find("CM Player Follow Camera").GetComponent<ICinemachineCamera>();
        CameraManager.RegisterCamera((int) CameraIDs.PlayerFollow, mainCam);

        // Register game over camera.
        ICinemachineCamera gameOverCam = GameObject.Find("CM Game Over Camera").GetComponent<ICinemachineCamera>();
        gameOverCam.Follow = Player.transform;
        gameOverCam.LookAt = Player.transform;
        CameraManager.RegisterCamera((int) CameraIDs.GameOver, gameOverCam);


        // Switch to the main camera for this scene.
        CameraManager.SwitchToCamera((int) CameraIDs.PlayerFollow);
    }

    private void InitCameras_TestScene()
    {
        // Register player follow camera.
        ICinemachineCamera mainCam = GameObject.Find("CM Player Follow Camera").GetComponent<ICinemachineCamera>();
        CameraManager.RegisterCamera((int) CameraIDs.PlayerFollow, mainCam);

        // Register game over camera.
        ICinemachineCamera gameOverCam = GameObject.Find("CM Game Over Camera").GetComponent<ICinemachineCamera>();
        gameOverCam.Follow = Player.transform;
        gameOverCam.LookAt = Player.transform;
        CameraManager.RegisterCamera((int) CameraIDs.GameOver, gameOverCam);


        // Switch to the main camera for this scene.
        CameraManager.SwitchToCamera((int) CameraIDs.PlayerFollow);
    }

    private void InitInput()
    {
        InputManager.RegisterInputActionMap((int) InputActionMapIDs.Player, "Player", true);
        InputManager.RegisterInputActionMap((int) InputActionMapIDs.BuildMode, "Build Mode", true);
        InputManager.RegisterInputActionMap((int) InputActionMapIDs.UI, "UI", true);
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
        _UI_GameOverDialog = GameObject.Find("UI/Game Over Dialog").GetComponent<GameOverDialog>();
        _UI_LevelUpDialog = GameObject.Find("UI/Level Up Dialog").GetComponent<LevelUpDialog>();
        _UI_RadialMenuDialog = GameObject.Find("UI/Radial Menu Dialog").GetComponent<RadialMenuDialog>();
        _UI_TechTreeDialog = GameObject.Find("UI/Tech Tree Dialog").GetComponent<TechTreeDialog>();

        GameObject obj = GameObject.Find("UI/Pause Menu Dialog");
        if (obj)
            _UI_PauseMenuDialog = obj.GetComponent<PauseMenuDialog>();

        _UI_TopBar = GameObject.Find("UI/HUD/Top Bar").GetComponent<Image>();
        _UI_TimeToNextWaveText = GameObject.Find("UI/HUD/Top Bar/Time To Next Wave Text (TMP)").GetComponent<TMP_Text>();
        _UI_WaveNumberText = GameObject.Find("UI/HUD/Top Bar/Wave Number Text (TMP)").GetComponent<TMP_Text>();
        _UI_MonstersLeftText = GameObject.Find("UI/HUD/Top Bar/Monsters Left Text (TMP)").GetComponent<TMP_Text>();
        _UI_VillagersLostCountText = GameObject.Find("UI/HUD/Top Bar/Villagers Lost Count Text (TMP)").GetComponent<TMP_Text>();
        _UI_MonstersKilledCountText = GameObject.Find("UI/HUD/Top Bar/Monsters Killed Count Text (TMP)").GetComponent<TMP_Text>();
        _UI_SurvivalTimeText = GameObject.Find("UI/HUD/Top Bar/Survival Time Text (TMP)").GetComponent<TMP_Text>();
        _UI_ScoreText = GameObject.Find("UI/HUD/Top Bar/Score Text (TMP)").GetComponent<TMP_Text>();

        _UI_PlayerHealthBar = GameObject.Find("Player Health Bar").GetComponent<PlayerHealthBar>();
        _UI_PlayerHungerBar = GameObject.Find("Player Hunger Bar").GetComponent<PlayerHungerBar>();        
        _UI_KillComboCountText = GameObject.Find("UI/HUD/Kill Combo Count Text (TMP)").GetComponent<TMP_Text>();

        _UI_BottomBar = GameObject.Find("UI/HUD/Bottom Bar").GetComponent<Image>();
        _UI_VillagerCountText = GameObject.Find("UI/HUD/Bottom Bar/Villager Count Text (TMP)").GetComponent<TMP_Text>();
        _UI_BuildingCountText = GameObject.Find("UI/HUD/Bottom Bar/Building Count Text (TMP)").GetComponent<TMP_Text>();
        _UI_XPCountText = GameObject.Find("UI/HUD/Bottom Bar/XP Count Text (TMP)").GetComponent<TMP_Text>();
        _UI_FoodCountText = GameObject.Find("UI/HUD/Bottom Bar/Food Count Text (TMP)").GetComponent<TMP_Text>();
        _UI_WoodCountText = GameObject.Find("UI/HUD/Bottom Bar/Wood Count Text (TMP)").GetComponent<TMP_Text>();
        _UI_StoneCountText = GameObject.Find("UI/HUD/Bottom Bar/Stone Count Text (TMP)").GetComponent<TMP_Text>();

        _UI_FloatingStatusBarPrefab = Resources.Load<GameObject>("UI/Common/Floating Stat Bar");
    }

    private void GetBridgeConstructionZoneReferences()
    {
        UnityObject[] objs = GameObject.FindObjectsOfType(typeof(BridgeConstructionZone), false);

        _BridgeConstructionZones = new List<BridgeConstructionZone>();
        foreach (UnityObject uObj in objs)
        {
            if (uObj.GameObject().TryGetComponent(out BridgeConstructionZone bZone))
                _BridgeConstructionZones.Add(bZone);
        }

        _BridgeConstructionZones.Sort(CompareBridgeConstructionZoneTo);
    }

    private int CompareBridgeConstructionZoneTo(BridgeConstructionZone a, BridgeConstructionZone b)
    {
        //Debug.Log($"A: \"{a.name}\"    B: \"{b.name}\"    Result: {a.name.CompareTo(b.name)}");

        return a.name.CompareTo(b.name);
    }

    public void AddToScore(int amount)
    {
        // If the player is dead, simply return. This ignores any additional points being earned by villagers killing monsters.
        if (GameState == GameStates.GameOver ||
            Player == null ||
            (Player != null && Player.GetComponent<Health>().CurrentHealth == 0))
        {
            return;
        }


        if (amount <= 0)
            Debug.LogError("The amount to add to the score must be positive!");

        Score += amount;
    }

    public bool CheckIfGameIsOver()
    {
        // NOTE: We don't check if the player is dead here, as we receive an event when that happens via the OnPlayerDeath() method below, which instantly switches us to the GameOver game state;
        if (Player == null ||
            Player.transform.position.y <= PlayerFallOutOfWorldDeathHeight ||
            GameState == GameStates.GameOver)
        {
            ChangeGameState(GameStates.GameOver);

            return true;
        }
        else
        {
            return false;
        }
    }

    public void TogglePauseGameState()
    {
        Debug.Log($"Game State {GameState}");

        if (GameState == GameStates.Startup || 
            GameState == GameStates.GameOver || 
            GameState == GameStates.MainMenu)
        {
            GameIsPaused = false;
            return;
        }


        if (!GameIsPaused)
        {
            GameIsPaused = true;
            Time.timeScale = 0.0f;

            _UI_PauseMenuDialog.OpenDialog();
        }
        else
        {
            _UI_PauseMenuDialog.CloseDialog();

            Time.timeScale = 1.0f;
            GameIsPaused = false;
        }
    }

    private void ChangeGameState(GameStates newGameState)
    {
        // We don't need to do anything if the new game state is the same one we're already in.
        if (newGameState == GameState)
            return;


        PreviousGameState = GameState;
        bool prevGameStateWasStartup = GameState == GameStates.Startup;
        GameState = newGameState;

        _GameStateStartTime = Time.time;


        switch (GameState)
        {
            case GameStates.MainMenu:
                MusicPlayer.FadeToTrack(MusicParams.PlayerBuildPhaseMusic, false, MusicParams.PlayerBuildPhaseMusicVolume);

                // Fade in the bird sounds.
                StartCoroutine(Utils_Audio.FadeAudioSource(_BirdsAudioSource, 0f, SoundParams.BirdSoundsVolume, SoundParams.BirdSoundsFadeTime));

                EnableHUD(false);
                break;

            case GameStates.PlayerBuildPhase:
                MusicPlayer.FadeToTrack(MusicParams.PlayerBuildPhaseMusic, false, MusicParams.PlayerBuildPhaseMusicVolume);

                // Fade in the bird sounds.
                StartCoroutine(Utils_Audio.FadeAudioSource(_BirdsAudioSource, 0f, SoundParams.BirdSoundsVolume, SoundParams.BirdSoundsFadeTime));

                ResourceManager.RestoreResourceNodes();
                _UI_TimeToNextWaveText.enabled = true;

                if (!prevGameStateWasStartup)
                    StartCoroutine(ShowGamePhaseTextAndFadeOut("Monster Attack Defeated!", Color.green));

                break;

            case GameStates.MonsterAttackPhase:
                MusicPlayer.FadeToTrack(MusicParams.MonsterAttackPhaseMusic, true, MusicParams.MonsterAttackPhaseMusicVolume);

                // Fade out the bird sounds.
                StartCoroutine(Utils_Audio.FadeAudioSource(_BirdsAudioSource, SoundParams.BirdSoundsVolume, 0f, SoundParams.BirdSoundsFadeTime));


                _UI_MonstersLeftText.enabled = true;
                _UI_WaveNumberText.enabled = true;
                StartCoroutine(ShowGamePhaseTextAndFadeOut("Monsters Are Attacking!", Color.red));

                MonsterManager.BeginNextWave();
                break;

            case GameStates.GameOver:
                // Fade out the bird sounds.
                StartCoroutine(Utils_Audio.FadeAudioSource(_BirdsAudioSource, SoundParams.BirdSoundsVolume, 0f, SoundParams.BirdSoundsFadeTime));

                if (PreviousGameState != GameStates.MonsterAttackPhase)
                {
                    MusicPlayer.FadeToTrack(MusicParams.MonsterAttackPhaseMusic, true, MusicParams.MonsterAttackPhaseMusicVolume);

                    _UI_TimeToNextWaveText.enabled = false;

                    _UI_MonstersLeftText.enabled = true;
                    _UI_WaveNumberText.enabled = true;

                    MonsterManager.BeginNextWave();
                }


                // Disable HUD.
                //EnableHUD(false);

                // Disable player input.
                InputManager.EnableInputActionMap((int) InputActionMapIDs.Player, false);

                // Switch to game over camera.
                SetupGameOverCamera();
                CameraManager.SwitchToCamera((int) CameraIDs.GameOver);

                // Fade the screen to somewhat red.
                SceneSwitcher.FadeOut(new Color32(128, 0, 0, 180), 2.5f);

                ShowGamePhaseText("Game Over!", Color.red);

                // Fire the game over event.
                OnGameOver?.Invoke();

               if (Utils_HighScores.IsNewHighScore(Score, SurvivalTime))
               {
                    // Activate the parent canvas of the name entry dialog to show the dialog.
                    _UI_GameOverDialog.OpenDialog();
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
             !SceneSwitcher.IsFading && !SceneSwitcher.IsTransitioningToScene && !MusicPlayer.IsFading && !CameraManager.IsTransitioning))    // And no transitions taking place?
        {

            if (!DisableMonstersSpawning)
            {
                timeToNextWave = 0;
                _UI_TimeToNextWaveText.enabled = false;

                ChangeGameState(GameStates.MonsterAttackPhase);
            }

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
            TextPopup.ShowTextPopup(TextPopup.AdjustStartPosition(Player.gameObject),
                                    $"+{XP_EarnedPerWave} XP", 
                                    TextPopupColors.XPColor);

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

        int monstersLeft = MonsterManager.MonstersLeft;

        _UI_MonstersLeftText.text = $"Monsters Left: {monstersLeft} of {MonsterManager.CurrentWaveSize}";
        _UI_WaveNumberText.text = $"Wave #{MonsterManager.CurrentWaveNumber} Incoming!";
    }

    private void SetupGameOverCamera()
    {
        ICinemachineCamera gameOvercam = CameraManager.GetCameraWithID((int) CameraIDs.GameOver);
        if (Player.transform.position.y <= PlayerFallOutOfWorldDeathHeight)
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
    }

    private void UpdateWaveTimer(float timeToNextWave)
    {
        if (timeToNextWave < 0)
            timeToNextWave = 0;

        _UI_TimeToNextWaveText.text = $"Next Wave In: {Utils_HighScores.TimeValueToString(timeToNextWave, false)}";
    }

    private void OnPlayerDeath(GameObject sender, GameObject attacker)
    {
        PlayerIsDead = true;

        ChangeGameState(GameStates.GameOver);
    }

    public void PlayerFellInWater()
    {
        // Don't let the code in this function run more than once.
        if (_PlayerDrowned)
            return;


        _PlayerDrowned = true;
    }

    private void EnableHUD(bool state = true)
    {
        if (_UI_PlayerHealthBar)
            _UI_PlayerHealthBar.gameObject.SetActive(state);
        if (_UI_PlayerHungerBar)
            _UI_PlayerHungerBar.gameObject.SetActive(state);


        _UI_TopBar.gameObject.SetActive(state);
        _UI_BottomBar.gameObject.SetActive(state);
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

    public BridgeConstructionZone GetBridgeZoneBeforeArea(LevelAreas area)
    {
        foreach (BridgeConstructionZone zone in _BridgeConstructionZones)
        {
            if (zone.PrevArea == area)
                return zone;
        }


        return null;
    }

    public BridgeConstructionZone GetBridgeZoneAfterArea(LevelAreas area)
    {
        foreach (BridgeConstructionZone zone in _BridgeConstructionZones)
        {
            if (zone.NextArea == area)
                return zone;
        }


        return null;
    }

    public GameObject GetFloatingStatusBarPrefab()
    {
        return _UI_FloatingStatusBarPrefab;
    }

    
    private void OnCameraTransitionStarted(ICinemachineCamera startCam, ICinemachineCamera endCam)
    {
        // Disable the floating status bars.
        if (_UI_FloatingStatusBarOverviewCam != null)
        {
            _UI_FloatingStatusBarOverviewCam.gameObject.SetActive(false);
        }
        
    }

    private void OnCameraTransitionEnded(ICinemachineCamera startCam, ICinemachineCamera endCam)
    {
        // Show the floating status bars only when the player follow cam is active.
        // This is an easy way to enable/disable them, without messing with the camera
        // stack on the player camera, which specifies the overlay camera.
        if (_UI_FloatingStatusBarOverviewCam != null)
        {
            bool showFloatingStatusBars = (endCam.VirtualCameraGameObject.name == "CM Player Follow Camera");
            _UI_FloatingStatusBarOverviewCam.gameObject.SetActive(showFloatingStatusBars);
        }

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

    public Bounds TerrainBounds { get; private set; } // The terrain bounds in world space.

    public bool PlayerHasSpawned { get; private set; } = false;
    public bool PlayerIsDead { get; private set; }

    public int TotalMonstersKilled { get; private set; }


    public VillageManager_Buildings VillageManager_Buildings { get; private set; }
    public VillageManager_Villagers VillageManager_Villagers { get; private set; }


    public int Score { get; private set; }
    public float SurvivalTime { get; private set; } // The total amount of time the player has survived so far.


    public Texture2D AreasMap { get; private set; }

    public int BridgeConstructionZoneCount { get { return _BridgeConstructionZones.Count; } }
    public List<BridgeConstructionZone> BridgeConstructionZonesList {  get { return _BridgeConstructionZones; } }
    public BridgeConstructionZone GetBridgeConstructionZone(int index) {  return _BridgeConstructionZones[index]; }

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



}
