using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;


public class RadialMenuItem : MonoBehaviour
{
    public static Color TextColor = Color.gray;
    public static Color TextHighlightColor = Color.white;
    public static float DefaultTextSize;

    // This property is only important when a new RadialMenuItem object is created. It tells us whether or not this menu item should highlight itself when the Start() method is called.
    // I couldn't get the default menu item to highlight at first, because this GameObject isn't initialized yet when the RadialMenu.InitMenuItemsVisualElements() function
    // creates a new menu item on the fly and sets it up. So I added this property so the new menu item can handle updating itself in the Start() that first time.
    // Then name property exists for the same reason, but for setting the menu item's text.
    public bool IsDefaultMenuItem;



    public bool IsInited { get; private set; }

    public string Name 
    { 
        get
        {
            return _Name;
        }
        set
        {
            _Name = value;
            SetText(_Name);
        }
    }


    private string _Name;

       
        
    private TMP_Text _UI_Object;


    // Start is called before the first frame update
    void Start()
    {
        _UI_Object = GetComponent<TMP_Text>();
        DefaultTextSize = _UI_Object.fontSize;

        _UI_Object.text = _Name;

        if (!IsDefaultMenuItem)
            Unhighlight();
        else
            Highlight();

        IsInited = true;
    }


    public void SetColor(Color32 newColor)
    {
        TextColor = newColor;


        if (_UI_Object == null)
            return;

        if (_UI_Object.color != TextHighlightColor)
            _UI_Object.color = newColor;
    }

    public void SetHighlightColor(Color32 newColor)
    {
        bool isHighlighted = _UI_Object.color == TextHighlightColor;
        
        TextHighlightColor = newColor;


        if (_UI_Object == null)
            return;

        if (isHighlighted)
            _UI_Object.color = newColor;
    }

    public void SetText(string text)
    {
        if (_UI_Object == null)
            return;

        _UI_Object.text = text;

        // Set the game object name.
        name = $"Radial Menu Item \"{_Name}\" (TMP)";
    }

    public void Show(bool show = true)
    {
        if (_UI_Object == null)
            return;

        gameObject.SetActive(show);
    }

    public void Highlight()
    {
        if (_UI_Object == null)
            return;


        _UI_Object.faceColor = TextHighlightColor;
        _UI_Object.fontSize = DefaultTextSize + 4;
    }

    public void Unhighlight()
    {
        if (_UI_Object == null)
            return;


        _UI_Object.fontSize = DefaultTextSize;
        _UI_Object.faceColor = TextColor;
    }


}
