using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

using Random = UnityEngine.Random;


public class MonsterManager : MonoBehaviour
{
    public List<Monster_Base> MonsterPrefabs;

    public float ComboStreakResetTime = 5.0f;

    public int BaseMonsterSpawnAmount = 5;
    public int MaxMonstersOfAnyGivenType = 5;

    [Tooltip("The percentage damage is decreased by when a monster is hit by a damage type it is resistant to.")]
    [Range(0f, 1f)]
    public float DamageResistanceBuffAmount = 0.2f;
    [Tooltip("The percentage damage is increased by when a monster is hit by a damage type it is vulnerable to.")]
    [Range(0f, 1f)]
    public float DamageVulnerabilityBuffAmount = 0.2f;


    private List<MonsterDefinition> _MonsterDefinitions;

    private Transform _SpawnPointsParent;

    private List<int> _MonsterSpawnList;
    private bool _WaveIsDoneSpawning;
    private GameObject _MonstersParent;
    private float _LastWaveTime;
    private List<GameObject> _AliveMonsters;
    private float _LastMonsterDeathTime;

    private GameManager _GameManager;


    // Start is called before the first frame update
    void Start()
    {
        _GameManager =  GameManager.Instance;

        MonsterStats.InitMonsterDefinitions(MonsterPrefabs);
        _MonsterDefinitions = MonsterStats.MonsterDefinitions;


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
        if (Time.time - _LastMonsterDeathTime >= ComboStreakResetTime)
            ResetComboStreak();
    }

    public void BeginNextWave()
    {       
        StartCoroutine(SpawnWave());
    }

    public void ResetComboStreak()
    {
        if (CurrentComboStreak > MaxComboStreak)
            MaxComboStreak = CurrentComboStreak;

        CurrentComboStreak = 0;
    }

    private IEnumerator SpawnWave()
    {
        MonstersKilled = 0;
        _WaveIsDoneSpawning = false;

        int spawnPointCount = _SpawnPointsParent.transform.childCount;
        if (spawnPointCount < 1)
            throw new Exception("Cannot spawn monsters because no spawn points have been added under the \"Monster Spawn Points\" parent GameObject!");



        CurrentWaveNumber++;
        CurrentWaveSize = BaseMonsterSpawnAmount + ((CurrentWaveNumber - 1) * 3);

        // Determine how many monster types will spawn in the current wave.
        int totalMonsterTypeCount = Mathf.Max(1, (CurrentWaveNumber + 1) / 2); // Make a new monster appear in every other wave
        if (CurrentWaveNumber > _MonsterDefinitions.Count)
            totalMonsterTypeCount = _MonsterDefinitions.Count;

        int lowLevelMonsterTypeCount = (int)(totalMonsterTypeCount * 0.33f);

        //Debug.Log($"Wave #: {CurrentWaveNumber}    Base Spawn Count: {BaseMonsterSpawnAmount}    Wave Spawn Count: {CurrentWaveSize}");
        for (int i = 0; i < CurrentWaveSize; i++)
        {
            // Randomly select a spawn point from the list.
            int spawnPointIndex = Random.Range(0, spawnPointCount);

            // Select a monster.
            float rand = Random.Range(0f, 1f);
            int monsterIndex = 0;

            float rand2 = 0f;
            if (totalMonsterTypeCount == 1)
                rand2 = 0;
            else if (rand <= 0.25f) // We give a 25% chance of a low level monster spawning.
                rand2 = Random.Range(0f, 0.25f);
            else if (rand > 0.25f && rand <= 0.65f) // We give a 40% chance of a mid level monster spawning.
                rand2 = Random.Range(0.26f, 0.65f);
            else // We give a 35% chance of a high level monster spawning.
                rand2 = Random.Range(0.66f, 1f);


            monsterIndex = (int) (totalMonsterTypeCount * rand2);

            MonsterDefinition monsterDef = _MonsterDefinitions[monsterIndex];


            // Spawn the monster.
            Vector3 spawnPos = _SpawnPointsParent.GetChild(spawnPointIndex).transform.position;
            NavMeshHit hit;
            bool result = NavMesh.SamplePosition(spawnPos, out hit, 5.0f, NavMesh.AllAreas);
            if (!result)
            {
                Debug.LogError($"Failed to find position on nav mesh near {spawnPos}!");
                continue;
            }

            GameObject monster = Instantiate(monsterDef.Prefab, 
                                             hit.position,
                                             Quaternion.identity,
                                             _MonstersParent.transform);

            // Initialize the monster's stats.          
            monsterDef.ApplyStatsToMonster(monster.GetComponent<Monster_Base>());


            // If a monster wave has started in the game over state, it means the game over was caused by the player dying while not in a monster attack phase.
            // So if that is the case, make all spawning monsters invincible so we can see them destroy everything on the game over screen!
            // This ensures that the villagers can't kill them all. ;)
            if (_GameManager.GameState == GameStates.GameOver)
                monster.GetComponent<Health>().IsInvincible = true;


            // Subscribe to the monster's OnDeath event and add it to the list of alive monsters.
            monster.GetComponent<Health>().OnDeath += OnDeath;
            _AliveMonsters.Add(monster);

            yield return new WaitForSeconds(2.0f);
        }


        _MonsterSpawnList.Clear();

        _WaveIsDoneSpawning = true;
    }

