using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IMonster
{
    void ClearTargets();
    GameObject GetTarget();
    bool SetTarget(GameObject target, bool discardTarget = false);


    GameObject gameObject { get; }
    Transform transform { get; }


    Health HealthComponent { get; }


    bool IsAttacking { get; }

    float GetDangerValue();
    int GetScoreValue();


    float GetAttackPower();
    int GetTier();


    Material IcyMaterial { get; }
    bool HasStatusEffect { get; }
    StatusEffectsFlags StatusEffectsFlags { get; set; }


    bool TargetIsBuilding { get; }
    bool TargetIsMonster { get; }
    bool TargetIsResourceNode { get; }
    bool TargetIsVillager { get; }
}
