using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Dialog_Base : MonoBehaviour, IDialog
{
    private static List<IDialog> OpenDialogs = new List<IDialog>();


    protected GameManager _GameManager;

    protected InputManager _InputManager;
    protected InputManager_UI _InputManager_UI;

    protected InputActionMap _PreviouslyActiveInputMap;

    protected bool _IgnoreGamePhaseText = false;
    protected bool _PauseGameWhileDialogOpen = false;



    void Awake()
    {
        _GameManager = GameManager.Instance;

        Dialog_OnAwake();
    }

    // Start is called before the first frame update
    void Start()
    {
        _InputManager = _GameManager.InputManager;
        _InputManager_UI = _InputManager.UI;
        
        CloseDialog();

        Dialog_OnStart();
    }

    void OnEnable()
    {
        Dialog_OnEnable();
    }

    // Update is called once per frame
    void Update()
    {
        Dialog_OnUpdate();
    }

    void OnGUI()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDestroy()
    { 
        Dialog_OnDestroy(); 
    }


    protected virtual void Dialog_OnAwake()
    {

    }

    protected virtual void Dialog_OnStart()
    {

    }

    protected virtual void Dialog_OnEnable()
    {

    }

    protected virtual void Dialog_OnUpdate()
    {

    }

    protected virtual void Dialog_OnDestroy()
    {

    }



    public virtual void CloseDialog()
    {
        if (_PauseGameWhileDialogOpen)
            Time.timeScale = 1.0f;


        Cursor.visible = false;

        SwitchInputMaps(false);

        gameObject.SetActive(false);
        OpenDialogs.Remove(this);
    }

    public virtual void OpenDialog(bool closeOtherOpenDialogs = false)
    {
        if (_InputManager == null)
        {
            Debug.LogWarning($"The \"{this.GetType()}\" could not open, because the InputManager is not ready yet!");
            return;
        }

        // Don't allow a dialog to open while game phase text is on the middle of the screen.
        if (!_IgnoreGamePhaseText && _GameManager.GamePhaseTextIsVisible)
        {
            Debug.LogWarning($"The \"{this.GetType()}\" could not open, because game phase text is currently visible on the center of the screen!");
            return;
        }


        if (AreAnyDialogsOpen())
        {
            if (closeOtherOpenDialogs)
            {
                CloseAllDialogs();
            }
            else
            {
                Debug.LogWarning($"The \"{this.GetType()}\" could not open, because another dialog is already open!");
                return;
            }
        }


        if (_PauseGameWhileDialogOpen)
            Time.timeScale = 0.0f;


        SwitchInputMaps(true);

        OpenDialogs.Add(this);
        gameObject.SetActive(true);
    }

    public void CloseAllOpenDialogs()
    {
        SwitchInputMaps(false);

        CloseAllDialogs();
    }

    protected virtual void SwitchInputMaps(bool dialogIsOpening)
    {
        if (dialogIsOpening)
        {
            //_GameManager.InputManager.SwitchToActionMap((int)InputActionMapIDs.UI);

            // Switch to the UI input map.
            _PreviouslyActiveInputMap = _InputManager.GetPlayerInputComponent().currentActionMap;
            _PreviouslyActiveInputMap?.Disable();

            _InputManager.GetPlayerInputComponent().actions.FindActionMap(InputManager.ACTION_MAP_UI).Enable();


            // Disable any inputs that may still be on. This stops the player from sometimes stabbing over and over and over again while the dialog is open.
            _InputManager.ResetAllInputMapControlValues();
        }
        else
        {
            // Switch to the previously active input map.
            _InputManager.GetPlayerInputComponent().actions.FindActionMap(InputManager.ACTION_MAP_UI).Disable();

            _PreviouslyActiveInputMap?.Enable();
        }

    }



    public static bool AreAnyDialogsOpen()
    {
        return OpenDialogs.Count > 0;
    }

    public static int OpenDialogsCount()
    {
        return OpenDialogs.Count;
    }

    /// <summary>
    /// Returns true if this dialog is open.
    /// </summary>
    public bool IsOpen()
    {
        return OpenDialogs.Contains(this);
    }



    public static void CloseAllDialogs()
    {
        for (int i = 0; i < OpenDialogs.Count; i++)
        {
            OpenDialogs[i].CloseDialog();

        } // end for i
    }


}
