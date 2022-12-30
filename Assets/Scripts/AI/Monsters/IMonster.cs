using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IMonster
{
    void ClearTargets();
    GameObject GetTarget();
    bool SetTarget(GameObject target, bool discardTarget = false);


    public GameObject gameObject { get; }
    public Transform transform { get; }


    public Health HealthComponent { get; }


    bool IsAttacking { get; }

    float GetDangerValue();
    int GetScoreValue();


    float GetAttackPower();
    int GetTier();



    bool TargetIsBuilding { get; }
    bool TargetIsMonster { get; }
    bool TargetIsResourceNode { get; }
    bool TargetIsVillager { get; }
}
