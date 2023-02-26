using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;



public class Building_Turret : Building_Base
{
    [Tooltip("How close the turrent must be facing to it's target (in degrees) before it will fire.")]
    [Range(0f, 180f)]
    [SerializeField] private float _AimThreshold = 5f;

    [Tooltip("How far away the turret can see and attack enemies.")]
    [Range(0f, 50f)]
    [SerializeField] private float _AttackRange = 10f;

    [Tooltip("How often the turret fires (in seconds) when it is facing it's target.")]
    [SerializeField] private float _ProjectileFrequency = 1.5f;

    [Tooltip("A marker that specifies the position where projectiles will spawn.")]
    [SerializeField] private GameObject _ProjectileSpawnPoint;

    [Tooltip("The rotation speed of the turret in degrees per second.")]
    [SerializeField] private float _RotationSpeed = 20f;



    private GameManager _GameManager;

    private IMonster _Target;
    private GameObject _TurretTop;

    private bool _IsLockedOnTarget;
    private float _LastFireTime;

    private Vector3 _TurretDirection;

    private static ObjectPool<IProjectile> _ProjectilePool;
    private static GameObject _TurretProjectilePrefab;



    protected override void InitBuilding()
    {
        ConfigureBasicBuildingSetup("Defense", "Turret");

        _GameManager = GameManager.Instance;

        _TurretDirection = transform.forward;
        _TurretTop = transform.Find("Turret Top").gameObject;



        if (_ProjectilePool == null)
            _ProjectilePool = new ObjectPool<IProjectile>(CreateProjectile, OnTakenFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 128, 1024);


        if (_TurretProjectilePrefab == null)
        {
            string projectilePrefabPath = "Projectiles/Projectile - Turret";
            _TurretProjectilePrefab = Resources.Load<GameObject>(projectilePrefabPath);

            if (_TurretProjectilePrefab == null)
                throw new Exception($"Turret failed to load projectile prefab \"{projectilePrefabPath}\"!");
        }



        StartCoroutine(AimTurret());
    }

    protected override void UpdateBuilding()
    {
        base.UpdateBuilding();


        if (_Target != null)
        {
            // Check if the target has died.
            if (_Target.HealthComponent.CurrentHealth == 0)
            {
                _Target = null;
                _IsLockedOnTarget = false;
            }
            else if (_IsLockedOnTarget &&
                     (Time.time - _LastFireTime >= _ProjectileFrequency))
            {
                FireProjectile();
            }
        }


        if (_Target == null)
            FindTarget();            

    }

    private IEnumerator AimTurret()
    {
        Vector3 turretPos = new Vector3(transform.position.x, 0f, transform.position.z);


        while (true)
        {
            Vector3 enemyPos;
            
            if (_Target != null && _GameManager.GameState == GameStates.MonsterAttackPhase)
                enemyPos = new Vector3(_Target.transform.position.x, 0f, _Target.transform.position.z);
            else
                enemyPos = new Vector3(0f, 0f, 1000f); // There are no enemies outside of the enemy attack phase, so set a fake enemy position to the north.
            

            Vector3 targetDirection = Vector3.Normalize(enemyPos - transform.position);

            // First get the horizontal rotation angle.
            float angleH = Utils_Math.CalculateSignedAngle(_TurretDirection, targetDirection, Vector3.up);
            float rotAmount = _RotationSpeed * Time.deltaTime;

            if (angleH < 0)
                rotAmount *= -1;

            _TurretTop.transform.Rotate(new Vector3(0, rotAmount, 0));
            _TurretDirection = _TurretTop.transform.forward;

            _IsLockedOnTarget = Mathf.Abs(angleH) <= _AimThreshold;

            //Debug.Log($"Turret Aiming:    TurrentDirection: {_TurretDirection}    angleH: {angleH}    rotAmount: {rotAmount}");

            yield return null;
        }


        //_IsLockedOnTarget = false;
    }

    private void FindTarget()
    {
        if (_Target == null)
        {
            GameObject monster = Utils_AI.FindNearestObjectOfType(gameObject, typeof(Monster_Base));
            if (monster != null &&
                Vector3.Distance(transform.parent.transform.position, monster.transform.position) <= _AttackRange &&
                monster.GetComponent<Health>().CurrentHealth > 0)
            {
                _Target = monster.GetComponent<IMonster>();

                //Debug.Log($"Turret is targeting monster {_Target.gameObject.name} at ({_Target.transform.position}).");
            }
        }

    }



    private void FireProjectile()
    {
        _LastFireTime = Time.time;


        IProjectile newProjectile = _ProjectilePool.Get();


        newProjectile.ResetProjectile(transform,
                                      _ProjectileSpawnPoint.transform.position,
                                      _Target.gameObject);

        newProjectile.OnDestroyed += OnProjectileDestroyed;
    }

    private static void OnProjectileDestroyed(IProjectile sender)
    {
        //Debug.Log("Projectile destroyed!");

        _ProjectilePool.Release(sender);

        //Debug.Log($"D - Turret Projectile Pool Counts:    Total: {_ProjectilePool.CountAll}    Active: {_ProjectilePool.CountActive}    Inactive: {_ProjectilePool.CountInactive}");
    }



    // Projectile Pool Methods
    // ====================================================================================================

    private static IProjectile CreateProjectile()
    {
        // Instantiate a new projectile underneath the world so the player can't see it spawn in.
        GameObject newProjectile = Instantiate(_TurretProjectilePrefab, new Vector3(0, -128, 0), Quaternion.identity);

        IProjectile newProjectileComponent = newProjectile.GetComponent<IProjectile>();
        newProjectileComponent.OnDestroyed += OnProjectileDestroyed;

        return newProjectileComponent;
    }


    private static void OnReturnedToPool(IProjectile projectile)
    {
        //Debug.Log($"R - Turret Projectile Pool Counts:    Total: {_ProjectilePool.CountAll}    Active: {_ProjectilePool.CountActive}    Inactive: {_ProjectilePool.CountInactive}");
    }

    private static void OnTakenFromPool(IProjectile projectile)
    {
        //Debug.Log($"T - Turret Projectile Pool Counts:    Total: {_ProjectilePool.CountAll}    Active: {_ProjectilePool.CountActive}    Inactive: {_ProjectilePool.CountInactive}");
    }

    private static void OnDestroyPoolObject(IProjectile projectile)
    {
        Destroy(projectile.gameObject);

        //Debug.Log("Destroyed turret pool projectile!");
    }

    // ====================================================================================================


}
