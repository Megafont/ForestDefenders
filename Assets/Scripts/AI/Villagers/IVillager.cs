using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IVillager
{
    bool SetTarget(GameObject target);

    public GameObject gameObject { get; }
    public Transform transform { get; }

    Health HealthComponent { get; }

    bool IsAttacking { get; }

    string VillagerType { get; }

}