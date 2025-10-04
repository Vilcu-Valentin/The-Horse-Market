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
    public static Horse CreateRandomHorse(TierDef tier, int traitsNo, int ascension = 0)
    {
        bool useAsc = false;
        if(ascension > 0)
            useAsc = true;

        var selectedVisual = TraitSystem.PickVisual();
        var selectedTraits = TraitSystem.PickTraits(traitsNo, useAsc);

        return CreateFoal(tier, selectedVisual, selectedTraits, ascension);
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

    public static Horse CreateCustomHorse(Horse horse)
    {
        return new Horse(Guid.NewGuid(), horse.Tier, horse.Visual,horse.Traits.ToList(), horse.ascensions, horse.horseName);
    }

    /// <summary>
    /// Used in horse ascension: copies all the parameters of the old horse, 
    /// destroys it, and creates a new copy with ascension properties and Tier I.
    /// </summary>
    /// <param name="horse">The horse to ascend</param>
    /// <returns>A new ascended horse</returns>
    public static Horse AscendHorse(Horse horse)
    {
        int asc = horse.ascensions + 1;
        var ascensionTrait = TraitSystem.PickAscensionTrait(horse.Traits.ToList());

        var allTraits = horse.Traits.ToList();
        if (ascensionTrait != null)
            allTraits.Add(ascensionTrait);

        Debug.Log("Ascension!!" + ascensionTrait.DisplayName);
        return new Horse(Guid.NewGuid(), HorseMarketDatabase.Instance._allTiers[0], horse.Visual, allTraits, asc)
        {
            horseName = horse.horseName,
            favorite = horse.favorite
        };
    }
}
