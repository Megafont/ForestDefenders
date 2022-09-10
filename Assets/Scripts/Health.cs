using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int MaxHealth = 100;

    public float CurrentHealth { get; private set; }


    public delegate void DeathHandler(GameObject sender);
    public event DeathHandler OnDeath;


    // Start is called before the first frame update
    void Start()
    {
        CurrentHealth = MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0)
            throw new Exception("The damage amount must be positive!");


        // If this entity is already dead, then do nothing. This way the code below won't spam the OnDeath event when an entity is dead.
        if (CurrentHealth <= 0)
            return;


        CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            OnDeath?.Invoke(gameObject);
        }

    }

}
