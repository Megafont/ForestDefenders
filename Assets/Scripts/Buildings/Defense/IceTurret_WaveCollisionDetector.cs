using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class IceTurret_WaveCollisionDetector : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"Ice ring hit GameObject: {other.name}");

        StartCoroutine(StatusEffect_Ice.StartStatusEffect(other.gameObject));
    }

}
