using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;



/// <summary>
/// This class is the main input mamanger.
/// It holds references to several sub-managers. The code in them is based on the ThirdPersonController script
/// in Unity's official Third Person Controller asset.
/// HOWEVER, most of the code has been split into the sub-managers for organization, especially most of the code I added.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    PlayerInput _PlayerInputComponent;

    private Dictionary<int, InputActionMap> _InputActionMaps;

    public const string ACTION_MAP_BUILD_MODE = "Build Mode";
    public const string ACTION_MAP_PLAYER = "Player";
    public const string ACTION_MAP_UI = "UI";



    void Awake()
    {
        _InputActionMaps = new Dictionary<int, InputActionMap>();

        _PlayerInputComponent = GetComponent<PlayerInput>();
        if (_PlayerInputComponent == null)
            throw new Exception("The PlayerInput component was not found!");


        GetInputManagerReferences();
    }



    private void GetInputManagerReferences()
    {
        BuildMode = GetComponent<InputManager_BuildMode>();
        if (BuildMode == null)
            throw new Exception("The InputManager_BuildMode component was not found!");

        Player = GetComponent<InputManager_Player>();
        if (Player == null)
            throw new Exception("The InputManager_Player component was not found!");

        UI = GetComponent<InputManager_UI>();
        if (UI == null)
            throw new Exception("The InputManager_UI component was not found!");
    }

    public PlayerInput GetPlayerInputComponent()
    {
        return _PlayerInputComponent;
    }


    public void EnableInputActionMap(int inputActionMapID, bool state = true)
    {
        InputActionMap actionMap = _InputActionMaps[inputActionMapID];
    }

    public void EnableAllInputActionMaps(bool state)
    {
        foreach (InputActionMap actionMap in _InputActionMaps.Values)
        {
            if (state)
                actionMap.Enable();
            else
                actionMap.Disable();
        }

    }

    public void SwitchToActionMap(int inputActionMapID)
    {
        InputActionMap actionMap = _InputActionMaps[inputActionMapID];

        _PlayerInputComponent.SwitchCurrentActionMap(actionMap.name);
    }

    public bool IsActionMapActive(int inputActionMapID)
    {
        return _InputActionMaps[inputActionMapID].enabled;
    }

    public void RegisterInputActionMap(int inputActionMapID, string name, bool throwIfNotFound = true)
    {
        InputActionMap actionMap = _PlayerInputComponent.actions.FindActionMap(name, throwIfNotFound);

        _InputActionMaps.Add(inputActionMapID, actionMap);
    }

    public void ResetAllInputMapControlValues()
    {
        BuildMode.Reset();
        Player.Reset();
        UI.Reset();
    }



    public InputManager_BuildMode BuildMode { get; private set; }
    public InputManager_Player Player { get; private set; }
    public InputManager_UI UI { get; private set; }


}
