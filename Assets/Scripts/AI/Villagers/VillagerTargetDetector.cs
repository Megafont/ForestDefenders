using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class VillagerTargetDetector : MonoBehaviour
{
    public float DetectionRadius = 5.0f;

    private Villager_Base _Parent;
    private SphereCollider _Collider;



    // Start is called before the first frame update
    void Start()
    {
        _Collider = GetComponent<SphereCollider>();
        _Collider.radius = DetectionRadius;

        _Parent = transform.parent.gameObject.GetComponent<Villager_Base>();
    }


    public void Enable(bool enabledState)
    {
        if (_Collider)
            _Collider.enabled = enabledState;    
    }

    void OnTriggerStay(Collider other)
    {   
        //Debug.Log("Trigger: " + other.name);


        if (other.CompareTag("Monster"))
        {
            bool result = _Parent.SetTarget(other.gameObject);

            Enable(!result);
        }

    }

}
