using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Projectile_Simple : Projectile_Base
{
    
    protected override void UpdateProjectile()
    {
        base.UpdateProjectile();


        if (_Target)
        {
            transform.position += _CurDirection * _CurSpeed * Time.deltaTime;
        }
    }

    protected override void SetTarget(GameObject target)
    {
        base.SetTarget(target);

        _CurDirection = _TargetPosition - transform.position;
        _CurDirection.Normalize();

        transform.forward = _CurDirection;
    }

}
