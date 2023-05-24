using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

        Dialog_OnStart();

        // If the dialog has opened by default, leave it alone. Otherwise, deactivate its game object.
        if (!IsOpen())
            gameObject.SetActive(false);
    }

    void OnEnable()
    {
        Dialog_OnEnable();
    }

    void OnDestroy()
    {
        Dialog_OnDestroy();

        OpenDialogs.Clear();
    }

    // Update is called once per frame
    void Update()
    {       
        if (!IsOpen())
            return;


        Dialog_OnUpdate();


        if (_InputManager_UI.Navigate != Vector2.zero)
            Dialog_OnNavigate();

        if (_InputManager_UI != null)
        {
            if (_InputManager_UI.Confirm)
                Dialog_OnConfirm();
            else if (_InputManager_UI.Cancel)
                Dialog_OnCancel();
        }

    }

    void OnGUI()
    {

    }



    protected virtual void Dialog_OnAwake() { }
    protected virtual void Dialog_OnStart() { }
    protected virtual void Dialog_OnEnable() { }
    protected virtual void Dialog_OnUpdate() { }
    protected virtual void Dialog_OnDestroy() { }


    // This method is called when the user uses any non-mouse navigational UI controls (WASD/Arrow keys, Joystick, Gamepad).
    protected virtual void Dialog_OnNavigate() { }
    // These two methods are called when the users presses the confirm or cancel buttons on the gamepad.
    protected virtual void Dialog_OnConfirm() { }
    protected virtual void Dialog_OnCancel() { }



    public virtual void CloseDialog()
    {
        if (_PauseGameWhileDialogOpen)
            Time.timeScale = 1.0f;


        OpenDialogs.Remove(this);


        if (OpenDialogs.Count < 1)
            Cursor.visible = false;


        StartCoroutine(SwitchInputMaps(false));

    }

    public virtual void OpenDialog(bool closeOtherOpenDialogs = false)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


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
                CloseAllDialogsExcept(this);
            }
            else
            {
                Debug.LogWarning($"The \"{this.GetType()}\" could not open, because another dialog is already open!");
                return;
            }
        }


        if (_PauseGameWhileDialogOpen)
            Time.timeScale = 0.0f;


        OpenDialogs.Add(this);
        gameObject.SetActive(true);

        StartCoroutine(SwitchInputMaps(true));
    }

    public void CloseAllOpenDialogs()
    {
        StartCoroutine(SwitchInputMaps(false));

        CloseAllDialogsExcept(null);
    }

    protected virtual IEnumerator SwitchInputMaps(bool dialogIsOpening)
    {        
        if (dialogIsOpening)
        {
            //_GameManager.InputManager.SwitchToActionMap((int)InputActionMapIDs.UI)

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

            // Wait for a short delay so the player character can't react to the last UI button press.
            // The UI confirm button and player attack buttons are the same one, so this prevents that issue.
            yield return new WaitForSeconds(_GameManager.DialogCloseInputChangeDelay);


            // This check is here, because otherwise the building selection menu will auto close a fraction of a second after the second menu opens.
            // So this check catches that scenario and aborts the rest of this code since setting the GameObject to be inactive hides the window.
            if (!IsOpen())
            {
                _PreviouslyActiveInputMap?.Enable();

                // Lastly, disable the dialog's GameObject.
                gameObject.SetActive(false);
            }
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



    public static void CloseAllDialogsExcept(Dialog_Base dialogToIgnore)
    {
        for (int i = 0; i < OpenDialogs.Count; i++)
        {
            if (dialogToIgnore != null && OpenDialogs[i] == (IDialog) dialogToIgnore)
                OpenDialogs[i].CloseDialog();

        } // end for i
    }


}
