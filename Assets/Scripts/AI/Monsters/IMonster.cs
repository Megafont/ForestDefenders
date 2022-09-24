using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IMonster
{
    public GameObject gameObject { get; }

    public Health HealthComponent { get; }


    int GetScoreValue();
}
