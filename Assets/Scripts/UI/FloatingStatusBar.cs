using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using TMPro;


public class FloatingStatusBar : MonoBehaviour
{
    [Range(1f, 10f)]
    [SerializeField] private float _BorderThickness = 3f;
    [Min(25)]
    [SerializeField] private int _BarLength = 100;
    [Min(0)]
    [SerializeField] private float _DefaultBarScale = 0.0025f;
    [Min(1)]
    [SerializeField] private int _MaxVisibleDistance = 30;

    [SerializeField] private bool _FadeBarColorAsStatDrops = true;
    [SerializeField] private Color32 _BackgroundColor = Color.gray;
    [SerializeField] private Color32 _FullColor = new Color32(0, 150, 0, 255);
    [SerializeField] private Color32 _MediumColor = new Color32(255, 170, 0, 255);
    [SerializeField] private Color32 _LowColor = Color.red;


    private float _VerticalPosOffset;

    private RectTransform _RectTransform;

    private Image _UI_StatBarBackgroundImage;
    private Image _UI_StatBarImage;
    private TMP_Text _UI_StatBarText;

    private GameManager _GameManager;
    private CameraManager _CameraManager;




    // Start is called before the first frame update
    void Start()
    {
        _GameManager = GameManager.Instance;
        _CameraManager = _GameManager.CameraManager;

        StatBarScale = _DefaultBarScale;

        VerticalPosOffset = 1f;

        GameObject healthBarBackgroundObj = transform.Find("Stat Bar Background").gameObject;
        _UI_StatBarBackgroundImage = healthBarBackgroundObj.GetComponent<Image>();

        GameObject healthBarObj = transform.Find("Stat Bar").gameObject;
        _UI_StatBarImage = healthBarObj.GetComponent<Image>();

        GameObject healthBarTextObj = transform.Find("Stat Bar Text (TMP)").gameObject;
        _UI_StatBarText = healthBarTextObj.GetComponent<TMP_Text>();

        _RectTransform = GetComponent<RectTransform>();

        UpdateStatBar();
    }

    private void Update()
    {
        //Debug.Log($"Distance from player: {Vector3.Distance(transform.position, _GameManager.Player.transform.position)}");

        ICinemachineCamera camera = _CameraManager.GetActiveCamera();
        if (camera == null)
            return;


        // If this floating status bar is within the distance limit from the active camera, then make it visible. Otherwise hide it.
        if (Vector3.Distance(transform.position, camera.VirtualCameraGameObject.transform.position) <= _MaxVisibleDistance)
        {
            // Reset the bar's scale to make it visible.
            StatBarScale = _DefaultBarScale;

            // We need to use the parent here, because the text component is on a child object rotated 180 degrees so it always faces the right way.
            // Otherwise it ends up backwards when the parent object turns around to face the camera. The text is facing down the negative Z-axis by default.
            transform.LookAt(_CameraManager.GetActiveCamera().VirtualCameraGameObject.transform); // Make the text always face the player's camera.
        }
        else
        {
            // Hide the bar by setting its scale to 0. This way the status bar's GameObject is still active so it can redisplay itself when
            // the player gets close enough.
            StatBarScale = 0;
        }

    }

    private void UpdateStatBar()
    {
        // Is the health bar UI initialized?
        if (_UI_StatBarImage == null || _UI_StatBarText == null)
            return;

        // Set the length of the parent object in case the player's max health has changed.
        //float width = Mathf.Max(MaxValue * _LengthMultiplier, 150f); // Make sure the bar is never too narrow for the text.
        _RectTransform.sizeDelta = new Vector2(_BarLength, _RectTransform.sizeDelta.y);


        // How far should we fill the health bar?
        float fillPercentage = CurrentValue / MaxValue;
        float shadedBarSectionLength = Mathf.Round(_RectTransform.rect.width * fillPercentage);

        _UI_StatBarBackgroundImage.rectTransform.sizeDelta = new Vector2(_RectTransform.rect.width,
                                                                           50f);

        _UI_StatBarImage.rectTransform.sizeDelta = new Vector2(shadedBarSectionLength - (_BorderThickness * 2), 
                                                               43f);

        _UI_StatBarText.rectTransform.sizeDelta = new Vector2(_RectTransform.rect.width,
                                                              50f);

        _UI_StatBarBackgroundImage.color = _BackgroundColor;


        // Fade the bar between colors as health drops if enabled.
        if (_FadeBarColorAsStatDrops)
        {
            if (CurrentValue >= MaxValue / 2)
                _UI_StatBarImage.color = Color.Lerp(_MediumColor, _FullColor, (fillPercentage - 0.5f) * 2f);
            else
                _UI_StatBarImage.color = Color.Lerp(_LowColor, _MediumColor, fillPercentage * 2f);
        }
        else
            _UI_StatBarImage.color = _FullColor;


        // Update the health bar text.
        _UI_StatBarText.text = $"{Label} {CurrentValue} / {MaxValue}";
    }

    public void SetValue(float value)
    {
        CurrentValue = value <= MaxValue ? value : MaxValue;
        UpdateStatBar();
    }



    public float MaxValue { get; set; }
    public float CurrentValue { get; private set; }


    public string Label { get; set; } = "Value:";

    public float VerticalPosOffset
    {
        get { return _VerticalPosOffset; }
        set
        {
            _VerticalPosOffset = value;

            transform.localPosition = new Vector3(transform.localPosition.x, 
                                                  transform.localPosition.y + _VerticalPosOffset,
                                                  transform.localPosition.z);
        }
    }

    public float StatBarScale
    {
        get { return transform.localScale.x; }
        set { transform.localScale = new Vector3(value, value, value); }
    }

}
