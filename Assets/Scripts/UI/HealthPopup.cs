using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;
using Cinemachine;
using TMPro;


public class HealthPopup : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve _AccelerationCurve;

    [SerializeField]
    [Range(0f, 5f)]
    private float _FadeStartDelay = 2.0f;

    [SerializeField]
    [Range(0f, 5f)]
    private float _FadeOutTime = 1.0f;

    [SerializeField]
    [Range(0f, 5f)]
    private float _MaxMoveSpeed = 2.5f;

    [SerializeField]
    private TMP_Text _TMP_Text;


    private float _ElapsedTime;
    private float _MoveSpeed;


    private static ObjectPool<HealthPopup> _HealthPopupPool;
    private static GameObject _HealthPopupPrefab;
    private static GameObject _HealthPopupsParent; // The parent object that will contain all health popups in the hierarchy.

    private static GameObject _PlayerCamera;


    // Start is called before the first frame update
    void Start()
    {
        if (_PlayerCamera == null)
            _PlayerCamera = GameManager.Instance.CameraManager.GetCameraWithID((int) CameraIDs.PlayerFollow).VirtualCameraGameObject;
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
        transform.parent.LookAt(_PlayerCamera.transform); // Make the text always face the player's camera.
    }

    public void ResetHealthPopup(float healthChangedAmount)
    {
        if (healthChangedAmount < 0)
        {
            _TMP_Text.text = $"{healthChangedAmount}";
            _TMP_Text.color = Color.red;
        }
        else
        {
            _TMP_Text.text = $"+{healthChangedAmount}";
            _TMP_Text.color = Color.green;
        }


        _ElapsedTime = 0;
        transform.parent.gameObject.SetActive(true);
    }


    
    public static HealthPopup ShowHealthPopup(Vector3 startPosition, float healthChangedAmount)
    {
        if (_HealthPopupPool == null)
            _HealthPopupPool = new ObjectPool<HealthPopup>(CreateHealthPopup, OnTakenFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 128, 1024);

        if (_HealthPopupsParent == null)
            _HealthPopupsParent = GameObject.Find("Health Popups");


        HealthPopup healthPopup = _HealthPopupPool.Get();
        healthPopup.transform.parent.position = startPosition; // See comments in Update() for why we're accessing the parent here.
        healthPopup.ResetHealthPopup(healthChangedAmount);
        
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
