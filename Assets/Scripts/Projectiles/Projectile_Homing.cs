using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Projectile_Homing : Projectile_Base
{
    [Range(0f, 1f)]
    public float DefaultHomingStrength = 0.5f;


    protected float _CurHomingStrength;


    protected override void UpdateProjectile()
    {
        base.UpdateProjectile();


        if (_Target == null || _Target.GetComponent<Health>().CurrentHealth == 0)
        {
            // Since there is no longer a target, keep flying in the current direction.
            transform.position += _CurDirection * _CurSpeed * Time.deltaTime;
        }
        else
        {
            CalculateAdjustedTargetPosition();

            Vector3 prevDirection = _CurDirection;
            Vector3 targetDirection = GetTargetDirection();


            // First get the horizontal rotation angle.
            float angleH = Utils_Math.CalculateSignedAngle(_CurDirection, targetDirection, Vector3.up);
            
            // Next get the vertical rotation angle (the angle around the local X-axis).
            float angleV = Utils_Math.CalculateSignedAngle(_CurDirection, targetDirection, transform.right);


            //Debug.Log($" 1: AngleH: {angleH}    AngleV: {angleV}    HomingStr: {_CurHomingStrength}");


            // Scale the angles based on homing strength and frame time.
            float scalar = _CurHomingStrength * Time.deltaTime;
            angleH *= scalar;
            angleV *= scalar;

            // Rotate the projectile's transform.
            transform.Rotate(new Vector3(angleV, angleH, 0));
            _CurDirection = transform.forward;


            //Debug.Log("Old Direction: {prevDirection}    New Direction: {_CurDirection}    Target Direction: {targetDirection}");


            // Draw debug rays.
            Debug.DrawRay(transform.position, prevDirection * 5.0f, Color.green, 0.05f);
            Debug.DrawRay(transform.position, _CurDirection * 5.0f, Color.yellow, 0.05f);
            Debug.DrawRay(transform.position, targetDirection * 5.0f, Color.red, 0.05f);

            // Update the projectile's position;
            transform.position += _CurDirection * _CurSpeed * Time.deltaTime;
        }
    }

    public override void ResetProjectile(Transform parent, Vector3 spawnPosition, GameObject target)
    {
        _CurHomingStrength = DefaultHomingStrength * 5;

        base.ResetProjectile(parent, spawnPosition, target);
    }

}
