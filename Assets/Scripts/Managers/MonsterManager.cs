using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;


public class MonsterManager : MonoBehaviour
{
    public List<Monster_Base> MonsterPrefabs;

    public float ComboStreakResetTime = 5.0f;

    public int BaseMonsterSpawnAmount = 3;



    private Transform _SpawnPointsParent;

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


        // Get the spawn points child object.
        _SpawnPointsParent = GameObject.Find("Monster Spawn Points").transform;
        if (_SpawnPointsParent == null)
            throw new Exception("The monster spawn points parent GameObject was not found!");

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

        int spawnPointCount = _SpawnPointsParent.transform.childCount;
        if (spawnPointCount < 1)
            throw new Exception("Cannot spawn monsters because no spawn points have been added under the \"Monster Spawn Points\" parent GameObject!");

        GenerateWaveSpawnList();
        for (int i = 0; i < _MonsterSpawnList.Count; i++)
        {
            // Randomly select a spawn point from the list.
            int spawnPointIndex = UnityEngine.Random.Range(0, spawnPointCount);

            // Spawn a monster.
            int monsterIndex = _MonsterSpawnList[i];

            Vector3 spawnPos = _SpawnPointsParent.GetChild(spawnPointIndex).transform.position;
            NavMeshHit hit;
            bool result = NavMesh.SamplePosition(spawnPos, out hit, 5.0f, NavMesh.AllAreas);
            if (!result)
            {
                Debug.LogError($"Failed to find position on nav mesh near {spawnPos}!");
                continue;
            }

            GameObject monster = Instantiate(MonsterPrefabs[monsterIndex].gameObject, 
                                             hit.position,
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
                //Debug.Log($"Added monster index {monsterIndex} to spawn list!");
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

    private void OnDeath(GameObject sender, GameObject attacker)
    {
        if (Time.time - _LastMonsterDeathTime >= ComboStreakResetTime)
            ResetComboStreak();


        _LastMonsterDeathTime = Time.time;        
        CurrentComboStreak++;


        MonstersKilled++;
        TotalMonstersKilled++;


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


        float aDangerLvl = a.GetDangerValue();
        float bDangerLvl = b.GetDangerValue();

        if ((aDangerLvl + aDangerLvl) < (bDangerLvl + bDangerLvl))
            return -1;
        else if ((aDangerLvl + aDangerLvl) > (bDangerLvl + bDangerLvl))
            return 1;
        else
            return 0;
    }



    public int CurrentComboStreak { get; private set; }

    public int MonstersKilled { get; private set; }
    public int TotalMonstersKilled { get; private set; } // Total monsters killed across all waves so far.
    public int MonstersLeft { get { return CurrentWaveSize - MonstersKilled; } }
    public bool WaveComplete { get { return _WaveIsDoneSpawning && _AliveMonsters.Count == 0; } }
    public int CurrentWaveNumber { get; private set; }
    public int CurrentWaveSize { get; private set; }


}
