using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;



public class KeyBindingTest : MonoBehaviour
{
    const string KEY_BINDS_PLAYER_PREFS_KEY = "KeyBinds";



    [SerializeField] private InputActionReference _TestBindingAction;
    
    private Button _BindButton;
    private TMP_Text _BindHeadingText;
    private TMP_Text _BindKeyText;

    private InputActionRebindingExtensions.RebindingOperation _RebindingOperation;

    private int _BindingIndex;

    private PlayerInput _PlayerInput;

    bool _IsRebindingKey = false;



    private void Awake()
    {
        // This will be obtained from the InputManager in the game.
        // I stuck the PlayerInput component here on this GameObject for simplicity.
        _PlayerInput = GetComponent<PlayerInput>();
        
        _BindButton = GameObject.Find("Bind Button").GetComponent<Button>();
        _BindHeadingText = GameObject.Find("Bind Key Heading Text (TMP)").GetComponent<TMP_Text>();
        _BindKeyText = GameObject.Find("Bind Key Text (TMP)").GetComponent<TMP_Text>();

        _BindingIndex = _TestBindingAction.action.GetBindingIndexForControl(_TestBindingAction.action.controls[0]);
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadKeyBinds();

        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_IsRebindingKey && _TestBindingAction.action.IsPressed())
            _BindKeyText.color = Color.green;
        else
            _BindKeyText.color = Color.white;
    }

    public void StartRebinding()
    {
        _IsRebindingKey = true;

        // You can't rebind a control that is enabled.
        _PlayerInput.currentActionMap.Disable();

        _BindButton.gameObject.SetActive(false);
        _BindKeyText.text = "[Press key to bind]";

        _RebindingOperation = _TestBindingAction.action.PerformInteractiveRebinding()
                                    .WithControlsExcluding("Mouse")
                                    .OnMatchWaitForAnother(0.1f)
                                    .OnComplete(operation => RebindComplete())
                                    .Start();
    }

    private void RebindComplete()
    {
        _RebindingOperation.Dispose();

        _BindButton.gameObject.SetActive(true);

        _PlayerInput.currentActionMap.Enable();

        _IsRebindingKey = false;

        UpdateUI();
    }


    public void SaveKeyBinds()
    {
        string keyBinds = _PlayerInput.actions.SaveBindingOverridesAsJson();

        PlayerPrefs.SetString(KEY_BINDS_PLAYER_PREFS_KEY, keyBinds);
    }

    private void LoadKeyBinds()
    {
        string keyBinds = PlayerPrefs.GetString(KEY_BINDS_PLAYER_PREFS_KEY, string.Empty);

        if (string.IsNullOrWhiteSpace(keyBinds))
            return;

        _PlayerInput.actions.LoadBindingOverridesFromJson(keyBinds);
    }

    private void UpdateUI()
    {
        _BindKeyText.text = InputControlPath.ToHumanReadableString(_TestBindingAction.action.bindings[_BindingIndex].effectivePath,
                                                           InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

}
