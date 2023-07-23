using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Dialog_Base : MonoBehaviour, IDialog
{
    protected const float DIALOG_SCROLL_SPEED = 10.0f;


    private static List<IDialog> OpenDialogs = new List<IDialog>();


    protected GameManager _GameManager;

    protected InputManager _InputManager;
    protected InputMapManager_UI _InputManager_UI;

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
        _InputManager_UI = (InputMapManager_UI) _InputManager.GetInputMapManager((uint) InputActionMapIDs.UI);


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
            if (_InputManager_UI.Submit)
                Dialog_OnSubmit();
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
    // These two methods are called when the users presses the submit or cancel buttons on the gamepad.
    protected virtual void Dialog_OnSubmit() { }
    protected virtual void Dialog_OnCancel() { }



    public virtual void CloseDialog()
    {
        if (!IsOpen())
        {
            Debug.LogWarning($"CloseDialog() was called on {this.GetType().Name}, while the dialog is not open!");
            return;
        }


        if (_PauseGameWhileDialogOpen)
            Time.timeScale = 1.0f;


        OpenDialogs.Remove(this);


        // Only disable mouse cursor, and switch input maps if no dialogs are still open.
        if (OpenDialogs.Count < 1)
        {
            Cursor.visible = false;

            // Switch input maps and deactivate the dialog.
            StartCoroutine(SwitchInputMaps(false));
        }


        gameObject.SetActive(false);
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
            // Switch to the UI input map.
            _PreviouslyActiveInputMap = _InputManager.GetPlayerInputComponent().currentActionMap;
            _PreviouslyActiveInputMap?.Disable();

            _InputManager_UI.Enable();


            // Disable any inputs that may still be on. This stops the player from sometimes stabbing over and over and over again while the dialog is open.
            _InputManager.ResetAllInputMapControlValues();
        }
        else
        {
            // Only disable the UI input map and re-enable the previously active input map if no dialogs are still open.
            if (OpenDialogs.Count < 1)
            {            
                // Disable the UI input map.
                _InputManager_UI.Disable();

                // Re-enable the previous active input map.
                _PreviouslyActiveInputMap?.Enable();
            }


            // Wait for a short delay so the player character can't react to the last UI button press.
            // The UI submit button and player attack buttons are the same one, so this prevents that issue.
            yield return new WaitForSeconds(_GameManager.DialogCloseInputChangeDelay);

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



    public static void CloseAllDialogsExcept(IDialog dialogToIgnore)
    {
        if (OpenDialogs.Count < 1)
            return;


        for (int i = OpenDialogs.Count - 1; i >= 0; i--)
        {
            if (dialogToIgnore == null ||
                (dialogToIgnore != null && OpenDialogs[i] != dialogToIgnore))
            {
                //Debug.Log($"Dialog_Base.CloseAllDialogsExcept() closed dialog [{i}] ({OpenDialogs[i].GetType()}).");

                OpenDialogs[i].CloseDialog();
            }

        } // end for i
    }


}
