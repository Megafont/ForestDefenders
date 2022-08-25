using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class PlayerBase : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Health>().OnDeath += OnDeath;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDeath(GameObject sender)
    {
        Destroy(this.gameObject);
    }

}
