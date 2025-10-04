using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TraitSystem
{

    /// <summary>
    /// Takes a desired number of traits and creates a list of traits that doesn't conflict with eachother
    /// </summary>
    /// <param name="traitsNo">Number of desired trates</param>
    /// <returns>A list of traits (could be less then the traitsNo depending on the conflicts)</returns>
    public static List<TraitDef> PickTraits(int traitsNo, bool useAscension = false)
    {
        var selectedTraits = new List<TraitDef>(); 
        int traitsLeft = traitsNo;

        while (traitsLeft > 0)
        {
            var traits = HorseMarketDatabase.Instance._allTraits;
            if (useAscension)
            {
                traits = traits.Concat(HorseMarketDatabase.Instance._allAscensionTraits).ToList();
            }

            // Build the pool of allowed traits:
            var allowed = traits
                // 1. Never re-pick a trait you’ve already chosen
                .Where(t => !selectedTraits.Contains(t))
                // 2. And it must have no conflict *in either direction* with any chosen trait
                .Where(t => !selectedTraits
                    .Any(chosen => chosen.IsConflict(t) || t.IsConflict(chosen)))
                .Select(t => (item: t, ticket: t.rarityTickets))
                .ToList();

            // If we ran out of non-conflicting options, bail out early
            if (allowed.Count == 0)
                break;

            // Pick one weighted by rarityTickets
            var pick = WeightedSelector<TraitDef>.Pick(allowed);

            // Record it
            selectedTraits.Add(pick);
            traitsLeft--;
        }

        return selectedTraits;
    }

    /// <summary>
    /// Takes an already existing list of traits and picks a trait that doesn't conflict with others
    /// </summary>
    /// <param name="traits"></param>
    /// <param name="useAscension">If true, includes ascension traits in the selection pool</param>
    /// <returns>A trait that doesn't conflict with the already existing list of traits</returns>
    public static TraitDef PickTraits(List<TraitDef> traits, bool useAscension = false)
    {
        var selectedTraits = traits;

        // Start with regular traits, optionally add ascension traits
        var traitPool = HorseMarketDatabase.Instance._allTraits.AsEnumerable();
        if (useAscension)
        {
            traitPool = traitPool.Concat(HorseMarketDatabase.Instance._allAscensionTraits);
        }

        // Build the pool of allowed traits:
        var allowed = traitPool
            // 1. Never re-pick a trait you've already chosen
            .Where(t => !selectedTraits.Contains(t))
            // 2. And it must have no conflict *in either direction* with any chosen trait
            .Where(t => !selectedTraits
                .Any(chosen => chosen.IsConflict(t) || t.IsConflict(chosen)))
            .Select(t => (item: t, ticket: t.rarityTickets))
            .ToList();

        if (allowed.Count == 0)
            return null;

        // Pick one weighted by rarityTickets
        var pick = WeightedSelector<TraitDef>.Pick(allowed);
        return pick;
    }

    public static TraitDef PickAscensionTrait(List<TraitDef> existingTraits)
    {
        // Source pool: ascension traits only
        var ascensionTraits = HorseMarketDatabase.Instance._allAscensionTraits;

        // Build the pool of allowed ascension traits
        var allowed = ascensionTraits
            // 1. Never re-pick a trait already chosen (in any category)
            .Where(t => !existingTraits.Contains(t))
            // 2. Must have no conflict in either direction with any already chosen trait
            .Where(t => !existingTraits
                .Any(chosen => chosen.IsConflict(t) || t.IsConflict(chosen)))
            .Select(t => (item: t, ticket: t.rarityTickets))
            .ToList();

        // Bail if nothing is possible
        if (allowed.Count == 0)
            return null;

        // Pick one weighted by rarityTickets
        return WeightedSelector<TraitDef>.Pick(allowed);
    }


    /// <summary>
    /// Picks a random(weighted) visual from all available visuals
    /// </summary>
    /// <returns></returns>
    public static VisualDef PickVisual()
    {
        var visualChoices = HorseMarketDatabase.Instance._allVisuals.Select(v => (item: v, ticket: v.rarityTickets));
        return WeightedSelector<VisualDef>.Pick(visualChoices);
    }
}
