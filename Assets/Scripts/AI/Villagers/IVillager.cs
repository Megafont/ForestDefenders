using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IVillager
{
    public GameObject gameObject { get; }
    public Health HealthComponent { get; }

}
