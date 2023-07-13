using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private Dictionary<uint, InputMapManager> _InputMapManagers;



    void Awake()
    {
        _InputMapManagers = new Dictionary<uint, InputMapManager>();

        _PlayerInputComponent = GetComponent<PlayerInput>();
        if (_PlayerInputComponent == null)
            throw new Exception("The PlayerInput component was not found!");


        GetInputManagerReferences();
    }



    private void GetInputManagerReferences()
    {
        InputMapManager[] foundInputMapManagers = GetComponents<InputMapManager>();


        foreach (InputMapManager manager in foundInputMapManagers)
        {
            manager.Initialize(this);

            _InputMapManagers.Add(manager.ID, manager);            
        }
    }

    public PlayerInput GetPlayerInputComponent()
    {
        return _PlayerInputComponent;
    }

    /// <summary>
    /// Gets the InputMapManager for the current input action map returned by the PlayerInput component.
    /// </summary>
    /// <returns>The InputMapManager associated with the current input action map, or null if it is not found.</returns>
    public InputMapManager GetInputMapManagerOfCurrentInputActionMap()
    {
        string currentMapName = _PlayerInputComponent.currentActionMap.name;


        foreach (InputMapManager inputMapManager in _InputMapManagers.Values)
        {
            if (inputMapManager.InputActionMap.name == currentMapName)
                return inputMapManager;
        }


        return null;
    }

    public InputActionMap GetCurrentInputActionMap()
    {
        return _PlayerInputComponent.currentActionMap;
    }

    public string GetCurrentInputControlScheme()
    {
        return _PlayerInputComponent.currentControlScheme;
    }

    /// <summary>
    /// Gets the specified action map.
    /// </summary>
    /// <param name="actionMapID">The ID number of the input action map manager to retrieve. This is of type uint rather than using an enum directly so that this code can be used in any project with any set of input action maps.</param>
    /// <returns>The input action map manager with the specified ID.</returns>
    public InputMapManager GetInputMapManager(uint actionMapID)
    {
        return _InputMapManagers[actionMapID];
    }

    /// <summary>
    /// Gets the specified action map.
    /// </summary>
    /// <param name="actionMapID">The ID number of the input action map to retrieve. This is of type uint rather than using an enum directly so that this code can be used in any project with any set of input action maps.</param>
    /// <returns>The input action map with the specified ID.</returns>
    public InputActionMap GetInputActionMap(uint actionMapID)
    {
        return _InputMapManagers[actionMapID].InputActionMap;
    }

    public void EnableInputActionMap(uint inputActionMapID, bool state = true)
    {
        InputMapManager inputMapManager = _InputMapManagers[inputActionMapID];

        if (state)
            inputMapManager.Enable();
        else
            inputMapManager.Disable();
    }

    public void EnableAllInputActionMaps(bool state)
    {
        foreach (InputMapManager inputMapManager in _InputMapManagers.Values)
        {
            if (state)
                inputMapManager.Enable();
            else
                inputMapManager.Disable();
        }

    }

    public void SwitchToActionMap(uint inputActionMapID)
    {
        _InputMapManagers[inputActionMapID].SwitchToAsCurrentActionMap();
    }

    public bool IsActionMapActive(uint inputActionMapID)
    {
        return _InputMapManagers[inputActionMapID].IsEnabled;
    }

    public void ResetAllInputMapControlValues()
    {
        foreach (InputMapManager manager in _InputMapManagers.Values)
        {
            manager.ResetInputs();
        }

    }


}
