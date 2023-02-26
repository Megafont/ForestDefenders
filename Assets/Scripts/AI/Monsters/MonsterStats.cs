using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public static class MonsterStats
{
    private static List<Monster_Base> _MonsterPrefabsList;



    static MonsterStats()
    {
        MonsterDefinitions = new List<MonsterDefinition>();
    }

    public static void InitMonsterDefinitions(List<Monster_Base> monsterPrefabsList)
    {
        if (monsterPrefabsList == null)
            throw new Exception("The passed in monster prefabs list is null!");
        if (monsterPrefabsList.Count == 0)
            throw new Exception("The passed in monster prefabs list is empty!");

        _MonsterPrefabsList = monsterPrefabsList;



        CreateMonsterDefinition("Slime",        1, 10,   1,  1.00f,  100, 10, 2, 5);
        CreateMonsterDefinition("TurtleShell",  1, 20,   2,  0.95f,  200, 10, 2, 5);
        CreateMonsterDefinition("Bat",          1, 30,   3,  0.90f,  300, 10, 2, 5);
        CreateMonsterDefinition("Crab",         1, 40,   4,  0.85f,  400, 10, 2, 5);

        CreateMonsterDefinition("Plant",        2, 50,   5,  0.80f,  500, 20, 2, 5);
        CreateMonsterDefinition("Skeleton",     2, 60,   6,  0.75f,  600, 20, 2, 5);
        CreateMonsterDefinition("Spider",       2, 70,   7,  0.70f,  700, 20, 2, 5);
        CreateMonsterDefinition("Beholder",     2, 80,   8,  0.65f,  800, 20, 2, 5);

        CreateMonsterDefinition("Rat",          3, 100, 10,  0.60f, 1000, 30, 2, 5);
        CreateMonsterDefinition("Werewolf",     3, 115, 12,  0.55f, 1150, 30, 2, 5);
        CreateMonsterDefinition("Orc",          3, 130, 14,  0.50f, 1300, 30, 2, 5);
        CreateMonsterDefinition("Lizard",       3, 150, 16,  0.45f, 1500, 30, 2, 5);

        CreateMonsterDefinition("Golem",        4, 200, 20,  0.40f, 2000, 40, 2, 5);
        CreateMonsterDefinition("Specter",      4, 215, 23,  0.35f, 2150, 40, 2, 5);
        CreateMonsterDefinition("BlackKnight",  4, 230, 26,  0.30f, 2300, 40, 2, 5);
        CreateMonsterDefinition("FlyingDemon",  4, 250, 30,  0.25f, 2500, 40, 2, 5);

    }


    private static void CreateMonsterDefinition(string nameKeyword, int tier, float maxHealth, float attackPower, float attackCheckFrequency, int scoreValue, float maxChaseDistance, float targetCheckFreq, float targetCheckRadius)
    {
        GameObject prefab = FindPrefabWithNameKeyword(nameKeyword);
        if (prefab == null)
            throw new Exception($"Failed to find a monster prefab whose name contains the keyword \"{nameKeyword}\"!");


        MonsterDefinition monsterDef = new MonsterDefinition()
        {
            NameKeyword = nameKeyword,
            Prefab = prefab,

            Tier = tier,
            MaxHealth = maxHealth,
            AttackPower = attackPower,
            AttackCheckFrequency = attackCheckFrequency,
            ScoreValue = scoreValue,
            MaxChaseDistance = maxChaseDistance,
            TargetCheckFrequency = targetCheckFreq,
            TargetCheckRadius = targetCheckRadius,
        };


        MonsterDefinitions.Add(monsterDef);
    }

    private static GameObject FindPrefabWithNameKeyword(string nameKeyword)
    {
        foreach (Monster_Base monsterPrefab in _MonsterPrefabsList) 
        {
            if (monsterPrefab.name.Contains(nameKeyword))
                return monsterPrefab.gameObject;
        }

        return null;
    }



    public static List<MonsterDefinition> MonsterDefinitions { get; private set; }


}
