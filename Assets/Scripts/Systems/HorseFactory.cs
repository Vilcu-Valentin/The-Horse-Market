using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class HorseFactory 
{
    // cache all of them on first access
    static readonly VisualDef[] _allVisuals;
    static readonly TraitDef[] _allTraits;
    static readonly TierDef[] _allTiers;

    // static ctor runs once before any method or property
    static HorseFactory()
    {
        _allVisuals = Resources.LoadAll<VisualDef>("Visuals");
        if (_allVisuals.Length == 0)
            Debug.LogError("No VisualDefs found in Resources/Visuals!");

        _allTraits = Resources.LoadAll<TraitDef>("Traits");
        if (_allTraits.Length == 0)
            Debug.LogError("No TraitDefs found in Resources/Traits!");

        _allTiers = Resources.LoadAll<TierDef>("Tiers");
        if (_allTiers.Length == 0)
            Debug.LogError("No TierDefs found in Resources/Tiers!");
    }

    /// <summary>
    /// Used in generating a random horse, wrapper for CreateFoal
    /// Used by either the CrateSystem or the ShopSystem
    /// </summary>
    /// <param name="tier"></param>
    /// <param name="traitsNo"></param>
    /// <returns>A fully generated horse</returns>
    public static Horse CreateRandomHorse(TierDef tier, int traitsNo)
    {
        var visualChoices = _allVisuals.Select(v => (item: v, ticket: v.rarityTickets));
        var selectedVisual = WeightedSelector<VisualDef>.Pick(visualChoices);

        List<TraitDef> selectedTraits = new List<TraitDef>();
        for (int i = 0; i < traitsNo; i++)
        {
            var traitsChoices = _allTraits.Select(t => (item: t, ticket: t.rarityTickets));
            selectedTraits.Add(WeightedSelector<TraitDef>.Pick(traitsChoices));
        }

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
