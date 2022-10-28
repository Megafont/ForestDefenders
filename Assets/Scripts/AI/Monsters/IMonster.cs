using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IMonster
{
    bool SetTarget(GameObject target);


    public GameObject gameObject { get; }
    public Transform transform { get; }


    public Health HealthComponent { get; }


    int GetScoreValue();
}
