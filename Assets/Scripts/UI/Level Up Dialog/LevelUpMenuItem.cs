using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;


public class LevelUpMenuItem : MonoBehaviour, IPointerEnterHandler
{
    public delegate void MainMenuItem_EventHandler(GameObject sender);

    public event MainMenuItem_EventHandler OnMouseEnter;



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
