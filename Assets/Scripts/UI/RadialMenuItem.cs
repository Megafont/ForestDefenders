using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;


public class RadialMenuItem : MonoBehaviour
{
    public static Color DefaultTextColor = Color.gray;
    public static Color HighlightTextColor = Color.white;
    public static float DefaultTextSize;

    // This property is only important when a new RadialMenuItem object is created. It tells us whether or not this menu item should highlight itself when the Start() method is called.
    // I couldn't get the default menu item to highlight at first, because this GameObject isn't initialized yet when the RadialMenu.InitMenuItemsVisualElements() function
    // creates a new menu item on the fly and sets it up. So I added this property so the new menu item can handle updating itself in the Start() that first time.
    // Then name property exists for the same reason, but for setting the menu item's text.
    public bool IsDefaultMenuItem;
    public string Name;


    public bool IsInited { get; private set; }
    
    
    private TMP_Text _UI_Object;


    // Start is called before the first frame update
    void Start()
    {
        _UI_Object = GetComponent<TMP_Text>();
        DefaultTextSize = _UI_Object.fontSize;

        _UI_Object.text = Name;

        if (!IsDefaultMenuItem)
            Unhighlight();
        else
            Highlight();

        IsInited = true;
    }


    public void SetText(string text)
    {
        if (_UI_Object == null)
            return;

        _UI_Object.text = text;
    }

    public void Highlight()
    {
        if (_UI_Object == null)
            return;


        _UI_Object.faceColor = HighlightTextColor;
        _UI_Object.fontSize = DefaultTextSize + 4;
    }

    public void Unhighlight()
    {
        if (_UI_Object == null)
            return;


        _UI_Object.fontSize = DefaultTextSize;
        _UI_Object.faceColor = DefaultTextColor;
    }


}
