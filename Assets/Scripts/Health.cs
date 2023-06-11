using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;


public class Health : MonoBehaviour
{
    private const float _HealthPopupTextSize = 20f;



    [SerializeField] private float _MaxHealth = 100;
    [SerializeField] private bool _EnableDamageFlash = true;
    [SerializeField] private Color _DamageFlashColor = Color.red;
    [SerializeField] private float _DamageFlashTime = 0.1f;

    [Tooltip("The damage types this entity is resistant to.")]
    [SerializeField] private DamageTypes _IsResistantTo;
    [Tooltip("The damage types this entity is vulnerable to.")]
    [SerializeField] private DamageTypes _IsVulnerableTo;

    [SerializeField] private bool _IsInvincible = false;



    private GameManager _GameManager;
    private MonsterManager _MonsterManager;

    private float _LastDamageTime;

    private SoundParams _SoundParams;
    private SoundSetPlayer _SoundSetPlayer;


    // These hold references to all child objects with MeshRenderers or SkinnedMeshRenderers so the entire object can be made to flash when damage is taken.
    private List<MeshRenderer> _MeshRenderers;
    private List<SkinnedMeshRenderer> _SkinnedMeshRenderers;



    public delegate void Health_OnDeathEventHandler(GameObject sender, GameObject attacker);
    public delegate void Health_OnDamagedEventHandler(GameObject sender, GameObject attacker, float amount, DamageTypes damageType);
    public delegate void Health_OnHealedEventHandler(GameObject sender, GameObject healer, float amount);
    public delegate void Health_OnHealthChangedEventHandler(GameObject sender, GameObject changeSource, float changeAmount);

    public event Health_OnDeathEventHandler OnDeath;
    public event Health_OnHealedEventHandler OnHeal;
    public event Health_OnDamagedEventHandler OnTakeDamage;
    public event Health_OnHealthChangedEventHandler OnHealthChanged;



    private void Awake()
    {
        _GameManager = GameManager.Instance;
        _MonsterManager = _GameManager.MonsterManager;

        _SoundParams = _GameManager.SoundParams;
        _SoundSetPlayer = gameObject.AddComponent<SoundSetPlayer>();

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

    public void DealDamage(float amount, DamageTypes damageType, GameObject attacker)
    {
        if (amount <= 0)
            throw new Exception("The damage amount must be positive!");


        // If this entity is already dead, then do nothing. This way the code below won't spam the OnDeath event when an entity is dead.
        if (CurrentHealth <= 0 || _IsInvincible)
            return;



        amount = Mathf.RoundToInt(amount);
        if (amount == 0)
            return;


        PlayAttackHitSound();


        if (_EnableDamageFlash &&
            Time.time - _LastDamageTime > _DamageFlashTime)
        {
            _LastDamageTime = Time.time;
            

            // Only do the damage flash effect if the AI is enabled.
            if (gameObject.activeSelf)
                StartCoroutine(DoDamageFlash());
        }


        // If the damage type doesn't allow random variance, then directly apply the damage amount.
        // Otherwise give it a random variance.
        float changeAmount = 0.0f;
        if (damageType == DamageTypes.Drowning)
        {
            changeAmount = amount;
        }
        else
        {
            float randomVariance = Random.Range(0.0f, _GameManager.AttackDamageVariance);
            changeAmount = Mathf.RoundToInt(amount - (amount * randomVariance));
            changeAmount = Mathf.Max(changeAmount, 1.0f); // Enforce that the attack will always at least 1 point of damage.
        }


        float buffAmount = 0;
        if (GetComponent<Monster_Base>() != null)
        {
            if ((_IsResistantTo & damageType) > 0)
                buffAmount = (amount * _MonsterManager.DamageResistanceBuffAmount) * -1;
            else if ((_IsVulnerableTo & damageType) > 0)
                buffAmount = amount * _MonsterManager.DamageVulnerabilityBuffAmount;
        }

        changeAmount += buffAmount;
        //Debug.Log($"Amount: {amount}    Buff: {buffAmount}");


        CurrentHealth -= changeAmount;
        if (CurrentHealth < 0)
            changeAmount = changeAmount + CurrentHealth; // Calculate how many damage points it took to kill this entity. So if it had 10 health and took 12 damage, this will return 10.

        OnHealthChanged?.Invoke(gameObject, attacker, -changeAmount);
        OnTakeDamage?.Invoke(gameObject, attacker, changeAmount, damageType);

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            OnDeath?.Invoke(gameObject, attacker);
        }


        SpawnHealthPopup(-changeAmount, buffAmount);
    }

