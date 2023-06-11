using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;
using Cinemachine;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using System.Runtime.CompilerServices;

public class TextPopup : MonoBehaviour
{
    private static ObjectPool<TextPopup> _TextPopupPool;
    private static GameObject _TextPopupPrefab;
    private static GameObject _TextPopupsParent; // The parent object that will contain all text popups in the hierarchy.

    private static GameManager _GameManager;
    private static CameraManager _CameraManager;



    private const float DEFAULT_POPUP_SCALE = 0.1f;
    private const float DEFAULT_TEXT_SIZE = 20f;
    private const float DEFAULT_OUTLINE_WIDTH = 0.2f;
    private const float DEFAULT_MAX_VISIBLE_DISTANCE = 20f;

    private const float DEFAULT_FADE_START_DELAY = 2.0f;
    private const float DEFAULT_FADE_OUT_TIME = 1.0f;
    private const float DEFAULT_MAX_MOVE_SPEED = 2.0f;

    private static readonly Color32 _DefaultTextColor = Color.white;
    private static readonly Color32 _DefaultOutlineColor = Color.black;



    [SerializeField]
    private AnimationCurve _AccelerationCurve;
    
    [Min(0)]
    [SerializeField] private float _DefaultPopupScale = DEFAULT_POPUP_SCALE;

    [Min(1)]
    [SerializeField] private float _MaxVisibleDistance = DEFAULT_MAX_VISIBLE_DISTANCE;



    private float _FadeStartDelay = DEFAULT_FADE_START_DELAY;
    private float _FadeOutTime = DEFAULT_FADE_OUT_TIME;
    private float _MaxMoveSpeed = DEFAULT_MAX_MOVE_SPEED;

    private TMP_Text _TMP_Text;

    private float _ElapsedTime;
    private float _MoveSpeed;

    private Color32 _TextColor;
    private Color32 _OutlineColor;



    private void Awake()
    {
        _TMP_Text = GetComponent<TMP_Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _GameManager = GameManager.Instance;
        _CameraManager = _GameManager.CameraManager;
    }

    // Update is called once per frame
    void Update()
    {
        _ElapsedTime += Time.deltaTime;

        UpdateAppearance();

        if (_ElapsedTime > _FadeStartDelay + _FadeOutTime)
        {
            _TextPopupPool.Release(this);
            transform.parent.gameObject.SetActive(false);
        }

    }

    private void UpdateAppearance()
    {
        //Debug.Log($"Distance from player: {Vector3.Distance(transform.position, _GameManager.Player.transform.position)}");

        ICinemachineCamera camera = _CameraManager.GetActiveCamera();
        if (camera == null)
            return ;

        
        // If this popup is within the distance limit from the active camera, then make it visible. Otherwise hide it.
        if (Vector3.Distance(transform.position, camera.VirtualCameraGameObject.transform.position) <= _MaxVisibleDistance)
        {
            // Reset the scale to make it visible.
            transform.localScale = new Vector3(_DefaultPopupScale, _DefaultPopupScale, _DefaultPopupScale);

            // We need to use the parent here, because the text component is on a child object rotated 180 degrees so it always faces the right way.
            // Otherwise it ends up backwards when the parent object turns around to face the camera. The text is facing down the negative Z-axis by default.
            transform.parent.position += Vector3.up * _MoveSpeed * Time.deltaTime;
            transform.parent.LookAt(_CameraManager.GetActiveCamera().VirtualCameraGameObject.transform); // Make the text always face the player's camera.


            // Animate the text popup.
            if (_ElapsedTime <= _FadeStartDelay)
            {
                float curveResult = _AccelerationCurve.Evaluate(_ElapsedTime / _FadeStartDelay);

                _MoveSpeed = Mathf.Max(_MoveSpeed, (_MaxMoveSpeed * curveResult));
            }
            else if (_ElapsedTime >= _FadeStartDelay && _ElapsedTime < _FadeStartDelay + _FadeOutTime)
            {
                float percent = (_ElapsedTime - _FadeStartDelay) / _FadeOutTime;

                _TMP_Text.color = Color.Lerp(_TextColor, Color.clear, percent);
                _TMP_Text.outlineColor = Color.Lerp(_OutlineColor, Color.clear, percent);
            }
        }
        else
        {
            // Hide the bar by setting its scale to 0. This way the status bar's GameObject is still active so it can redisplay itself when
            // the player gets close enough.
            transform.localScale = Vector3.zero;
        }
    }

