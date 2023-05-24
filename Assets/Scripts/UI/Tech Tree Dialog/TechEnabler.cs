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
            case "Large Garden":
            case "Farm":
            case "Medium House":
            case "Large House":
            case "Wood Walls":
            case "Stone Walls":
            case "Turret":
            case "Ice Turret":
            case "Mage Tower":
            case "Wood Bridge (10m)":
            case "Wood Bridge (20m)":
                // For buildings we do nothing since the building menu simply asks the tech tree if they are researched
                // or not. If so, they are displayed in the menu.
                break;

            default:
                Debug.LogWarning($"Cannot enable the unknown tech \"{techName}\"");
                break;
        }
    }

    
}
