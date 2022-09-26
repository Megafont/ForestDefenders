using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using StarterAssets;
using TMPro;


public class RadialMenu : MonoBehaviour
{
    private List<RadialMenuItem> _MenuItems;

    public GameObject RadialMenuItemPrefab;


    [Min(1)]
    public float Radius = 150;





    public delegate void RadialMenuEventHandler(GameObject sender);
    public event RadialMenuEventHandler OnSelectionChanged;
    

    private BuildModeManager _BuildModeManager;

    private StarterAssetsInputs _Input;

    private bool _IsInitializing;
    private bool _IsOpen;
    private float _MenuItemSizeInDegrees;
    private int _PrevSelectedItemIndex;

    private GameObject _RadialMenuPanel;

    private TMP_Text _MenuTitleUI;
    private TMP_Text _MenuBottomBarUI;
    private Color32 _MenuBottomBarTextColor = new Color32(255, 160, 0, 0);
    private Color32 _MenuTitleTextColor = new Color32(255, 160, 0, 0);

    private GameObject _RadialMenuItemsParent;
    
    

    // Start is called before the first frame update
    void Start()
    {
        _BuildModeManager = GameManager.Instance.BuildModeManager;
        _Input = GameManager.Instance.PlayerInput;

        _MenuTitleUI = GameObject.Find("Radial Menu/Panel/Title Bar/Title Text (TMP)").GetComponent<TMP_Text>();
        _MenuBottomBarUI = GameObject.Find("Radial Menu/Panel/Bottom Text Bar/Bottom Text Bar (TMP)").GetComponent<TMP_Text>();
        _RadialMenuPanel = GameObject.Find("Radial Menu/Panel");
        _RadialMenuItemsParent = GameObject.Find("Radial Menu/Panel/Menu Items Parent");


        _MenuItems = new List<RadialMenuItem>();
        // Create one menu item so the list isn't empty (to prevent errors).
        GameObject newItem = Instantiate(RadialMenuItemPrefab, Vector3.zero, Quaternion.identity, _RadialMenuItemsParent.transform);
        _MenuItems.Add(newItem.GetComponent<RadialMenuItem>());

        _RadialMenuPanel.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {

    }



    public void ShowRadialMenu(string title, string[] names, int defaultItemIndex = 0)
    {
        if (names.Length == 0)
            throw new Exception("The passed in menu items list is empty!");
        if (defaultItemIndex < 0)
            throw new Exception("The passed in default menu item index must be positive!");
        if (defaultItemIndex >= names.Length) // This is >= instead of > to take into account the "Cancel" item that is added to the end of the menu automatically.
            throw new Exception("The passed in default menu item index must not be larger than the number of menu items!");

        if (_RadialMenuPanel == null)
            throw new Exception("The radial menu panel is null!");


        if (_IsInitializing || _IsOpen)
        {
            Debug.LogError("Can't show radial menu because one is already open!");
            return;
        }


        _IsInitializing = true;

        MenuConfirmed = false;
        MenuCancelled = false;

        _MenuTitleUI.text = title;

        InitMenuItemsVisualElements(names, defaultItemIndex);

        _RadialMenuPanel.SetActive(true);
        SelectedItemIndex = 0;

        _IsInitializing = false;

        StartCoroutine(DoRadialMenu());
    }

    private IEnumerator DoRadialMenu()
    {
        if (_IsOpen)
            throw new Exception("Cannot display the radial menu, because one is already displayed!");

        if (_MenuItems == null)
            throw new Exception("The menu items list is null!");
        else if (_MenuItems.Count == 0)
            throw new Exception("The menu items list is null!");



        _IsOpen = true;
        Time.timeScale = 0; // Pause the game.

        while (true)
        {
            DoUIChecks();

            if (MenuConfirmed || MenuCancelled)
                break;

            yield return null;
        }


        /*
        if (MenuConfirmed)
            Debug.Log($"Radial Menu \"{_MenuTitleUI.text}\": Selected Item = \"{SelectedItemName}\"");
        else if (MenuCancelled)
            Debug.Log($"Radial Menu  \"{_MenuTitleUI.text}\": Selection cancelled.");
        */


        // Wait slightly to stop the player character from receiving the same button press that closed the menu, which will make him jump or attack upon closing the menu.
        yield return new WaitForSecondsRealtime(0.1f);

        _RadialMenuPanel.SetActive(false);
        _IsOpen = false;
        Time.timeScale = 1; // Unpause the game.
    }

    private void DoUIChecks()
    {
        if (_IsInitializing)
            return;


        GetSelectedItemIndex();

        if (_Input.UI_Confirm && !MenuCancelled)
        {
            if (SelectedItemName != "Cancel")
                MenuConfirmed = true;
            else
                MenuCancelled = true;
        }
        else if (_Input.UI_Cancel && !MenuConfirmed)
        {
            MenuCancelled = true;
        }

    }

    private void GetSelectedItemIndex()
    {
        Vector2 dir = _Input.UI_Navigate;


        // Filter out inputs that are zero. Otherwise it will reselect the top item
        // in the menu as soon as you let go of the joystick.
        if (dir == Vector2.zero)
            return;


        float angle = Vector3.Angle(Vector3.down, dir);

        if (dir.x < 0)
            angle = 360 - angle;


        int index = (int)Mathf.Round((float)(angle / _MenuItemSizeInDegrees));

        // If the index is equal to the number of items, it means angle was 360, which is the same as 0. So reset index to 0 so it points to the first item as it should.
        if (index >= _MenuItems.Count)
            index = 0;


        /*
        if (SelectedIndex < 0)
            Debug.Log($"Angle: {angle}    Index: {index}    Item Menu Slice Size: {_MenuItemSizeInDegrees}    Selected: NONE");
        else
            Debug.Log($"Angle: {angle}    Index: {index}    Item Menu Slice Size: {_MenuItemSizeInDegrees}    Selected: {_MenuItems[SelectedIndex].Name}");
        */


        if (index != SelectedItemIndex)
        {
            _PrevSelectedItemIndex = SelectedItemIndex;
            SelectedItemIndex = index;

            UpdateSelection();
        }

    }

    private void UpdateSelection()
    {
        if (SelectedItemIndex >= 0)
            _MenuItems[SelectedItemIndex].Highlight();

        if (_PrevSelectedItemIndex >= 0)
            _MenuItems[_PrevSelectedItemIndex].Unhighlight();


        OnSelectionChanged?.Invoke(gameObject);
    }


    private void InitMenuItemsVisualElements(string[] names, int defaultItemIndex)
    {
        int menuItemCount = names.Length + 1;


        // Calculate the size of the pie slice each menu item occupies in the radial menu.
        // The plus one takes into account the Cancel item that is added to the end of the menu automatically below.
        _MenuItemSizeInDegrees = 360 / menuItemCount;

        Quaternion q = Quaternion.identity;
        float angle = 0;

        float menuItemVerticalOffset = CalculateMenuItemsVerticalOffset();

        // Get the number of items we need to iterate through (whichever one is higher is what we need).
        // The plus one takes into account the Cancel item that is added to the end of the menu automatically below.
        int length = Mathf.Max(_MenuItems.Count, menuItemCount);


        // Iterate through the list of menu items.
        for (int i = 0; i < length; i++)
        {
            // If there isn't a next menu item, create it.
            if (i >= _MenuItems.Count)
                CreateMenuItem();

            // If the menu item is within the length of the passed in list of items, then set it up.
            // This is >= instead of > to take into account the "Cancel" menu item that is added automatically.
            if (i < menuItemCount)
            {
                angle = _MenuItemSizeInDegrees * i;
                q.eulerAngles = new Vector3(0, 0, -angle); // We negate the angle so the menu items are placed around the center going clockwise rather than the inverse.
                Vector3 offset = q * (Vector3.up * Radius);

                // Position the menu item.
                // The last part of this line in ()s shifts the menu items down by half the height of the title bar so they appear centered in the area beneath it.
                _MenuItems[i].transform.position = _RadialMenuPanel.transform.position + offset + new Vector3(0, menuItemVerticalOffset, 0);

                // Debug.Log($"Menu Pos: {RadialMenuPanel.transform.position}    Offset: {offset}    Menu Item Pos: {_MenuItems[i].UI.transform.position}");


                // Remove highlighting if this menu item is highlighted.
                _MenuItems[i].Unhighlight();

                // Set the name of the menu item.
                if (i < names.Length)
                    _MenuItems[i].Name = names[i];
                else if (i == names.Length)
                    _MenuItems[i].Name = "Cancel";


                // If this is the first menu item, set it as the default.
                if (i == defaultItemIndex)
                {
                    _MenuItems[i].IsDefaultMenuItem = true;
                    _MenuItems[i].Highlight();
                }
                else
                {
                    _MenuItems[i].IsDefaultMenuItem = false;
                }


                // Make sure this menu item's UI is enabled.
                _MenuItems[i].Show(true);
            }
            else
            {
                _MenuItems[i].Name = "Unused";

                // This is an extra menu item that won't be used this time around, so disable it.
                _MenuItems[i].Show(false);
            }

        } // end for i

    }

    /// <summary>
    /// Offsets the menu items vertically so they are centered between the title bar and the bottom bar.
    /// </summary>
    /// <returns></returns>
    private float CalculateMenuItemsVerticalOffset()
    {
        float titleBarHeight = _MenuTitleUI.rectTransform.rect.height;
        float bottomBarHeight = _MenuBottomBarUI.rectTransform.rect.height;

        return ((-titleBarHeight) + (bottomBarHeight)) / 2;
    }

    private void CreateMenuItem()
    {
        GameObject newMenuItem = Instantiate(RadialMenuItemPrefab,
                                             Vector3.zero,
                                             Quaternion.identity,
                                             _RadialMenuItemsParent.transform);

        _MenuItems.Add(newMenuItem.GetComponent<RadialMenuItem>());
    }



    public bool MenuConfirmed { get; private set; }
    public bool MenuCancelled { get; private set; }

    public Color32 MenuItemTextColor
    {
        get { return RadialMenuItem.TextColor; }
        set
        {
            RadialMenuItem.TextColor = value;

            foreach (RadialMenuItem item in _MenuItems)
                item.SetColor(value);
        }
    }

    public Color32 MenuItemHighlightTextColor
    {
        get { return RadialMenuItem.TextHighlightColor; }
        set
        {
            RadialMenuItem.TextHighlightColor = value;
            _MenuItems[SelectedItemIndex].SetHighlightColor(value);
        }
    }
    
    public int SelectedItemIndex { get; private set; }
    public string SelectedItemName { get { return _MenuItems[SelectedItemIndex].Name; } }

    public string BottomBarText
    {
        get { return _MenuBottomBarUI.text; }
        set { _MenuBottomBarUI.text = value; }        
    }

    public Color BottomBarTextColor
    {
        get
        {
            return _MenuBottomBarTextColor;
        }
        set
        {
            if (_MenuBottomBarUI)
            {
                _MenuBottomBarTextColor = value;
                _MenuBottomBarUI.color = _MenuBottomBarTextColor;
            }
        }
    }

    public Color TitleTextColor
    {
        get
        {
            return _MenuTitleTextColor;
        }
        set
        {
            if (_MenuTitleUI)
            {
                _MenuTitleTextColor = value;
                _MenuTitleUI.color = _MenuTitleTextColor;
            }
        }
    }
}
