using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Health : MonoBehaviour
{
    public int MaxHealth = 100;

    public bool EnableDamageFlash = true;
    public Color DamageFlashColor = Color.red;
    public float DamageFlashTime = 0.1f;

    private float _LastDamageTime;



    // These hold references to all child objects with MeshRenderers or SkinnedMeshRenderers so the entire object can be made to flash when damage is taken.
    private List<MeshRenderer> _MeshRenderers;
    private List<SkinnedMeshRenderer> _SkinnedMeshRenderers;



    public float CurrentHealth { get; private set; }


    public delegate void DeathHandler(GameObject sender);
    public event DeathHandler OnDeath;



    private void Awake()
    {
        _MeshRenderers = new List<MeshRenderer>();
        _SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();

        CurrentHealth = MaxHealth;

        _LastDamageTime = Time.time;
        FindRenderers();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            throw new Exception("The damage amount must be positive!");


        // If this entity is already dead, then do nothing. This way the code below won't spam the OnDeath event when an entity is dead.
        if (CurrentHealth <= 0)
            return;


        if (EnableDamageFlash &&
            Time.time - _LastDamageTime > DamageFlashTime)
        {
            _LastDamageTime = Time.time;
            StartCoroutine(DoDamageFlash());
        }
            
        CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            OnDeath?.Invoke(gameObject);
        }

    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            throw new Exception("The damage amount must be positive!");


        CurrentHealth += amount;

        if (CurrentHealth >= MaxHealth)
            CurrentHealth = MaxHealth;
    }

    private IEnumerator DoDamageFlash()
    {
        foreach (MeshRenderer renderer in _MeshRenderers)
            renderer.material.color = DamageFlashColor;
        foreach (SkinnedMeshRenderer renderer in _SkinnedMeshRenderers)
            renderer.material.color = DamageFlashColor;

        yield return new WaitForSeconds(DamageFlashTime);

        foreach (MeshRenderer renderer in _MeshRenderers)
            renderer.material.color = Color.white;
        foreach (SkinnedMeshRenderer renderer in _SkinnedMeshRenderers)
            renderer.material.color = Color.white;
    }


    private void FindRenderers()
    {
        _MeshRenderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
        _SkinnedMeshRenderers = new List<SkinnedMeshRenderer>(GetComponentsInChildren<SkinnedMeshRenderer>());        
    }

}
