using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class MonsterTargetDetector : MonoBehaviour
{
    [SerializeField] private float _DetectionRadius = 5.0f;



    private IMonster _Parent;
    private SphereCollider _Collider;



    // Start is called before the first frame update
    void Start()
    {
        _Collider = GetComponent<SphereCollider>();
        _Collider.radius = _DetectionRadius;

        _Parent = transform.parent.gameObject.GetComponent<Monster_Base>();
    }


    public void Enable(bool enabledState)
    {
        if (_Collider)
            _Collider.enabled = enabledState;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Villager"))
        {
            _Parent.SetTarget(other.gameObject);
        }
    }

}
