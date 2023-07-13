using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public abstract class InputMapManager : MonoBehaviour
{
    protected InputManager _InputManager;
    protected PlayerInput _PlayerInputComponent;
    protected InputActionMap _InputActionMap;



    void Awake()
    {
    }

    void Start()
    {

    }

    void Update()
    {
        if (_InputActionMap != null &&
            _InputActionMap.enabled)
        {
            UpdateInputs();
        }
    }



    public void Disable()
    {
        if (_InputActionMap != null)
            _InputActionMap.Disable();
        else
            LogWarning("Disable");

    }
    public void Enable()
    {
        if (_InputActionMap != null)
            _InputActionMap.Enable();
        else
            LogWarning("Enable");
    }

    public void SwitchToAsCurrentActionMap()
    {
        if (_PlayerInputComponent != null)
        {
            // This call resets all other input maps so that any that are on will be switched off.
            // Otherwise we end up with odd bugs. This way any buttons that were held down in the previously
            // active map will be turned off so they don't become stuck on because we disabled that input map.
            // In other words, the game will think they are still held down even if they are not.
            _InputManager.ResetAllInputMapControlValues();

            _PlayerInputComponent.SwitchCurrentActionMap(_InputActionMap.name);
        }
        else
            LogWarning("SwitchToAsCurrentActionMap");
    }



    protected void LogWarning(string methodName)
    {
        Debug.LogWarning($"{this.GetType()}.{methodName} was called before this input submanager was initialized!");
    }



    public virtual void Initialize(InputManager inputManager)
    {
        SetID();

        _InputManager = inputManager;
        _PlayerInputComponent = _InputManager.GetPlayerInputComponent();

        Init();
    }


    public abstract void ResetInputs();


    protected abstract void Init();
    protected abstract void SetID();
    protected abstract void UpdateInputs();



    public uint ID { get; protected set; }
    public InputActionMap InputActionMap { get { return _InputActionMap; } }
    public bool IsEnabled { get { return _InputActionMap.enabled; } }
}
