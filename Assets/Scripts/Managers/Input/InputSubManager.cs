using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public abstract class InputSubManager : MonoBehaviour
{
    protected InputManager _InputManager;



    private void Awake()
    {
        _InputManager = GetComponent<InputManager>();
        if (_InputManager == null)
            throw new Exception("The InputManager component was not found!");


        Init();
    }



    protected abstract void Init();

}