    private void ResetTextPopup(Vector3 startPosition, string text, Color32 textColor, Color32 outlineColor,
                                float fontSize = DEFAULT_TEXT_SIZE, float outlineWidth = DEFAULT_OUTLINE_WIDTH,
                                float fadeStartDelay = DEFAULT_FADE_START_DELAY, float fadeOutTime = DEFAULT_FADE_OUT_TIME, float maxMoveSpeed = DEFAULT_MAX_MOVE_SPEED)
    {
        // See the comments in UpdateAppearance() for why we're accessing the parent here instead of the main object.
        transform.parent.position = startPosition;

        _TMP_Text.text = text;
        _TMP_Text.color = textColor;
        _TMP_Text.fontSize = fontSize;

        _TMP_Text.outlineColor = outlineColor;
        _TMP_Text.outlineWidth = outlineWidth;


        // Cache the text colors. We need to remember these to use them when we fade out the text popup.
        _TextColor = textColor;
        _OutlineColor = outlineColor;

        _FadeStartDelay = fadeStartDelay;
        _FadeOutTime = fadeOutTime;
        _MaxMoveSpeed = maxMoveSpeed;

        _ElapsedTime = 0;
        _MoveSpeed = 0;
        transform.parent.gameObject.SetActive(true);
    }

    public static void ShowTextPopup(Vector3 startPosition, string text,
                                     float fontSize = DEFAULT_TEXT_SIZE, float outlineWidth = DEFAULT_OUTLINE_WIDTH,
                                     float fadeStartDelay = DEFAULT_FADE_START_DELAY, float fadeOutTime = DEFAULT_FADE_OUT_TIME, float maxMoveSpeed = DEFAULT_MAX_MOVE_SPEED)
    {
        ShowTextPopup(startPosition,
                      text,
                      _DefaultTextColor,
                      _DefaultOutlineColor,
                      fontSize,
                      outlineWidth,
                      fadeStartDelay,
                      fadeOutTime,
                      maxMoveSpeed);
    }

    public static void ShowTextPopup(Vector3 startPosition, string text, Color32 textColor,
                                     float fontSize = DEFAULT_TEXT_SIZE, float outlineWidth = DEFAULT_OUTLINE_WIDTH,
                                     float fadeStartDelay = DEFAULT_FADE_START_DELAY, float fadeOutTime = DEFAULT_FADE_OUT_TIME, float maxMoveSpeed = DEFAULT_MAX_MOVE_SPEED)
    {
        ShowTextPopup(startPosition, 
                      text, 
                      textColor, 
                      _DefaultOutlineColor,
                      fontSize,
                      outlineWidth,
                      fadeStartDelay,
                      fadeOutTime,
                      maxMoveSpeed);
    }
    
    public static void ShowTextPopup(Vector3 startPosition, string text, Color32 textColor, Color32 outlineColor,
                                     float fontSize = DEFAULT_TEXT_SIZE, float outlineWidth = DEFAULT_OUTLINE_WIDTH,
                                     float fadeStartDelay = DEFAULT_FADE_START_DELAY, float fadeOutTime = DEFAULT_FADE_OUT_TIME, float maxMoveSpeed = DEFAULT_MAX_MOVE_SPEED)
    {
        if (_TextPopupPool == null)
            _TextPopupPool = new ObjectPool<TextPopup>(CreateTextPopup, OnTakenFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 128, 1024);

        if (_TextPopupsParent == null)
            _TextPopupsParent = GameObject.Find("Text Popups");


        TextPopup textPopup = _TextPopupPool.Get();
        if (textPopup == null)
            Debug.LogError("Failed to get a new text popup!");


        textPopup.ResetTextPopup(startPosition, text, textColor, outlineColor, fontSize, outlineWidth, fadeStartDelay, fadeOutTime, maxMoveSpeed);
    }

