using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;


public class MonsterManager : MonoBehaviour
{
    public List<Monster_Base> MonsterPrefabs;

    public int CurrentComboStreak { get; private set; }
    public float ComboStreakResetTime = 5.0f;

    public int MonstersKilled { get; private set; }
    public int MonstersLeft { get { return CurrentWaveSize - MonstersKilled; } }
    public bool WaveComplete { get { return _WaveIsDoneSpawning && _AliveMonsters.Count == 0; } }
    public int CurrentWaveNumber { get; private set; }
    public int CurrentWaveSize { get; private set; }

    public int BaseMonsterSpawnAmount = 3;

    public GameObject[] SpawnPoints = new GameObject[0];


    private List<int> _MonsterSpawnList;
    private bool _WaveIsDoneSpawning;
    private GameObject _MonstersParent;
    private float _LastWaveTime;
    private List<GameObject> _AliveMonsters;
    private float _LastMonsterDeathTime;


    // Start is called before the first frame update
    void Start()
    {
        MonsterPrefabs.Sort(CompareMonsters);

        _MonsterSpawnList = new List<int>();
        _AliveMonsters = new List<GameObject>();

        _MonstersParent = GameObject.Find("Monsters");
        _LastWaveTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BeginNextWave()
    {
        CurrentWaveNumber++;
        
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

        GenerateWaveSpawnList();
        for (int i = 0; i < _MonsterSpawnList.Count; i++)
        {
            // Randomly select a spawn point from the list.
            int spawnPointIndex = UnityEngine.Random.Range(0, SpawnPoints.Length - 1);

            // Spawn a monster.
            int monsterIndex = _MonsterSpawnList[i];
            
            GameObject monster = Instantiate(MonsterPrefabs[monsterIndex].gameObject, 
                                             SpawnPoints[spawnPointIndex].transform.position,
                                             Quaternion.identity,
                                             _MonstersParent.transform);

            // Subscribe to the monster's OnDeath event and add it to the list of alive monsters.
            monster.GetComponent<Health>().OnDeath += OnDeath;
            _AliveMonsters.Add(monster);

            yield return new WaitForSeconds(2.0f);
        }


        _MonsterSpawnList.Clear();

        _WaveIsDoneSpawning = true;
    }

    /// <summary>
    /// Generates a shuffled list of prefab indices for all monsters that should spawn in the current wave.
    /// If there are 10 slimes to spawn, the Slime's monster index will be in this list 10 times.
    /// </summary>
    /// <returns>A shuffled list of prefab indices for each monster instance to be spawned in the current wave.</returns>
    private void GenerateWaveSpawnList()
    {
        _MonsterSpawnList.Clear();


        foreach (IMonster prefab in MonsterPrefabs)
        {
            int monsterIndex = MonsterPrefabs.IndexOf((Monster_Base) prefab);

            int monsterCount = GetMonsterCountForWave(prefab, monsterIndex);
            for (int i = 0; i < monsterCount; i++)
            {
                _MonsterSpawnList.Add(monsterIndex);
                Debug.Log($"Added monster index {monsterIndex} to spawn list!");
            }
        }


        CurrentWaveSize = _MonsterSpawnList.Count;

        Utils.ShuffleList(_MonsterSpawnList);
    }

    private int GetMonsterCountForWave(IMonster monster, int monsterIndex)
    {
        if (monsterIndex >= CurrentWaveNumber)
            return 0;


        return BaseMonsterSpawnAmount + (CurrentWaveNumber - (monsterIndex + 1));
    }

    private int GetMonstersInTierCount(int tier)
    {
        int count = 0;
        foreach (IMonster prefab in MonsterPrefabs)
        {
            if (prefab.GetTier() == tier)
                count++;
        }

        return count;
    }

    private void OnDeath(GameObject sender)
    {
        if (Time.time - _LastMonsterDeathTime >= ComboStreakResetTime)
            ResetComboStreak();


        _LastMonsterDeathTime = Time.time;        
        CurrentComboStreak++;


        MonstersKilled++;


        int enemyScoreValue = sender.GetComponent<IMonster>().GetScoreValue();
        GameManager.Instance.AddToScore(enemyScoreValue * CurrentComboStreak);


        //Debug.Log($"Kill Streak: {CurrentComboStreak}  Monster Base Score: {enemyScoreValue}");


        _AliveMonsters.Remove(sender);        
    }



    /// <summary>
    /// Used to compare two monsters for sorting the list from weakest to strongest.
    /// </summary>
    /// <param name="a">The first monster to compare.</param>
    /// <param name="b">The second monster to compare.</param>
    /// <returns>1 if a is greater than be, 0 if they are considered equal, and -1 if a is less than b.</returns>
    private int CompareMonsters(IMonster a, IMonster b)
    {
        int aTier = a.GetTier();
        int bTier = b.GetTier();

        if (aTier < bTier)
            return -1;
        else if (aTier > bTier)
            return 1;


        int aDangerLvl = a.GetDangerValue();
        int bDangerLvl = b.GetDangerValue();

        if ((aDangerLvl + aDangerLvl) < (bDangerLvl + bDangerLvl))
            return -1;
        else if ((aDangerLvl + aDangerLvl) > (bDangerLvl + bDangerLvl))
            return 1;
        else
            return 0;
    }

}