    public void Heal(float amount, GameObject healer)
    {
        if (amount <= 0)
            throw new Exception("The damage amount must be positive!");


        CurrentHealth += amount;

        float changeAmount = amount;
        if (CurrentHealth > MaxHealth)
        {
            changeAmount = CurrentHealth - MaxHealth;
            CurrentHealth = MaxHealth;
        }
            

        OnHealthChanged?.Invoke(gameObject, healer, changeAmount);
        OnHeal?.Invoke(gameObject, healer, changeAmount);

        SpawnHealthPopup(changeAmount);
    }

    /// <summary>
    /// Resets health to max without firing any health changed events.
    /// </summary>
    public void ResetHealthToMax()
    {
        float changeAmount = MaxHealth - CurrentHealth;
        CurrentHealth = MaxHealth;

        OnHealthChanged?.Invoke(gameObject, null, changeAmount);
        OnHeal?.Invoke(gameObject, null, changeAmount);
    }

    private void PlayAttackHitSound()
    {
        // Play sound effect.
        if (gameObject.CompareTag("Building"))
        {
            _SoundSetPlayer.SoundSet = _SoundParams.GetSoundSet("Sound Set - Attack Impacts On Buildings");
            _SoundSetPlayer.PlayRandomSound();
        }
        else // The object getting hit is a character (player, monster, or villager).
        {
            _SoundSetPlayer.SoundSet = _SoundParams.GetSoundSet("Sound Set - Attack Impacts On Characters");
            _SoundSetPlayer.PlayRandomSound();
        }
    }

    private IEnumerator DoDamageFlash()
    {
        foreach (MeshRenderer renderer in _MeshRenderers)
            renderer.material.color = _DamageFlashColor;
        foreach (SkinnedMeshRenderer renderer in _SkinnedMeshRenderers)
            renderer.material.color = _DamageFlashColor;

        yield return new WaitForSeconds(_DamageFlashTime);

        foreach (MeshRenderer renderer in _MeshRenderers)
            renderer.material.color = Color.white;
        foreach (SkinnedMeshRenderer renderer in _SkinnedMeshRenderers)
            renderer.material.color = Color.white;
    }

    private void SpawnHealthPopup(float healthChangedAmount, float buffAmount = 0)
    {
        Color textColor = Color.white;
        string text = "";
        if (healthChangedAmount >= 0)
        {
            text = $"+{healthChangedAmount}";
            textColor = TextPopupColors.HealColor;
        }
        else
        {
            text = $"{healthChangedAmount}";

            if (buffAmount < 0)
                textColor = TextPopupColors.DamageResistanceColor;
            else if (buffAmount > 0)
                textColor = TextPopupColors.DamageVulnerableColor;
            else
                textColor = TextPopupColors.DamageNormalColor;
        }


        TextPopup.ShowTextPopup(TextPopup.AdjustStartPosition(gameObject), 
                                text, 
                                textColor, 
                                _HealthPopupTextSize);
    }

    private void FindRenderers()
    {
        _MeshRenderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
        _SkinnedMeshRenderers = new List<SkinnedMeshRenderer>(GetComponentsInChildren<SkinnedMeshRenderer>());        
    }

    public void SetMaxHealth(float amount)
    {
        _MaxHealth = amount;
        TotalMaxHealthIncrease = 0;
    }

    public void IncreaseMaxHealth(float amount)
    {
        _MaxHealth += amount;
        TotalMaxHealthIncrease += amount;
    }



    public float CurrentHealth { get; private set; }
    
    public float MaxHealth 
    { 
        get { return _MaxHealth; } 
        private set { _MaxHealth = value; } 
    }

    public bool IsAlive
    {
        get { return CurrentHealth > 0; }
    }

    public bool IsInvincible
    {
        get { return _IsInvincible; }
        set { _IsInvincible = value; }
    }

    public float TotalMaxHealthIncrease { get; private set; }
}
