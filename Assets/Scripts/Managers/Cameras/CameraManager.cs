using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Cinemachine;



public class CameraManager : MonoBehaviour
{
    private Dictionary<int, ICinemachineCamera> _Cameras;

    private CinemachineBrain _ActiveBrain;

    private ICinemachineCamera _ActiveCamera;
    private int _ActiveCameraID;



    private void Awake()
    {
        _Cameras = new Dictionary<int, ICinemachineCamera>();

        _ActiveBrain = CinemachineCore.Instance.GetActiveBrain(0);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchToCamera(int cameraID)
    {
        // Simply return if the requested camera is already active.
        if (cameraID == _ActiveCameraID)
            return;


        // Disable active cameras.
        foreach (ICinemachineCamera camera in _Cameras.Values)
        {
            camera.Priority = 0;
            camera.VirtualCameraGameObject.SetActive(false);
        }


        // Enable the requested camera.
        _ActiveCamera = _Cameras[cameraID];
        _ActiveCamera.Priority = 10;
        _ActiveCamera.VirtualCameraGameObject.SetActive(true);
        _ActiveCameraID = cameraID;

    }

    public bool IsActiveCamera(int cameraID)
    {
        return cameraID == _ActiveCameraID;
    }

    public int GetActiveCameraID()
    {
        return _ActiveCameraID;
    }

    public void RegisterCamera(int cameraID, ICinemachineCamera newCamera)
    {
        if (newCamera == null)
            throw new Exception($"The passed in camera of type \"{cameraID}\" is null!");


        _Cameras.Add(cameraID, newCamera);       
    }

}
