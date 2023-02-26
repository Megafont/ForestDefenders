using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public static class TechEnabler
{
    public static void EnableTech(string techName)
    {
        switch (techName)
        {
            case "Repair Damaged Buildings":
                GameManager.Instance.VillageManager_Villagers.EnableVillagersHealBuildings();
                break;

            // Building Unlocks
            case "Farm":
            case "Medium House":
            case "Wood Walls":
            case "Stone Walls":
            case "Spike Tower":
            case "Mage Tower":
            case "Wooden Bridge":
                // For buildings we do nothing since the menu simply asks the tech tree if they are researched
                // or not. If so, they are displayed in the menu. So we simpyly break here.
                break;

            default:
                Debug.LogWarning($"Cannot enable the unknown tech \"{techName}\"");
                break;
        }
    }

    
}
