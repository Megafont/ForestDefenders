using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Cinemachine;
using StarterAssets;
using TMPro;


public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text UI_TimeToNextWaveText;
    public TMP_Text UI_WaveNumberText;
    public TMP_Text UI_MonstersLeftText;
    public TMP_Text UI_ScoreText;


    public static GameManager Instance;

    public GameObject Player { get; private set; }

    public BuildModeManager BuildModeManager { get; private set; }
    public StarterAssetsInputs PlayerInput { get; private set; }
    public MonsterManager MonsterManager { get; private set; }


    public RadialMenu UI_RadialMenu { get; private set; }


    private float _BuildPhaseLength = 10f; // BuildModeDefinitions.BuildPhaseBaseLength;

    private float _GameStateStartTime;

    private int _Score;



    public GameStates GameState { get; private set; } = GameStates.Startup;


    public enum GameStates
    {
        Startup,
        PlayerBuildPhase,
        MonsterAttackPhase,
        GameOver,
    }


    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            throw new Exception("There is already a GameManager present in the scene!");


        
        GameObject playerPrefab = Resources.Load<GameObject>("Player/Player (Male)");
        GameObject playerObj = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        Player = playerObj.transform.Find("Player Armature").gameObject;
        Player.GetComponent<Health>().OnDeath += OnPlayerDeath;


        BuildModeManager = GameObject.Find("Build Mode Manager").GetComponent<BuildModeManager>();
        MonsterManager = GameObject.Find("Monster Manager").GetComponent<MonsterManager>();
        PlayerInput = Player.gameObject.GetComponent<StarterAssetsInputs>();


        InitRadialMenu();

        UI_MonstersLeftText.enabled = false;
        UI_TimeToNextWaveText.enabled = false;
        UI_WaveNumberText.enabled = false;
        UI_ScoreText.text = UI_ScoreText.text = "Score: 0000000000";


        ChangeGameState(GameStates.PlayerBuildPhase);
    }

    // Update is called once per frame
    void Update()
    {
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

    private void InitRadialMenu()
    {
        GameObject obj = GameObject.Find("Radial Menu");
        if (!obj)
            throw new Exception("The radial menu GameObject was not found!");
        else
        {
            UI_RadialMenu = obj.GetComponent<RadialMenu>();
            if (UI_RadialMenu == null)
                throw new Exception("The radial menu GameObject does not have a RadialMenu component!");
        }
    }

    public void AddToScore(int amount)
    {
        if (amount <= 0)
            Debug.LogError("The amount to add to the score must be positive!");

        _Score += amount;
        UI_ScoreText.text = $"Score: {_Score:0000000000}";
    }

    private void ChangeGameState(GameStates newState)
    {
        MonsterManager.ResetComboStreak();

        GameState = newState;
        _GameStateStartTime = Time.time;


        switch (GameState)
        {
            case GameStates.PlayerBuildPhase:
                UI_TimeToNextWaveText.enabled = true;

                break;

            case GameStates.MonsterAttackPhase:
                UI_MonstersLeftText.enabled = true;
                UI_WaveNumberText.enabled = true;

                MonsterManager.BeginNextWave();
                break;

            case GameStates.GameOver:
                DisableHUD();
                break;

        } // end switch GameState
    }

    private void GameState_PlayerBuildPhase()
    {
        float timeToNextWave = _BuildPhaseLength - (Time.time - _GameStateStartTime);

        if (timeToNextWave <= 0)
        {
            UI_TimeToNextWaveText.enabled = false;

            ChangeGameState(GameStates.MonsterAttackPhase);

            return;
        }

        UpdateWaveTimer(timeToNextWave);
    }

    private void GameState_MonsterAttackPhase()
    {
        int monstersLeft = MonsterManager.MonstersLeft;

        UI_MonstersLeftText.text = $"Monsters Left: {monstersLeft} of {MonsterManager.WaveSize}";
        UI_WaveNumberText.text = $"Wave #{MonsterManager.WaveNumber} Incoming!";

        if (MonsterManager.WaveComplete)
        {
            UI_MonstersLeftText.enabled = false;
            UI_WaveNumberText.enabled = false;

            // Give player a scoring bonus for clearing the wave.
            AddToScore(MonsterManager.WaveNumber * 100);

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
    }

}

