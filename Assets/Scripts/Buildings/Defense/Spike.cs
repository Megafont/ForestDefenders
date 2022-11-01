using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Spike : MonoBehaviour
{
    public int AttackPower = 10;

    public float KnockbackDistance = 2.0f;
    public float KnockbackDuration = 0.15f;

    
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"COLLIDED WITH {other.name}!");

        Health health = other.GetComponent<Health>();
        if (health == null)
            return;


        //Debug.Log($"IMPALED {other.name}!");


        health.TakeDamage(AttackPower, gameObject);

        // Get the knockback direction.
        Quaternion rotation = transform.parent.rotation;
        Vector3 angles = rotation.eulerAngles;
        angles.z = 0; // Remove 90 degree rotation on the z-axis that is there from the export from Blender to Unity process.
        rotation.eulerAngles = angles;

        Vector3 knockbackDirection = rotation * new Vector3(-1, 0, 0);

        StartCoroutine(Utils_Knockback.ApplyKnockbackMotion(other.gameObject, knockbackDirection, KnockbackDistance, KnockbackDuration));

    }


}
