using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class BridgeConstructionZone : MonoBehaviour
{
    [Tooltip("The Y-axis rotation (in degrees) that bridges will always be locked to when built in this construction zone.")]
    [Range(0f, 359f)]
    public float LockRotation = 0f;


    // How far the player must move the construction ghost to cause it to unlock from the bridge constuction zone.
    const float UNLOCK_DISTANCE = 1f;



    public void ApplyConstraints(Transform gameObject)
    {
        Vector3 position = gameObject.transform.position;
        Vector3 rotation = Vector3.zero;


        if (LockRotation == 0f || LockRotation == 180f)
        {
            position.x = transform.position.x;
            rotation.y = LockRotation;
        }
        else if (LockRotation == 90f || LockRotation == 270f)
        {
            position.z = transform.position.z;
            rotation.y = LockRotation;
        }


        // If the player has tried to move the construction ghost by more than UnlockDistance, then allow
        // it to unlock from the bridge construction zone.
        if (Vector3.Distance(gameObject.position, position) > UNLOCK_DISTANCE)
            return;


        // Apply the bridge constraints of this bridge construction zone to the transform of the building
        // construction ghost to lock it in this zone.
        gameObject.transform.position = position;
        gameObject.transform.rotation = Quaternion.Euler(rotation);
    }

}
