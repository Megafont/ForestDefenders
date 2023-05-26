using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;



public class Building_IceTurret : Building_Base
{
    [Tooltip("How far away the ice turret can attack enemies.")]
    [Range(2f, 10f)]
    [SerializeField] private float _AttackRange = 10f;

    [Tooltip("How long the ice turret takes to charge up (in seconds).")]
    [Range(1f, 60f)]
    [SerializeField] private float _ChargeUpTime = 10f;

    [Tooltip("How long (in seconds) it takes the ice wave to expand from the tower to its maximum size.")]
    [Range(0.05f, 5f)]
    [SerializeField] private float _IceWaveTravelTime = 0.25f;

    [Tooltip("The percentage of the ice wave's expansion that the wave will start fading away at. A value of 0.75 means it will start fading out at 75% of the way to its max distance from the ice turret.")]
    [Range(0.05f, 0.95f)]
    [SerializeField] private float _IceWaveFadeOutThreshold = 0.75f;


    [Min(0.01f)]
    [SerializeField] private float _TopRotationsPerChargeUp = 5.0f;

    [Min(0.01f)]
    [SerializeField] private float _TopRotationsPerIceWave = 1.0f;

    [SerializeField] private AnimationCurve _TopAnimCurve;


    private GameObject _TurretTop;
    
    private GameObject _IceWave;
    private MeshRenderer _IceWaveRenderer;

    private float _TurretTopChargeUpTotalRotation; // Total rotation of the top of the tower during charge up.
    private float _TurretTopIceWaveTotalRotation; // Total rotation of the top of the tower during an ice wave blast.
    private float _IceWaveMinScale = 0.4f; // The starting scale of the ice wave before it fires.
    private float _IceWaveMaxScale; // The scale of the ice wave at its max distance from the tower just as it disappears.

    private bool _IsCharging = true;
    private float _ChargingElapsedTime;
    private float _WaveElapsedTime;




    protected override void InitBuilding()
    {
        ConfigureBasicBuildingSetup("Defense", "Ice Turret");

        _GameManager = GameManager.Instance;

        _TurretTopChargeUpTotalRotation = 360 * _TopRotationsPerChargeUp;
        _TurretTopIceWaveTotalRotation = 360 * _TopRotationsPerIceWave;

        _IceWaveMaxScale = _AttackRange / 1; // The ice wave has a radius of 1 (origin to the center of the ring's width), so dividing attack range by that value tells us how much the ring will be scaled up when it reaches its max distance from the tower.
    }

    protected override void ConfigureBuildingComponents()
    {
        base.ConfigureBuildingComponents();


        _TurretTop = transform.Find("Ice Turret Top").gameObject;

        _IceWave = transform.Find("Ice Wave").gameObject;
        _IceWaveRenderer = _IceWave.GetComponent<MeshRenderer>();
        ResetIceWave();
    }

    protected override void UpdateBuilding()
    {
        base.UpdateBuilding();


        if (_IsCharging)
        {
            _ChargingElapsedTime += Time.deltaTime;

            // Animate the top of the ice turret.
            float rotPercentComplete = _TopAnimCurve.Evaluate(_ChargingElapsedTime / _ChargeUpTime);
            float curRot = _TurretTopChargeUpTotalRotation * rotPercentComplete;
            _TurretTop.transform.rotation = Quaternion.Euler(new Vector3(0f, curRot, 0f));


            // Check if we're done charging up.
            if (_ChargingElapsedTime >= _ChargeUpTime)
            {
                _ChargingElapsedTime = 0;
                _IsCharging = false;
            }
        }
        else
        {
            _WaveElapsedTime += Time.deltaTime;

            // Animate the top of the ice turret.
            float rotPercentComplete = _TopAnimCurve.Evaluate(_WaveElapsedTime / _IceWaveTravelTime);
            float curRot = _TurretTopIceWaveTotalRotation * rotPercentComplete;
            _TurretTop.transform.rotation = Quaternion.Euler(new Vector3(0f, -curRot, 0f));


            // Check if the ice wave is done.
            if (_WaveElapsedTime > _IceWaveTravelTime)
            {
                _IsCharging = true;
                _WaveElapsedTime = 0;
                ResetIceWave();
            }
            else
            {
                ExpandIceWave();
            }
        }

    }


    private void ExpandIceWave()
    {
        float expansionPercentageComplete = _WaveElapsedTime / _IceWaveTravelTime;
        float curScale = (_IceWaveMaxScale - _IceWaveMinScale) * expansionPercentageComplete + _IceWaveMinScale;

        // We leave the y-axis alone. Not scaling it causes the wave to appear flatter as it expands, which looks nice.
        _IceWave.transform.localScale = new Vector3(curScale, _IceWave.transform.localScale.y, curScale);

        //Debug.Log($"Expansion {expansionPercentageComplete}");
        // Is it time to fade out?
        if (expansionPercentageComplete >= _IceWaveFadeOutThreshold)
        {
            float fadePercentage = (expansionPercentageComplete - _IceWaveFadeOutThreshold) / (1f - _IceWaveFadeOutThreshold);
            //Debug.Log($"Fade Amount: {fadePercentage}");

            Color32 color = _IceWaveRenderer.material.color;
            color.a = (byte) (255 - (fadePercentage * 255f));
            _IceWaveRenderer.material.color = color;

            //Debug.Log($"Color: {color}");
            //Debug.Log("");
        }
    }

    private void ResetIceWave()
    {
        _IceWave.transform.localScale = new Vector3(_IceWaveMinScale, _IceWaveMinScale, _IceWaveMinScale);

        // NOTE: We used to deactivate the ice ring here, and reactivate it when it fires again.
        //       However, this disables the coroutines it runs, so enemies gut stuck with infinite ice status effect! So that line was removed here.

        Color32 color = _IceWaveRenderer.material.color;
        color.a = 255;
        _IceWaveRenderer.material.color = color;
    }

}
