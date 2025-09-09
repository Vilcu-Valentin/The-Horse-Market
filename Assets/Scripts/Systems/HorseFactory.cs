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

        var selectedVisual = TraitSystem.PickVisual();
        var selectedTraits = TraitSystem.PickTraits(traitsNo);

        return CreateFoal(tier, selectedVisual, selectedTraits);
    }

    /// <summary>
    /// Used in building a custom horse
    /// </summary>
    /// <param name="tier"></param>
    /// <param name="visual"></param>
    /// <param name="traits"></param>
    /// <returns>A fully custom generated horse</returns>
    public static Horse CreateFoal(TierDef tier, VisualDef visual, List<TraitDef> traits, int ascension = 0)
    {
        return new Horse(Guid.NewGuid(), tier, visual, traits, ascension);
    }

    /// <summary>
    /// Used in horse ascension, it copies all the parameters of the old horse, destroys it and creates a new copy with ascensions properties and Tier I
    /// </summary>
    /// <param name="horse"></param>
    /// <returns>An ascended horse</returns>
    public static Horse AscendHorse(Horse horse)
    {
        int asc = horse.ascensions + 1;
        return new Horse(Guid.NewGuid(), HorseMarketDatabase.Instance._allTiers[0], horse.Visual, horse.Traits.ToList(), asc)
        {
            horseName = horse.horseName,
            favorite = horse.favorite
        };
    } 
}
