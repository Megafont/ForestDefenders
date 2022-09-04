using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;


public class RadialMenu : MonoBehaviour
{
    public string[] MenuItems;

    public GameObject RadialMenuPanel;
    public GameObject RadialMenuItemPrefab;

    [Min(1)]
    public float Radius = 100;


    public delegate void ItemSelectedCallback(int selectedMenuItemIndex);
    

    private static bool _IsOpen;




    // Start is called before the first frame update
    void Start()
    {
        RadialMenuPanel.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ShowRadialMenu()
    {
        StartCoroutine("DoRadialMenu");
    }

    public IEnumerator DoRadialMenu()
    {
        if (_IsOpen)
            throw new Exception("Cannot display the radial menu, because one is already displayed!");

        if (RadialMenuPanel == null)
            throw new Exception("The radial menu panel is null!");

        if (MenuItems == null)
            throw new Exception("The menu items list is null!");
        else if (MenuItems.Length == 0)
            throw new Exception("The menu items list is null!");


        CreateMenuItemsVisualElements();

        RadialMenuPanel.SetActive(true);
        _IsOpen = true;


        while (true)
        {
            yield return null;
        }
        //yield return new WaitForSeconds(2.0f);

        RadialMenuPanel.SetActive(false);
        _IsOpen = false;

        // Cleanup menu item objects.
        Utils.DestroyAllChildGameObjects(RadialMenuPanel);

    }

    private void CreateMenuItemsVisualElements()
    {
        float menuItemSizeInDegrees = 360 / MenuItems.Length;

        Quaternion q = Quaternion.identity;
        float angle = 0;
        for (int i = 0; i < MenuItems.Length; i++)
        {
            angle = menuItemSizeInDegrees * i;
            q.eulerAngles = new Vector3(0, 0, -angle); // We negate the angle so the menu items are placed around the center going clockwise rather than the inverse.
            Vector3 offset = q * (Vector3.up * Radius);

            // Debug.Log($"Pos: {RadialMenuPanel.transform.position}    Offset: {offset}");

            GameObject obj = Instantiate(RadialMenuItemPrefab, RadialMenuPanel.transform.position + offset, Quaternion.identity, RadialMenuPanel.transform);
            obj.GetComponent<TMP_Text>().text = MenuItems[i];
        }

    }

}
