using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IMonster
{
    void ClearTargets();
    bool SetTarget(GameObject target);


    public GameObject gameObject { get; }
    public Transform transform { get; }


    public Health HealthComponent { get; }


    bool IsAttacking { get; }

    int GetDangerValue();
    int GetScoreValue();


    int GetAttackPower();
    int GetTier();
    
}
