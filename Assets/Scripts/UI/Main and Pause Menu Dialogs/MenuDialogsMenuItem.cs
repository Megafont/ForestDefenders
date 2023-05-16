using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MenuDialogsMenuItem : MonoBehaviour, IPointerUpHandler
{
    public delegate void MenuDialogItem_EventHandler(GameObject sender);

    public event MenuDialogItem_EventHandler OnMouseUp;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    public void OnPointerUp(PointerEventData eventData)
    {
        OnMouseUp?.Invoke(gameObject);
    }
    

}
