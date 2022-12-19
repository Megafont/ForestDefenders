using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class LevelUpMenuItem : MonoBehaviour, IPointerUpHandler
{
    public delegate void MainMenuItem_EventHandler(GameObject sender);

    public event MainMenuItem_EventHandler OnMouseUp;


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
