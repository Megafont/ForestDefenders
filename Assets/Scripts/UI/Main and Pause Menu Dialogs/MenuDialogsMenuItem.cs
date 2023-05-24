using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MenuDialogsMenuItem : MonoBehaviour, IPointerEnterHandler
{
    public delegate void MenuDialogItem_EventHandler(GameObject sender);

    public event MenuDialogItem_EventHandler OnMouseEnter;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnMouseEnter?.Invoke(gameObject);
    }
    

}
