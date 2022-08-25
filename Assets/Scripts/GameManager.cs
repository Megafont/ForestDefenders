using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Cinemachine;
using TMPro;


public class GameManager : MonoBehaviour
{
    public TMP_Text UI_TimeToNextWaveText;
    public TMP_Text UI_WaveNumberText;
    public TMP_Text UI_MonstersLeftText;
    public TMP_Text UI_ScoreText;

    public GameObject Player { get; private set; }

    public static GameManager Instance;


    private GameObject _MainCamera;
    private MonsterManager _MonsterManager;

    private float _BuildTime = 10f;
    private float _GameStateStartTime;




    public GameStates GameState { get; private set; } = GameStates.Startup;


    public enum GameStates
    {
        Startup,
        PlayerBuildPhase,
        MonsterAttackPhase,
        GameOver,
    }


    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            throw new Exception("There is already a GameManager present in the scene!");


        UI_MonstersLeftText.enabled = false;
        UI_TimeToNextWaveText.enabled = false;
        UI_WaveNumberText.enabled = false;

        GameObject playerPrefab = Resources.Load<GameObject>("Player/MaleCharacter");

        Player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        Player.name = "Player";
        Player.GetComponent<Health>().OnDeath += OnPlayerDeath;

        _MainCamera = GameObject.Find("CM Main Camera");
        CinemachineVirtualCamera cmCam = _MainCamera.GetComponent<CinemachineVirtualCamera>();
        cmCam.Follow = Player.transform;
        cmCam.LookAt = Player.transform;

        _MonsterManager = GameObject.Find("Monster Manager").GetComponent<MonsterManager>();

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

    private void ChangeGameState(GameStates newState)
    {
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

                _MonsterManager.BeginNextWave();
                break;

            case GameStates.GameOver:
                DisableHUD();
                break;

        } // end switch GameState
    }

    private void GameState_PlayerBuildPhase()
    {
        float timeToNextWave = _BuildTime - (Time.time - _GameStateStartTime);

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
        int monstersLeft = _MonsterManager.MonstersLeft;

        UI_MonstersLeftText.text = $"Monsters Left: {monstersLeft}";
        UI_WaveNumberText.text = $"Wave #{_MonsterManager.WaveNumber} Incoming!";

        if (monstersLeft == 0)
        {
            UI_MonstersLeftText.enabled = false;
            UI_WaveNumberText.enabled = false;

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

