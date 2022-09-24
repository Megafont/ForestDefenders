using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterTargetDetector : MonoBehaviour
{
    public float DetectionRadius = 5.0f;

    private GameObject _Parent;
    private SphereCollider _Collider;



    // Start is called before the first frame update
    void Start()
    {
        _Collider = GetComponent<SphereCollider>();
        _Collider.radius = DetectionRadius;

        _Parent = transform.parent.gameObject;
    }


    public void Enable(bool enabledState)
    {
        _Collider.enabled = enabledState;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Trigger: " + other.name);


        if (other.tag == "Monster")
            return;


        if (other.tag == "Player" || other.tag == "Villager")
        {

            _Parent.GetComponent<AI_WithAttackBehavior>().SetTarget(other.gameObject);
        }
    }

}
