using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Cinemachine;
using Unity.VisualScripting;

public class CameraManager : MonoBehaviour
{
    private Dictionary<int, ICinemachineCamera> _Cameras;

    private ICinemachineCamera _ActiveCamera;
    private int _ActiveCameraID;


    public delegate void Camera_OnCameraChange(ICinemachineCamera startCam, ICinemachineCamera endCam);

    public Camera_OnCameraChange OnCameraTransitionStarted;
    public Camera_OnCameraChange OnCameraTransitionEnded;



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
        // If a camera transition is already in progress, then simply return.
        if (CinemachineCore.Instance.GetActiveBrain(0).IsBlending)
            return;


        ICinemachineCamera prevCam = _ActiveCamera;

        ICinemachineCamera requestedCam = _Cameras[cameraID];
        if (requestedCam == prevCam)
        {            
            return;
        }


        //Debug.Log($"Camera Transition Started    | Start Camera: \"{(prevCam != null ? prevCam.VirtualCameraGameObject.name : "null")}\"    | End Camera: \"{(requestedCam != null ? requestedCam.VirtualCameraGameObject.name : "null")}\"");

        DisableAllActiveCameras();


        // Enable the requested camera.
        requestedCam.Priority = 10;
        requestedCam.VirtualCameraGameObject.SetActive(true);
        _ActiveCameraID = cameraID;
        _ActiveCamera = requestedCam;

        OnCameraTransitionStarted?.Invoke(prevCam, _ActiveCamera);
        
        StartCoroutine(WaitForCameraTransitionToFinish(prevCam, _ActiveCamera));
    }

    private IEnumerator WaitForCameraTransitionToFinish(ICinemachineCamera startCam, ICinemachineCamera endCam)
    {
        if (startCam != null && endCam != null)
        {
            // Wait until the blend actually begins, because there is a slight delay between when the code that
            // called this function initiates it and when Cinemachine actually starts it.
            yield return new WaitUntil(() => CinemachineCore.Instance.IsLiveInBlend(startCam));

            // And now wait until the blend finishes.
            yield return new WaitUntil(() => !CinemachineCore.Instance.IsLiveInBlend(startCam));
        }


        //Debug.Log($"Camera Transition Ended      | Start Camera: \"{(startCam != null ? startCam.VirtualCameraGameObject.name : "null")}\"    | End Camera: \"{(endCam != null ? endCam.VirtualCameraGameObject.name : "null")}\"");

        OnCameraTransitionEnded?.Invoke(startCam, endCam);
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

    private void DisableAllActiveCameras()
    {
        // Disable active cameras.
        foreach (ICinemachineCamera camera in _Cameras.Values)
        {
            DisableCamera(camera);
        }
    }


}
