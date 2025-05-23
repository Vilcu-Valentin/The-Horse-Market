using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class HorseFactory 
{
    /// <summary>
    /// Used in generating a random horse, wrapper for CreateFoal
    /// Used by either the CrateSystem or the ShopSystem
    /// </summary>
    /// <param name="tier"></param>
    /// <param name="traitsNo"></param>
    /// <returns>A fully generated horse</returns>
    public static Horse CreateRandomHorse(TierDef tier, int traitsNo)
    {

        var selectedVisual = TraitSystem.Instance.PickVisual();
        var selectedTraits = TraitSystem.Instance.PickTraits(traitsNo);

        return CreateFoal(tier, selectedVisual, selectedTraits);
    }

    /// <summary>
    /// Used in building a custom horse
    /// </summary>
    /// <param name="tier"></param>
    /// <param name="visual"></param>
    /// <param name="traits"></param>
    /// <returns>A fully custom generated horse</returns>
    public static Horse CreateFoal(TierDef tier, VisualDef visual, List<TraitDef> traits)
    {
        return new Horse(Guid.NewGuid(), tier, visual, traits);
    }
}
