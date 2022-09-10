using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// This class is a test destructable building for testing the enemies ability to target and destroy a player structure.
/// </summary>
public class TestDestructableBuilding : MonoBehaviour
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
