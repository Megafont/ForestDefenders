using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace Test
{
    public class WaterDeathTrigger : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void OnTriggerEnter(Collider other)
        {
            // Did the player fall into the water?
            if (other.CompareTag("Player"))
                GameManager.Instance.PlayerFellInWater();


            Health health = other.GetComponent<Health>();
            if (health)
                health.DealDamage(health.CurrentHealth, DamageTypes.Drowning, null);
        }

    }


}
