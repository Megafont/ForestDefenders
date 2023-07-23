using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public static class Utils_UI
{
    public static int GetIndexOfMenuItem(Transform parentMenu, Transform child)
    {
        int index = 0;
        foreach (Transform t in parentMenu)
        {
            if (t == child)
                return index;

            index++;
        }


        return -1;
    }

    public static int GetSelectedMenuItemIndex(Transform parentMenu)
    {
        int index = -1;

        if (EventSystem.current == null)
            return index;

        if (EventSystem.current.currentSelectedGameObject == null)
            return index;


        foreach (Transform t in parentMenu)
        {
            if (t == EventSystem.current.currentSelectedGameObject.transform)
                return index;

            index++;
        }


        return index;
    }

    public static void SelectMenuItem(Transform parentMenu, int index)
    {
        // Tell the Unity EventSystem to select the correct button.
        EventSystem.current.SetSelectedGameObject(parentMenu.GetChild(index).gameObject);
    }

    public static Button SelectFirstButtonInPane(Transform parentPane)
    {
        Button[] buttons = parentPane.GetComponentsInChildren<Button>();

        EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);

        return buttons[0];
    }

}
