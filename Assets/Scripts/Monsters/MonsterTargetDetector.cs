using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterTargetDetector : MonoBehaviour
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
        // Debug.Log("Trigger: " + other.name);


        if (other.tag == "Enemy")
            return;


        if (other.tag == "Player")
        {
            _ParentEnemy.GetComponent<Monster_Base>().SetTarget(other.gameObject);
        }
    }

}
