using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RadialMenuItem : MonoBehaviour, IPointerEnterHandler
{
    public static float DefaultTextSize;
    public static float ImageSizeIncreaseOnSelection = 50;
    public static Color ThumbnailUnhighlightedColor = new Color32(100, 100, 100, 255);



    private string _Name;

    private TMP_Text _UI_TMPComponent;
    private Button _UI_ButtonComponent;
    private Image _UI_Thumbnail;

    private Vector2 _DefaultThumbnailSize;
    private Vector3 _DefaultThumbnailPosition;



    public delegate void RadialMenuItem_EventHandler(GameObject sender);

    public event RadialMenuItem_EventHandler OnMouseClick;
    public event RadialMenuItem_EventHandler OnMouseEnter;





    // Start is called before the first frame update
    void Start()
    {
        _UI_TMPComponent = GetComponent<TMP_Text>();
        _UI_ButtonComponent = GetComponent<Button>();

        _UI_Thumbnail = transform.Find("Icon").GetComponent<Image>();
        _DefaultThumbnailSize = _UI_Thumbnail.rectTransform.sizeDelta;
        _DefaultThumbnailPosition = _UI_Thumbnail.rectTransform.localPosition;

        DefaultTextSize = _UI_TMPComponent.fontSize;

        _UI_TMPComponent.text = _Name;

        if (!NewItemIsDefaultMenuItem)
            Unhighlight();
        else
            Highlight();

        
        IsInited = true;
    }

    public void SetText(string text)
    {
        if (_UI_TMPComponent == null)
            return;

        _UI_TMPComponent.text = text;

        // Set the game object name.
        name = $"Radial Menu Item \"{_Name}\" (TMP)";
    }

    public void SetThumbnail(Sprite thumbnail)
    {
        if (_UI_Thumbnail == null)
            _UI_Thumbnail = transform.Find("Icon").GetComponent<Image>();


        if (thumbnail)
        {
            //Debug.Log($"Set Thumb: {_Name}");
            _UI_Thumbnail.enabled = true;
            _UI_Thumbnail.sprite = thumbnail;
        }
        else
        {
            //Debug.Log($"Set Thumb null: {_Name}");
            _UI_Thumbnail.enabled = false;
        }

    }

    public void Show(bool show = true)
    {
        if (_UI_TMPComponent == null)
            return;

        gameObject.SetActive(show);
    }

    public void Highlight()
    {
        if (_UI_TMPComponent == null)
            return;


        _UI_TMPComponent.faceColor = _UI_ButtonComponent.colors.pressedColor;
        _UI_TMPComponent.fontSize = DefaultTextSize + 4;

        _UI_Thumbnail.color = Color.white;
        _UI_Thumbnail.rectTransform.sizeDelta = _DefaultThumbnailSize + new Vector2(ImageSizeIncreaseOnSelection, ImageSizeIncreaseOnSelection);
        _UI_Thumbnail.rectTransform.localPosition = _DefaultThumbnailPosition + new Vector3(0, ImageSizeIncreaseOnSelection, 0);
    }

    public void Unhighlight()
    {
        if (_UI_TMPComponent == null)
            return;


        _UI_TMPComponent.fontSize = DefaultTextSize;
        _UI_TMPComponent.faceColor = _UI_ButtonComponent.colors.normalColor;

        _UI_Thumbnail.color = ThumbnailUnhighlightedColor;
        _UI_Thumbnail.rectTransform.sizeDelta = _DefaultThumbnailSize;
        _UI_Thumbnail.rectTransform.localPosition = _DefaultThumbnailPosition;
    }



    public void OnPointerEnter(PointerEventData eventData)
    {
        OnMouseEnter?.Invoke(gameObject);
    }

    public void OnButtonClick()
    {
        OnMouseClick?.Invoke(gameObject);
    }




    private void SetNormalColor(Color32 newColor)
    {
        ColorBlock cBlock = _UI_ButtonComponent.colors;
        cBlock.normalColor = newColor;
        _UI_ButtonComponent.colors = cBlock;


        if (_UI_TMPComponent == null)
            return;


        if (!IsHighlighted)
            _UI_TMPComponent.color = newColor;
    }

    private void SetHighlightColor(Color32 newColor)
    {
        if (_UI_TMPComponent == null)
            return;


        ColorBlock cBlock = _UI_ButtonComponent.colors;
        cBlock.pressedColor = newColor;
        _UI_ButtonComponent.colors = cBlock;

        if (IsHighlighted)
            _UI_TMPComponent.color = newColor;
    }



    // This property is only important when a new RadialMenuItem object is created. It tells us whether or not this menu item should highlight itself when the Start() method is called.
    // I couldn't get the default menu item to highlight at first, because this GameObject isn't initialized yet when the RadialMenu.InitMenuItemsVisualElements() function
    // creates a new menu item on the fly and sets it up. So I added this property so the new menu item can handle updating itself in the Start() that first time.
    // The Name property exists for the same reason, but for setting the menu item's text.
    public bool NewItemIsDefaultMenuItem { get; set; }

    public bool IsHighlighted 
    { 
        get { return _UI_TMPComponent.color == _UI_ButtonComponent.colors.pressedColor; } 
    }

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

    public Color TextNormalColor
    {
        get { return _UI_ButtonComponent.colors.normalColor; }
        set
        {
            SetNormalColor(value);
        }
    }

    public Color TextHighlightColor
    {
        get { return _UI_ButtonComponent.colors.pressedColor; }
        set
        {
            SetNormalColor(value);
        }
    }

    public bool HasThumbnail
    {
        get 
        {
            if (_UI_Thumbnail == null)
                return false;
            else
                return _UI_Thumbnail.enabled; 
        }
    }

}
