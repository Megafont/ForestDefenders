using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PlayerHealthBar : MonoBehaviour
{
    [Tooltip("The length of the bar when the player has no upgrades to max health.")]
    [SerializeField] private float _BaseBarLength = 50;

    [Tooltip("The length of the health bar is the current value of the bar times this multiplier.")]
    [Range(0.25f, 10f)]
    [SerializeField] private float _LengthMultiplier = 5f;

    [Range(1f, 10f)]
    [SerializeField] private float _BorderThickness = 3f;

    [SerializeField] private bool _FadeBarColorAsHealthDrops = true;
    [SerializeField] private Color32 _BackgroundColor = Color.gray;
    [SerializeField] private Color32 _FullColor = new Color32(0, 150, 0, 255);
    [SerializeField] private Color32 _MediumColor = new Color32(255, 170, 0, 255);
    [SerializeField] private Color32 _LowColor = Color.red;



    private RectTransform _RectTransform;

    private Image _UI_HealthBarBackgroundImage;
    private Image _UI_HealthBarImage;
    private TMP_Text _UI_HealthBarText;

    private GameManager _GameManager;
    private Health _PlayerHealthComponent;

    private bool _HealthChanged = true; // This is set to true to force an initial update of the health bar.


    // Start is called before the first frame update
    void Start()
    {
        _GameManager = GameManager.Instance;

        GameObject healthBarBackgroundObj = transform.Find("Health Bar Background").gameObject;
        _UI_HealthBarBackgroundImage = healthBarBackgroundObj.GetComponent<Image>();

        GameObject healthBarObj = transform.Find("Health Bar").gameObject;
        _UI_HealthBarImage = healthBarObj.GetComponent<Image>();

        if (GameManager.Instance.PlayerIsInGame())
        {
            _PlayerHealthComponent = _GameManager.Player.GetComponent<Health>();
            _PlayerHealthComponent.OnHealthChanged += OnPlayerHealthChanged;
        }

        GameObject healthBarTextObj = transform.Find("Health Bar Text (TMP)").gameObject;
        _UI_HealthBarText = healthBarTextObj.GetComponent<TMP_Text>();

        _RectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_HealthChanged)
        {
            UpdateHealthBar();
        }
    }

    private void UpdateHealthBar()
    {
        // Is the health bar UI initialized?
        if (_PlayerHealthComponent == null || _UI_HealthBarImage == null || _UI_HealthBarText == null)
            return;


        float currentValue = _PlayerHealthComponent.CurrentHealth;
        float maxValue = _PlayerHealthComponent.MaxHealth;


        // Set the length of the parent object in case the player's max health has changed.
        float width = (_BaseBarLength + _PlayerHealthComponent.TotalMaxHealthIncrease) * _LengthMultiplier;
        _RectTransform.sizeDelta = new Vector2(width, _RectTransform.sizeDelta.y);


        // How far should we fill the health bar?
        float fillPercentage = currentValue / maxValue;
        float shadedBarSectionLength = Mathf.Round(_RectTransform.rect.width * fillPercentage);

        _UI_HealthBarBackgroundImage.rectTransform.sizeDelta = new Vector2(_RectTransform.rect.width,
                                                                           50f);

        _UI_HealthBarImage.rectTransform.sizeDelta = new Vector2(shadedBarSectionLength - (_BorderThickness * 2f), 
                                                                 50f - (_BorderThickness * 2));


        _UI_HealthBarBackgroundImage.color = _BackgroundColor;


        // Fade the bar between colors as health drops if enabled.
        if (_FadeBarColorAsHealthDrops)
        {
            if (_PlayerHealthComponent.CurrentHealth >= _PlayerHealthComponent.MaxHealth / 2)
                _UI_HealthBarImage.color = Color.Lerp(_MediumColor, _FullColor, (fillPercentage - 0.5f) * 2f);
            else
                _UI_HealthBarImage.color = Color.Lerp(_LowColor, _MediumColor, fillPercentage * 2f);
        }
        else
            _UI_HealthBarImage.color = _FullColor;


        // Update the health bar text.
        _UI_HealthBarText.text = $"Health: {currentValue} / {maxValue}";


        _HealthChanged = false;
    }

    private void OnPlayerHealthChanged(GameObject sender, GameObject changeSource, float changeAmount)
    {
        _HealthChanged = true;
    }

}
