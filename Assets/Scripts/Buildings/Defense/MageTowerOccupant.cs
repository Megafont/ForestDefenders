using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;


public class MageTowerOccupant : MonoBehaviour
{
    [SerializeField]
    private GameObject _MagicSpawnPoint;

    [SerializeField]
    private float _AttackFrequency = 2.0f;

    [SerializeField]
    private float _AttackRange = 20.0f;



    private Health _Health;

    private float _LastAttackTime;

    private IMonster _Target;


    private static GameObject _MagicProjectilePrefab;

    private static ObjectPool<IProjectile> _ProjectilePool;


    void Awake()
    {
        if (_ProjectilePool == null)
            _ProjectilePool = new ObjectPool<IProjectile>(CreateProjectile, OnTakenFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 128, 1024);


        if (_MagicProjectilePrefab == null)
        {
            string projectilePrefabPath = "Projectiles/Projectile - Mage Tower - Male";
            _MagicProjectilePrefab = Resources.Load<GameObject>(projectilePrefabPath);

            if (_MagicProjectilePrefab == null)
                throw new Exception($"Mage tower occupant failed to load projectile prefab \"{projectilePrefabPath}\"!");
        }


        _Health = GetComponent<Health>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        DoTargetCheck();

        if (Time.time - _LastAttackTime >= _AttackFrequency)
            FireProjectile();
    }

    private void OnDestroy()
    {
        _ProjectilePool.Dispose();
    }



    private void DoTargetCheck()
    {
        if (_Target == null)
        {
            GameObject monster = Utils_World.FindNearestObjectOfType(gameObject, typeof(Monster_Base));
            if (monster != null &&
                Vector3.Distance(transform.parent.transform.position, monster.transform.position) <= _AttackRange &&
                monster.GetComponent<Health>().CurrentHealth > 0)
            {
                _Target = monster.GetComponent<IMonster>();

                //Debug.Log($"Mage Tower is targeting monster {_Target.gameObject.name} at ({_Target.transform.position}).");
            }
        }
        else
        {
            // Check if the target has died.
            if (_Target.HealthComponent.CurrentHealth > 0)
            {
                // Keep the mage always facing his/her target.
                // NOTE: We use the mage's own y coordinate to stop him from tilting down and looking stupid when enemies approach the tower.
                transform.LookAt(new Vector3(_Target.transform.position.x, 
                                             transform.position.y,
                                             _Target.transform.position.z), 
                                 Vector3.up);
            }
            else // Target's health is 0.
            {
                // NOTE: Originally the code in this if statement was in a separate event method that was subscribed to the
                //       OnDeath event of the target's Health component. Unfortunately, in most cases the event never fired. I believe this was because
                //       this subscription happened after the monster script subscribed to the same event. Thus when it fired, the monster's own death
                //       event handler destroyed it before other handlers could run. So that's why it wouldn't fire.


                _Target = null;
            }

        }
    }

    private void FireProjectile()
    {
        _LastAttackTime = Time.time;


        if (_Target == null || _Target.HealthComponent.CurrentHealth == 0)
            return;


        IProjectile newProjectile = _ProjectilePool.Get();


        newProjectile.ResetProjectile(transform.parent, 
                                      _MagicSpawnPoint.transform.position,
                                      _Target.gameObject);
        
        newProjectile.OnDestroyed += OnProjectileDestroyed;
    }

    private static void OnProjectileDestroyed(IProjectile sender)
    {
        //Debug.Log("Projectile destroyed!");

        _ProjectilePool.Release(sender);

        //Debug.Log($"D - Mage Tower Projectile Pool Counts:    Total: {_ProjectilePool.CountAll}    Active: {_ProjectilePool.CountActive}    Inactive: {_ProjectilePool.CountInactive}");
    }




    // Projectile Pool Methods
    // ====================================================================================================

    private static IProjectile CreateProjectile()
    {
        // Instantiate a new projectile underneath the world so the player can't see it spawn in.
        GameObject newProjectile = Instantiate(_MagicProjectilePrefab, new Vector3(0, -128, 0), Quaternion.identity);

        IProjectile newProjectileComponent = newProjectile.GetComponent<IProjectile>();
        newProjectileComponent.OnDestroyed += OnProjectileDestroyed;

        return newProjectileComponent;
    }


    private static void OnReturnedToPool(IProjectile projectile)
    {
        //Debug.Log($"R - Mage Tower Projectile Counts:    Total: {_ProjectilePool.CountAll}    Active: {_ProjectilePool.CountActive}    Inactive: {_ProjectilePool.CountInactive}");
    }

    private static void OnTakenFromPool(IProjectile projectile)
    {
        //Debug.Log($"T - Mage Tower Projectile Counts:    Total: {_ProjectilePool.CountAll}    Active: {_ProjectilePool.CountActive}    Inactive: {_ProjectilePool.CountInactive}");
    }

    private static void OnDestroyPoolObject(IProjectile projectile)
    {
        // Sometimes this is null when the game shuts down.
        if (projectile == null)
            return;
        if (projectile.gameObject == null)
            return;


        Destroy(projectile.gameObject);

        //Debug.Log("Destroyed mage tower pool projectile!");
    }

    // ====================================================================================================



    public Health HealthComponent { get { return _Health; } }


}
