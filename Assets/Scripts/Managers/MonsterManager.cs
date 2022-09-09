using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;


public class MonsterManager : MonoBehaviour
{
    public GameObject MonsterPrefab;

    public int CurrentComboStreak { get; private set; }
    public float ComboStreakResetTime = 5.0f;

    public int MonstersKilled { get; private set; }
    public int MonstersLeft { get { return WaveSize - MonstersKilled; } }
    public bool WaveComplete { get { return _WaveIsDoneSpawning && _Monsters.Count == 0; } }
    public int WaveNumber { get; private set; }
    public int WaveSize { get; private set; }

    public int BaseWaveSize = 5;

    public GameObject[] SpawnPoints = new GameObject[0];


    private bool _WaveIsDoneSpawning;
    private GameObject _MonstersParent;
    private float _LastWaveTime;
    private List<GameObject> _Monsters;
    private float _LastMonsterDeathTime;


    // Start is called before the first frame update
    void Start()
    {
        _Monsters = new List<GameObject>();

        _MonstersParent = GameObject.Find("Monsters");
        _LastWaveTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BeginNextWave()
    {
        WaveNumber++;
        
        StartCoroutine(SpawnWave());
    }

    public void ResetComboStreak()
    {
        CurrentComboStreak = 0;
    }

    private IEnumerator SpawnWave()
    {
        MonstersKilled = 0;
        _WaveIsDoneSpawning = false;


        if (SpawnPoints.Length < 1)
            throw new Exception("Cannot spawn monsters because no spawn points have been specified in the inspector!");


        GameObject playerBase = GameObject.Find("Player Base");

        WaveSize = GetWaveSize();
        for (int i = 0; i < WaveSize; i++)
        {
            // Randomly select a spawn point from the list.
            int index = UnityEngine.Random.Range(0, SpawnPoints.Length - 1);

            // Spawn a monster.
            GameObject monster = Instantiate(MonsterPrefab, SpawnPoints[index].transform.position, Quaternion.identity, _MonstersParent.transform);
            monster.GetComponent<TestEnemy>().SetTarget(playerBase);
            monster.GetComponent<Health>().OnDeath += OnDeath;
            _Monsters.Add(monster);

            yield return new WaitForSeconds(2.0f);
        }

        _WaveIsDoneSpawning = true;
    }

    private int GetWaveSize()
    {
        int waveSize = BaseWaveSize + (WaveNumber * WaveSize / 2);

        return waveSize;
    }

    private void OnDeath(GameObject sender)
    {
        if (Time.time - _LastMonsterDeathTime >= ComboStreakResetTime)
            ResetComboStreak();


        _LastMonsterDeathTime = Time.time;        
        CurrentComboStreak++;


        MonstersKilled++;


        int enemyScoreValue = sender.GetComponent<IEnemy>().GetScoreValue();
        GameManager.Instance.AddToScore(enemyScoreValue * CurrentComboStreak);


        //Debug.Log($"Kill Streak: {CurrentComboStreak}  Enemy Base Score: {enemyScoreValue}");


        _Monsters.Remove(sender);        
    }
}
