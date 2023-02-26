using System.Collections;
using System.Collections.Generic;


using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


public class TechTreeTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TechTreeTileData _TileData;

    private Button _Button;
    private TMP_Text _TitleText;
    private TMP_Text _XPCostText;
    private Image _IconImage;

    private TechTreeDialog _ParentDialog;



    public delegate void TechTreeTile_EventHandler(TechTreeTile sender);

    public event TechTreeTile_EventHandler OnClick;
    public event TechTreeTile_EventHandler OnMouseOver;
    public event TechTreeTile_EventHandler OnMouseExit;



    // Start is called before the first frame update
    void Start()
    {
        _Button = GetComponent<Button>();
        _TitleText = transform.Find("Title Text (TMP)").GetComponent<TMP_Text>();
        _XPCostText = transform.Find("XP Cost Text (TMP)").GetComponent<TMP_Text>();
        _IconImage = GetComponentInChildren<Image>();

        UpdateUIColors();

        UpdateGUI();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TileClickedHandler()
    {
        OnClick?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnMouseOver?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData) 
    {
        OnMouseExit?.Invoke(this);
    }

    public void UpdateUIColors()
    {
        if (_Button == null)
            return;


        ColorBlock block = _Button.colors;


        Color32 color = _ParentDialog.LockedColor;
        if (_TileData.IsResearched)
        {
            color = _ParentDialog.ResearchedColor;
        }
        else
        {
            _XPCostText.color = _TileData.XPCost <= _ParentDialog.AvailableXP ? _ParentDialog.TileXPTextColor : _ParentDialog.TileNotEnoughXPTextColor;

            if (_TileData.IsLocked)
                color = _ParentDialog.LockedColor;
            else
                color = _ParentDialog.UnlockedColor;                
        }


        block.normalColor = color;
        block.disabledColor = color;
        
        block.selectedColor = _ParentDialog.SelectedColor;
        block.highlightedColor = _ParentDialog.HighlightColor;

        _Button.colors = block;
    }

    public void SetLockedFlag(bool value)
    {
        _TileData.IsLocked = value;

        UpdateUIColors();
        UpdateGUI();
    }

    public void SetResearchedFlag(bool value)
    {
        _TileData.IsResearched = value;

        UpdateUIColors();
        UpdateGUI();
    }

    public void SetTileData(TechTreeDialog parentDialog, TechTreeTileData tileData)
    {
        _ParentDialog = parentDialog;
        _TileData = tileData;
    }

    private void UpdateGUI()
    {
        _TitleText.text = !_TileData.IsLocked ? _TileData.Title : _ParentDialog.LockedTileDescriptionText;

        _XPCostText.text = !_TileData.IsResearched ? $"Cost: {_TileData.XPCost} XP" : null; // Display the XP only if the tech item has NOT been researched yet.
    }

    
    public TechTreeTileData TileData { get { return _TileData; } }
}

