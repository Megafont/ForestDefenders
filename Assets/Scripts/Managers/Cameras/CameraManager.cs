using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Cinemachine;


public class CameraManager : MonoBehaviour
{
    private Dictionary<int, ICinemachineCamera> _Cameras;

    private ICinemachineCamera _ActiveCamera;
    private int _ActiveCameraID;



    private void Awake()
    {
        _Cameras = new Dictionary<int, ICinemachineCamera>();
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
        // Disable active cameras.
        foreach (ICinemachineCamera camera in _Cameras.Values)
        {
            DisableCamera(camera);
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

    public ICinemachineCamera GetActiveCamera()
    {
        return _ActiveCamera;
    }

    public ICinemachineCamera GetCameraWithID(int cameraID)
    {
        return _Cameras[cameraID];
    }

    public void RegisterCamera(int cameraID, ICinemachineCamera newCamera)
    {
        if (newCamera == null)
            throw new Exception($"The passed in camera of type \"{cameraID}\" is null!");


        _Cameras.Add(cameraID, newCamera);

        DisableCamera(newCamera);
    }

    private void DisableCamera(ICinemachineCamera camera)
    {
        camera.Priority = 0;
        camera.VirtualCameraGameObject.SetActive(false);
    }

}
