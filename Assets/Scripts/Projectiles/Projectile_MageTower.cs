using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Projectile_MageTower : Projectile_Homing
{
    [Range(1f, 10f)]
    [Tooltip("The mage tower projectile will detonate when it gets within this distance of the target.")]
    public float DetonationRange = 5.0f;


    private ParticleSystem _ExplosionParticleSystem;

    private bool _HasDetonated = false;

    private WaitForSeconds _ExplosionDamageDelay;



    void Start()
    {
        _ExplosionParticleSystem = transform.GetChild(0).GetComponent<ParticleSystem>();      

        _ExplosionDamageDelay = new WaitForSeconds(0.5f);
    }


    protected override void UpdateProjectile()
    {
        base.UpdateProjectile();

        //Debug.Log($"Pos: {_TargetPosition}    Target: {_TargetPosition}    Dist: {Vector3.Distance(transform.position, _TargetPosition)}");
        if (_Target && !_HasDetonated && Vector3.Distance(transform.position, _TargetPosition) <= DetonationRange )
        {
            _HasDetonated = true;

            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<SphereCollider>().enabled = false;

            _CurHomingStrength = 0;

            // Play the explosion particle effect.
            _ExplosionParticleSystem.Play();

            // Start the coroutine to deal damage to monsters.
            StartCoroutine(DamageEnemiesInRange());
        }
        
        if (_HasDetonated && !_ExplosionParticleSystem.isPlaying)
        {
            // The explosion effect is done, so kill this particle.
            OnDeath();
        }
    }

    protected virtual IEnumerator DamageEnemiesInRange()
    {
        yield return _ExplosionDamageDelay;


        RaycastHit[] enemies = Physics.SphereCastAll(transform.position, _ExplosionParticleSystem.shape.radius + 2.0f, transform.forward, 2.0f, TargetLayers.value);
        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject obj = enemies[i].collider.gameObject;
            enemies[i].collider.gameObject.GetComponent<Health>().DealDamage(DefaultAttackPower, DamageType, this.gameObject);
        }
    }


    public override void ResetProjectile(Transform parent, Vector3 spawnPosition, GameObject target)
    {
        _HasDetonated = false;

        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<SphereCollider>().enabled = true;

        _CurHomingStrength = DefaultHomingStrength;

        base.ResetProjectile(parent, spawnPosition, target);
    }

}
