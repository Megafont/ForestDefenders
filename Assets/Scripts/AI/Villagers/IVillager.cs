using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IVillager
{
    void ClearTargets();
    GameObject GetTarget();
    bool SetTarget(GameObject target, bool discardTarget = false);

    public GameObject gameObject { get; }
    public Transform transform { get; }

    Health HealthComponent { get; }

    bool IsAttacking { get; }

    string VillagerTypeName { get; }



    bool TargetIsBuilding { get; }
    bool TargetIsMonster { get; }
    bool TargetIsResourceNode { get; }
    bool TargetIsVillager { get; }
}