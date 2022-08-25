using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDetector : MonoBehaviour
{
    public float DetectionRadius = 5.0f;

    private GameObject _ParentEnemy;
    private SphereCollider _Collider;


    // Start is called before the first frame update
    void Start()
    {
        _Collider = GetComponent<SphereCollider>();
        _Collider.radius = DetectionRadius;

        _ParentEnemy = transform.parent.gameObject;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
            return;

        //Debug.Log("Trigger: " + other.name);

        if (other.tag == "Player")
        {
            _ParentEnemy.GetComponent<TestEnemy>().SetTarget(other.gameObject);
        }
    }

}
