using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class InventorySystem 
{
    public static void SellHorse(Horse horse)
    {
        SaveSystem.Instance.RemoveHorse(horse);
        SaveSystem.Instance.AddEmeralds(horse.GetCurrentPrice());
    }
}
