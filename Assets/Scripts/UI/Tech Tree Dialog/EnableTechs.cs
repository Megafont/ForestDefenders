using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public static class EnableTechs
{
    public static void EnableTech(string techName)
    {
        switch (techName)
        {
            case "Repair Damaged Buildings":
                GameManager.Instance.VillageManager_Villagers.EnableVillagersHealBuildings();
                break;

            default:
                throw new Exception($"Cannot enable the unknown tech \"{techName}\"");
        }
    }

    
}
