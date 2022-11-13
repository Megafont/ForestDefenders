using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IProjectile
{
    event Projectile_Base.Projectile_OnCollidedHandler OnCollided;
    event Projectile_Base.Projectile_OnDestroyedHandler OnDestroyed;


    public GameObject gameObject { get; }

    void ResetProjectile(Transform parent, Vector3 spawnPosition, GameObject target);


}