    public static IEnumerator ShowTextPopupDelayed(float delayInSeconds, Vector3 startPosition, string text,
                                                   float fontSize = DEFAULT_TEXT_SIZE, float outlineWidth = DEFAULT_OUTLINE_WIDTH,
                                                   float fadeStartDelay = DEFAULT_FADE_START_DELAY, float fadeOutTime = DEFAULT_FADE_OUT_TIME, float maxMoveSpeed = DEFAULT_MAX_MOVE_SPEED)
    {
        yield return new WaitForSeconds(delayInSeconds);

        ShowTextPopup(startPosition,
                      text,
                      _DefaultTextColor,
                      _DefaultOutlineColor,
                      fontSize,
                      outlineWidth,
                      fadeStartDelay,
                      fadeOutTime,
                      maxMoveSpeed);
    }

    public static IEnumerator ShowTextPopupDelayed(float delayInSeconds, Vector3 startPosition, string text, Color32 textColor,
                                                   float fontSize = DEFAULT_TEXT_SIZE, float outlineWidth = DEFAULT_OUTLINE_WIDTH,
                                                   float fadeStartDelay = DEFAULT_FADE_START_DELAY, float fadeOutTime = DEFAULT_FADE_OUT_TIME, float maxMoveSpeed = DEFAULT_MAX_MOVE_SPEED)
    {
        yield return new WaitForSeconds(delayInSeconds);

        ShowTextPopup(startPosition,
                      text,
                      textColor,
                      _DefaultOutlineColor,
                      fontSize,
                      outlineWidth,
                      fadeStartDelay,
                      fadeOutTime,
                      maxMoveSpeed);
    }

    public static IEnumerator ShowTextPopupDelayed(float delayInSeconds, Vector3 startPosition, string text, Color32 textColor, Color32 outlineColor,
                                                   float fontSize = DEFAULT_TEXT_SIZE, float outlineWidth = DEFAULT_OUTLINE_WIDTH,
                                                   float fadeStartDelay = DEFAULT_FADE_START_DELAY, float fadeOutTime = DEFAULT_FADE_OUT_TIME, float maxMoveSpeed = DEFAULT_MAX_MOVE_SPEED)
    {
        yield return new WaitForSeconds(delayInSeconds);

        ShowTextPopup(startPosition,
                      text,
                      textColor,
                      outlineColor,
                      fontSize,
                      outlineWidth,
                      fadeStartDelay,
                      fadeOutTime,
                      maxMoveSpeed);
    }

    public static Vector3 AdjustStartPosition(GameObject source)
    {
        if (source == null)
            throw new Exception("The passed in source object is null!");


        NavMeshAgent agent = source.GetComponent<NavMeshAgent>();
        IBuilding building = source.GetComponent<Building_Base>();


        Vector3 startPos = source.transform.position;
        if (agent != null)
            startPos.y += agent.height / 2;
        else if (building != null)
            startPos.y += building.GetBuildingDefinition().Size.y - 1.0f;
        else if (source.CompareTag("Player"))
            startPos.y += 1;        
        

        return startPos;
    }


    // TextPopup Pool Methods
    // ====================================================================================================

    private static TextPopup CreateTextPopup()
    {
        if (_TextPopupPrefab == null)
            _TextPopupPrefab = Resources.Load<GameObject>("UI/Common/Text Popup");


        // Instantiate a new text popup underneath the world so the player can't see it spawn in.
        GameObject newTextPopupObject = Instantiate(_TextPopupPrefab, new Vector3(0, -128, 0), Quaternion.identity, _TextPopupsParent.transform);

        TextPopup newTextPopupComponent = newTextPopupObject.GetComponentInChildren<TextPopup>();

        return newTextPopupComponent;
    }


    private static void OnReturnedToPool(TextPopup projectile)
    {
        //Debug.Log($"R - Pool Counts:    Total: {_TextPopupPool.CountAll}    Active: {_TextPopupPool.CountActive}    Inactive: {_TextPopupPool.CountInactive}");
    }

    private static void OnTakenFromPool(TextPopup projectile)
    {
        //Debug.Log($"T - Pool Counts:    Total: {_TextPopupPool.CountAll}    Active: {_TextPopupPool.CountActive}    Inactive: {_TextPopupPool.CountInactive}");
    }

    private static void OnDestroyPoolObject(TextPopup projectile)
    {
        Destroy(projectile.gameObject);

        //Debug.Log("Destroyed text popup!");
    }

    // ====================================================================================================
    
}
