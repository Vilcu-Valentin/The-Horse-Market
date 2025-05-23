using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TraitSystem: MonoBehaviour
{
    public static TraitSystem Instance { get; private set; }

    public List<VisualDef> _allVisuals;
    public List<TraitDef> _allTraits;
    public List<TierDef> _allTiers;

    void Awake()
    {
        if (Instance == null) { Instance = this;}
        else Destroy(gameObject);
    }

    /// <summary>
    /// Takes a desired number of traits and creates a list of traits that doesn't conflict with eachother
    /// </summary>
    /// <param name="traitsNo">Number of desired trates</param>
    /// <returns>A list of traits (could be less then the traitsNo depending on the conflicts)</returns>
    public List<TraitDef> PickTraits(int traitsNo)
    {
        var selectedTraits = new List<TraitDef>(); 
        int traitsLeft = traitsNo;

        while (traitsLeft > 0)
        {
            // Build the pool of allowed traits:
            var allowed = _allTraits
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
    /// Takes a desired number of traits, and an already existing list of traits and creates a list of traits that doesn't conflict with eachother
    /// </summary>
    /// <param name="traits"></param>
    /// <param name="traitsNo"></param>
    /// <returns>A trait that doesn't conflict with the already exisitng list of traits</returns>
    public TraitDef PickTraits(List<TraitDef> traits)
    {
        var selectedTraits = traits;

        // Build the pool of allowed traits:
        var allowed = _allTraits
            // 1. Never re-pick a trait you’ve already chosen
            .Where(t => !selectedTraits.Contains(t))
            // 2. And it must have no conflict *in either direction* with any chosen trait
            .Where(t => !selectedTraits
                .Any(chosen => chosen.IsConflict(t) || t.IsConflict(chosen)))
            .Select(t => (item: t, ticket: t.rarityTickets))
            .ToList();

        if(allowed.Count == 0)
            return null;

        // Pick one weighted by rarityTickets
        var pick = WeightedSelector<TraitDef>.Pick(allowed);

        return pick;
    }

    /// <summary>
    /// Picks a random(weighted) visual from all available visuals
    /// </summary>
    /// <returns></returns>
    public VisualDef PickVisual()
    {
        var visualChoices = _allVisuals.Select(v => (item: v, ticket: v.rarityTickets));
        return WeightedSelector<VisualDef>.Pick(visualChoices);
    }
}
