using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Cinemachine;


public class Camera_Orbit : MonoBehaviour
{
    [SerializeField] private float _Speed = 10f;


    private CinemachineOrbitalTransposer _OrbitalTransposer;


    void Start()
    {
        var vcam = GetComponent<CinemachineVirtualCamera>();
        if (vcam != null)
            _OrbitalTransposer = vcam.GetCinemachineComponent<CinemachineOrbitalTransposer>();
    }

    void Update()
    {
        if (_OrbitalTransposer != null)
            _OrbitalTransposer.m_XAxis.Value += Time.deltaTime * _Speed;
    }

}
