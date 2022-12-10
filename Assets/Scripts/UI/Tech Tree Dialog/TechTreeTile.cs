using System.Collections;
using System.Collections.Generic;


using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


public class TechTreeTile : MonoBehaviour, IPointerEnterHandler
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

    public void UpdateUIColors()
    {
        if (_Button == null)
            return;


        ColorBlock block = _Button.colors;


        Color32 color = _ParentDialog.LockedColor;
        if (_TileData.IsResearched)
        {
            color = _ParentDialog.ResearchedColor;
            _XPCostText.text = null;
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
    }

    public void SetResearchedFlag(bool value)
    {
        _TileData.IsResearched = value;

        UpdateUIColors();
    }

    public void SetTileData(TechTreeDialog parentDialog, TechTreeTileData tileData)
    {
        _ParentDialog = parentDialog;
        _TileData = tileData;
    }

    private void UpdateGUI()
    {
        _TitleText.text = !_TileData.IsLocked ? _TileData.Title : _ParentDialog.LockedTileDescriptionText;

        _XPCostText.text = $"Cost: {_TileData.XPCost} XP";
    }

    
    public TechTreeTileData TileData { get { return _TileData; } }
}