    private void OnDeath(GameObject sender, GameObject attacker)
    {
        _LastMonsterDeathTime = Time.time;        
        

        // Only increase the combo streak if the monster was killed by the player.
        if (attacker.CompareTag("Player"))
            CurrentComboStreak++;


        MonstersKilled++;
        TotalMonstersKilled++;


        int enemyScoreValue = sender.GetComponent<IMonster>().GetScoreValue();
        int scoreMultiplier = (CurrentComboStreak > 0 ? CurrentComboStreak : 1); // We want a multiplier of one if the combo streak is 0 so the player doesn't get screwed out of some points.
        GameManager.Instance.AddToScore(enemyScoreValue * scoreMultiplier);


        //Debug.Log($"Kill Streak: {CurrentComboStreak}  Monster Base Score: {enemyScoreValue}");


        _AliveMonsters.Remove(sender);        
    }





    // ******************************************************************************************
    // !!!!!WARNING!!!!!
    // ******************************************************************************************
    //
    // The following several methods are currently deprecated, but still here until I
    // finalize how monsters spawn in the game.
    //
    // DO NOT DELETE THEM YET!
    //
    // ******************************************************************************************



    /// <summary>
    /// Generates a shuffled list of prefab indices for all monsters that should spawn in the current wave.
    /// If there are 10 slimes to spawn, the Slime's monster index will be in this list 10 times.
    /// </summary>
    /// <returns>A shuffled list of prefab indices for each monster instance to be spawned in the current wave.</returns>
    private void GenerateWaveSpawnList()
    {
        _MonsterSpawnList.Clear();


        for (int i = 0; i < _MonsterDefinitions.Count; i++)
        {
            MonsterDefinition monsterDef = _MonsterDefinitions[i];

            int monsterCount = GetMonsterCountForWave(monsterDef.Prefab.GetComponent<IMonster>(), i);
            for (int j = 0; j < monsterCount; j++)
            {
                _MonsterSpawnList.Add(i);
                //Debug.Log($"Added monster \"{monsterDef.prefab.name}\" (index {i}) to spawn list!");
            }
        }


        CurrentWaveSize = _MonsterSpawnList.Count;

        Utils.ShuffleList(_MonsterSpawnList);
    }

    private int GetMonsterCountForWave(IMonster monster, int monsterIndex)
    {
        if (monsterIndex >= CurrentWaveNumber)
            return 0;


        int monsterCount = BaseMonsterSpawnAmount + (CurrentWaveNumber - (monsterIndex + 1));

        return monsterCount <= MaxMonstersOfAnyGivenType ? monsterCount : MaxMonstersOfAnyGivenType;
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
    public int MaxComboStreak { get; private set; }
    public int MonstersKilled { get; private set; }
    public int TotalMonstersKilled { get; private set; } // Total monsters killed across all waves so far.
    public int MonstersLeft { get { return CurrentWaveSize - MonstersKilled; } }
    public bool WaveComplete { get { return _WaveIsDoneSpawning && _AliveMonsters.Count == 0; } }
    public int CurrentWaveNumber { get; private set; }
    public int CurrentWaveSize { get; private set; }


}
