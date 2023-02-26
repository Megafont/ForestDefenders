using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class MonsterDefinition
{
    // A keyword to search for in the prefab names to find the prefab for this monster.
    public string NameKeyword;
    public GameObject Prefab;

    public int Tier;
    public float MaxHealth;
    public float AttackPower;
    public float AttackCheckFrequency;
    public int ScoreValue;
    public float MaxChaseDistance;
    public float TargetCheckFrequency;
    public float TargetCheckRadius;


    public void ApplyStatsToMonster(Monster_Base monster)
    {
        monster.AttackPower = AttackPower;
        monster.AttackCheckFrequency = AttackCheckFrequency;

        monster.ScoreValue = ScoreValue;
        monster.MaxChaseDistance = MaxChaseDistance;

        monster.HealthComponent.MaxHealth = MaxHealth;
        monster.HealthComponent.ResetHealthToMax();

        monster.TargetCheckFrequency = TargetCheckFrequency;
        monster.TargetCheckRadius = TargetCheckRadius;
    }

}
