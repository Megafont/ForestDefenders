using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PlayerHungerBar : MonoBehaviour
{
    [Tooltip("The length of the bar when the player has no upgrades to max hunger.")]
    [SerializeField] private float _BaseBarLength = 50;

    [Tooltip("The length of the hunger bar is the current value of the bar times this multiplier.")]
    [Range(0.25f, 10f)]
    [SerializeField] private float _LengthMultiplier = 5f;

    [Range(1f, 10f)]
    [SerializeField] private float _BorderThickness = 3f;

    [SerializeField] private bool _FadeBarColorAsHungerIncreases = true;
    [SerializeField] private Color32 _BackgroundColor = Color.gray;
    [SerializeField] private Color32 _FullColor = new Color32(0, 150, 0, 255);
    [SerializeField] private Color32 _MediumColor = new Color32(255, 170, 0, 255);
    [SerializeField] private Color32 _LowColor = Color.red;



    private RectTransform _RectTransform;

    private Image _UI_HungerBarBackgroundImage;
    private Image _UI_HungerBarImage;
    private TMP_Text _UI_HungerBarText;

    private GameManager _GameManager;
    private Hunger _PlayerHungerComponent;

    private bool _HungerChanged = true; // This is set to true to force an initial update of the health bar.


    // Start is called before the first frame update
    void Start()
    {
        _GameManager = GameManager.Instance;

        GameObject healthBarBackgroundObj = transform.Find("Hunger Bar Background").gameObject;
        _UI_HungerBarBackgroundImage = healthBarBackgroundObj.GetComponent<Image>();

        GameObject healthBarObj = transform.Find("Hunger Bar").gameObject;
        _UI_HungerBarImage = healthBarObj.GetComponent<Image>();

        if (_GameManager.Player)
        {
            _PlayerHungerComponent = _GameManager.Player.GetComponent<Hunger>();
            _PlayerHungerComponent.OnHungerChanged += OnPlayerHungerChanged;
        }

        GameObject healthBarTextObj = transform.Find("Hunger Bar Text (TMP)").gameObject;
        _UI_HungerBarText = healthBarTextObj.GetComponent<TMP_Text>();

        _RectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_HungerChanged)
        {
            UpdateHealthBar();
        }
    }

    private void UpdateHealthBar()
    {
        // Is the health bar UI initialized?
        if (_PlayerHungerComponent == null || _UI_HungerBarImage == null || _UI_HungerBarText == null)
            return;


        float currentValue = _PlayerHungerComponent.CurrentHunger;
        float maxValue = _PlayerHungerComponent.MaxHunger;


        // Set the length of the parent object in case the player's max health has changed.
        float width = (_BaseBarLength + _PlayerHungerComponent.TotalMaxHungerIncrease) * _LengthMultiplier;
        _RectTransform.sizeDelta = new Vector2(width, _RectTransform.sizeDelta.y);

        //_RectTransform.sizeDelta = new Vector2(maxValue * _LengthMultiplier, _RectTransform.sizeDelta.y);


        // How far should we fill the health bar?
        float fillPercentage = currentValue / maxValue;
        float shadedBarSectionLength = Mathf.Round(_RectTransform.rect.width * fillPercentage);

        _UI_HungerBarBackgroundImage.rectTransform.sizeDelta = new Vector2(_RectTransform.rect.width,
                                                                           50f);

        _UI_HungerBarImage.rectTransform.sizeDelta = new Vector2(shadedBarSectionLength - (_BorderThickness * 2f), 
                                                                 50f - (_BorderThickness * 2));


        _UI_HungerBarBackgroundImage.color = _BackgroundColor;


        // Fade the bar between colors as health drops if enabled.
        if (_FadeBarColorAsHungerIncreases)
        {
            if (_PlayerHungerComponent.CurrentHunger >= _PlayerHungerComponent.MaxHunger / 2)
                _UI_HungerBarImage.color = Color.Lerp(_MediumColor, _FullColor, (fillPercentage - 0.5f) * 2f);
            else
                _UI_HungerBarImage.color = Color.Lerp(_LowColor, _MediumColor, fillPercentage * 2f);
        }
        else
            _UI_HungerBarImage.color = _FullColor;


        // Update the health bar text.
        _UI_HungerBarText.text = $"Hunger: {currentValue} / {maxValue}";


        _HungerChanged = false;
    }

    private void OnPlayerHungerChanged(GameObject sender, float changeAmount)
    {
        _HungerChanged = true;
    }

}
