using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;
using TMPro;


public class HealthPopup : MonoBehaviour
{
    [SerializeField] private AnimationCurve _AccelerationCurve;

    [Range(0f, 5f)]
    [SerializeField] private float _FadeStartDelay = 2.0f;
    
    [Range(0f, 5f)]
    [SerializeField] private float _FadeOutTime = 1.0f;

    [Range(0f, 5f)]
    [SerializeField] private float _MaxMoveSpeed = 2.5f;

    [SerializeField] private TMP_Text _TMP_Text;


    private float _ElapsedTime;
    private float _MoveSpeed;

    private static Color32 _DamageNormalColor = Color.red;
    private static Color32 _DamageResistanceColor = new Color32(50, 0, 0, 255);
    private static Color32 _DamageVulnerableColor = new Color32(255, 100, 0, 255);
    private static Color32 _HealColor = Color.green;


    private static ObjectPool<HealthPopup> _HealthPopupPool;
    private static GameObject _HealthPopupPrefab;
    private static GameObject _HealthPopupsParent; // The parent object that will contain all health popups in the hierarchy.

    private static CameraManager _CameraManager;



    // Start is called before the first frame update
    void Start()
    {
        _CameraManager = GameManager.Instance.CameraManager;
    }

    // Update is called once per frame
    void Update()
    {
        _ElapsedTime += Time.deltaTime;

        if (_ElapsedTime <= _FadeStartDelay)
        {
            float curveResult = _AccelerationCurve.Evaluate(_ElapsedTime / _FadeStartDelay);

            _MoveSpeed = Mathf.Max(_MoveSpeed, (_MaxMoveSpeed * curveResult));
        }
        else if (_ElapsedTime - _FadeStartDelay <= _FadeOutTime)
        {
            float percent = (_ElapsedTime - _FadeStartDelay) / _FadeOutTime;

            _TMP_Text.color = Color.Lerp(_TMP_Text.color, Color.clear, percent);
        }
        else
        {
            _HealthPopupPool.Release(this);
            transform.parent.gameObject.SetActive(false);
        }


        // We need to use the parent here, because the text component is on a child object rotated 180 degrees so it always faces the right way.
        // Otherwise it ends up backwards when the parent object turns around to face the camera. The text is facing down the negative Z-axis by default.
        transform.parent.position += Vector3.up * _MoveSpeed * Time.deltaTime;
        transform.parent.LookAt(_CameraManager.GetActiveCamera().VirtualCameraGameObject.transform); // Make the text always face the player's camera.
    }

    public void ResetHealthPopup(float healthChangedAmount, float buffAmount = 0)
    {
        if (healthChangedAmount >= 0)
        {
            _TMP_Text.text = $"+{healthChangedAmount}";
            _TMP_Text.color = _HealColor;
        }
        else
        {
            _TMP_Text.text = $"{healthChangedAmount}";

            if (buffAmount < 0)
                _TMP_Text.color = _DamageResistanceColor;
            else if (buffAmount > 0)
                _TMP_Text.color = _DamageVulnerableColor;
            else
                _TMP_Text.color = _DamageNormalColor;
        }


        _ElapsedTime = 0;
        transform.parent.gameObject.SetActive(true);
    }


    
    public static HealthPopup ShowHealthPopup(Vector3 startPosition, float healthChangedAmount, float buffAmount)
    {
        if (_HealthPopupPool == null)
            _HealthPopupPool = new ObjectPool<HealthPopup>(CreateHealthPopup, OnTakenFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 128, 1024);

        if (_HealthPopupsParent == null)
            _HealthPopupsParent = GameObject.Find("Health Popups");


        HealthPopup healthPopup = _HealthPopupPool.Get();
        healthPopup.transform.parent.position = startPosition; // See comments in Update() for why we're accessing the parent here.
        healthPopup.ResetHealthPopup(healthChangedAmount, buffAmount);
        
        return healthPopup;        
    }



    // HealthPopup Pool Methods
    // ====================================================================================================

    private static HealthPopup CreateHealthPopup()
    {
        if (_HealthPopupPrefab == null)
            _HealthPopupPrefab = Resources.Load<GameObject>("UI/Common/HealthPopup");


        // Instantiate a new health popup underneath the world so the player can't see it spawn in.
        GameObject newHealthPopupObject = Instantiate(_HealthPopupPrefab, new Vector3(0, -128, 0), Quaternion.identity, _HealthPopupsParent.transform);

        HealthPopup newHealthPopupComponent = newHealthPopupObject.GetComponentInChildren<HealthPopup>();

        return newHealthPopupComponent;
    }


    private static void OnReturnedToPool(HealthPopup projectile)
    {
        //Debug.Log($"R - Pool Counts:    Total: {_HealthPopupPool.CountAll}    Active: {_HealthPopupPool.CountActive}    Inactive: {_HealthPopupPool.CountInactive}");
    }

    private static void OnTakenFromPool(HealthPopup projectile)
    {
        //Debug.Log($"T - Pool Counts:    Total: {_HealthPopupPool.CountAll}    Active: {_HealthPopupPool.CountActive}    Inactive: {_HealthPopupPool.CountInactive}");
    }

    private static void OnDestroyPoolObject(HealthPopup projectile)
    {
        Destroy(projectile.gameObject);

        //Debug.Log("Destroyed pool health popup!");
    }

    // ====================================================================================================
    
}
