using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(Light))]
public abstract class Projectile_Base : MonoBehaviour, IProjectile
{
    public float DefaultAttackPower = 10.0f;
    public float DefaultMaxLifespan = 60.0f;
    public float DefaultSpeed = 5.0f;

    public DamageTypes DamageType = DamageTypes.Physical;

    public bool DieOnImpact = true;

    public LayerMask TargetLayers = ~0; // Set the layer mask to have all layers enabled.


    protected Vector3 _CurDirection;
    protected GameObject _Target;
    protected Vector3 _TargetPosition;

    protected float _CurAttackPower;
    protected float _CurMaxLifeSpan;
    protected float _CurSpeed;

    protected float _ProjectileStartTime;

    protected Vector3 _PrevPosition;

    protected Light _Light;

    public delegate void Projectile_OnCollidedHandler(IProjectile sender, GameObject objectHit);
    public delegate void Projectile_OnDestroyedHandler(IProjectile sender);

    public event Projectile_OnCollidedHandler OnCollided;
    public event Projectile_OnDestroyedHandler OnDestroyed;



    void Awake()
    {
        _Light = GetComponent<Light>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _PrevPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateProjectile();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"Projectile hit GameObject \"{other.name}\"!");

        OnCollision(other);
    }

    protected virtual void UpdateProjectile()
    {
        // Draw a debug trail behind the projectile.
        Debug.DrawLine(_PrevPosition, transform.position, Color.cyan, 15);
        _PrevPosition = transform.position;

        // Check if the projectile has lived out its maximum life span.
        if (Time.time - _ProjectileStartTime >= DefaultMaxLifespan)
            OnDeath();
    }

    protected virtual void OnCollision(Collider objectHit)
    {
        //Debug.Log($"Time Since Launch: {Time.time - _ProjectileStartTime}    hitParent: {objectHit.gameObject == transform.parent.gameObject}    hitChildOfParent: {objectHit.transform.IsChildOf(transform.parent)}");
        // If the projectile hit part of it's parent (the object that fired it) right after launch, then ignore the collision!
        if (Time.time - _ProjectileStartTime <= 1.0f && // The number being compared to here is the time in seconds after launch during which the projectile will ignore collisions with it's parent.
            (objectHit.gameObject == transform.parent.gameObject || objectHit.transform.IsChildOf(transform.parent)))
        {
            //Debug.Log("Ignoring collision!");
            return;
        }


        gameObject.SetActive(false);


        // If the object we collided with has a health component and it is on one of the target
        // layers for this projectile, then deal damage to it.
        Health targetHealth = objectHit.GetComponent<Health>();
        if (targetHealth &&
            Utils.LayerMaskContains(TargetLayers.value, objectHit.gameObject.layer))
        {
            targetHealth.DealDamage(_CurAttackPower, DamageType, gameObject);
        }


        // Invoke the OnCollided event.
        OnCollided?.Invoke(this, objectHit.gameObject);


        if (DieOnImpact)
            OnDeath();
    }

    protected virtual void OnDeath()
    {
        // Unparent this projectile since it is no longer in use for now.
        transform.parent = null;


        // Invoke the OnDestroyed event.
        OnDestroyed?.Invoke(this);

        gameObject.SetActive(false);
    }

    public virtual void ResetProjectile(Transform parent, Vector3 spawnPosition, GameObject target)
    {
        if (parent == null)
            throw new Exception("The projectile's parent cannot be null!");

        if (DefaultSpeed <= 0)
            throw new Exception("The projectile's speed cannot be set to 0 or a negative number!");

        if (target == null)
            Debug.LogError(GetType() + " cannot have a target of null!");


        transform.parent = parent.transform;

        transform.position = spawnPosition;
        _PrevPosition = transform.position;

        SetTarget(target);


        _CurAttackPower = DefaultAttackPower;
        _CurSpeed = DefaultSpeed;
        _CurMaxLifeSpan = DefaultMaxLifespan;

        _ProjectileStartTime = Time.time;


        // Remove any old subscriptions to these events.
        OnCollided = null;
        OnDestroyed = null;

        gameObject.SetActive(true);
    }

    protected virtual void SetTarget(GameObject target)
    {
        if (target == null)
            throw new Exception("The projectile's target cannot be set to null!");

        _CurDirection = GetTargetDirection();
        _Target = target;
        _TargetPosition = Utils_Math.CalculateAdjustedTargetPosition(_Target);
    }

    protected virtual Vector3 GetTargetDirection()
    {
        return Vector3.Normalize(_TargetPosition - transform.position);
    }

}
