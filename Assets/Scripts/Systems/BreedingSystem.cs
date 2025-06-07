using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static class BreedingSystem 
{
    public static void Breed(Horse parentA, Horse parentB) // also add a list of breeding items later
    {
        // Calculates upgrade odds
        Vector3 parentAOdds = Vector3.zero;
        Vector3 parentBOdds = Vector3.zero;
        Vector3 foalOdds = Vector3.zero;
        (parentAOdds.x, parentAOdds.y, parentAOdds.z) = CalculateUpgradeOdds(parentA);
        (parentBOdds.x, parentBOdds.y, parentBOdds.z) = CalculateUpgradeOdds(parentB);
        foalOdds.x = (parentAOdds.x + parentBOdds.x) / 2;
        foalOdds.y = (parentAOdds.y + parentBOdds.y) / 2;
        foalOdds.z = (parentAOdds.z + parentBOdds.z) / 2;

        // Selects new tier based on those odds
        float roll = Random.value;
        int delta;
        if (roll < foalOdds.x) delta = +1;
        else if (roll < foalOdds.x + foalOdds.y) delta = 0;
        else delta = -1;

        TierDef childTier = CalculateTier(parentA, parentB, delta);

        VisualDef childVisual = CalculateVisual(parentA, parentB);

        // Calculates trait inheritence
        List<TraitDef> childTraits = CalculateTraits(parentA, parentB);

        // Remove parents and add new foal to the player inventory
        SaveSystem.Instance.AddHorse(HorseFactory.CreateFoal(childTier, childVisual, childTraits));
        SaveSystem.Instance.RemoveHorse(parentA);
        SaveSystem.Instance.RemoveHorse(parentB);
    }

    public static (float baseUp, float baseSame, float baseDown) CalculateUpgradeOdds(Horse horse)
    {
        float _baseUp = horse.Tier.UpgradeChance;
        float _baseSame = horse.Tier.SameChance;
        float _baseDown = horse.Tier.DowngradeChance;

        float multUp = 1;
        float multDown = 1;
        foreach(TraitDef trait in horse.Traits)
        {
            multUp *= trait.UpgradeMult;
            multDown *= trait.DowngradeMult;
        }

        _baseUp *= multUp;
        _baseDown *= multDown;
        float totalRaw = _baseUp + _baseDown + _baseSame;
        return (_baseUp / totalRaw, _baseSame / totalRaw, _baseDown / totalRaw);
    }

    private static TierDef CalculateTier(Horse parentA, Horse parentB, int tierDelta)
    {
        int delta = tierDelta;
        int volatilityNo = Mathf.FloorToInt((CalculateTierVolatility(parentA) + CalculateTierVolatility(parentB)) / 2f);
        float volatilityMult = Mathf.Pow(volatilityNo, 0.68f);

        Vector3 deltaV = new Vector3(0.83f * volatilityMult, 0.14f * volatilityMult, 0.02f * volatilityMult);
        float rawDelta = deltaV.x + deltaV.y + deltaV.z;  
        deltaV = new Vector3(deltaV.x / rawDelta, deltaV.y / rawDelta, deltaV.z / rawDelta);

        float roll = Random.value;
        if (roll < deltaV.x) delta *= 1;
        else if (roll < deltaV.x + deltaV.y) delta *= 2;
        else delta *= 3;

        var allTiers = HorseMarketDatabase.Instance._allTiers;

        int newIndex = Mathf.Clamp(parentA.Tier.TierIndex + delta,
                             allTiers.First().TierIndex,
                             allTiers.Last().TierIndex);
        return allTiers[newIndex];
    }

    private static int CalculateTierVolatility(Horse parent)
    {
        int number = 0;
        foreach(TraitDef trait in parent.Traits)
        {
            if (trait.TierVolatility)
                number++;
        }

        return number;
    }

    private static VisualDef CalculateVisual(Horse parentA, Horse parentB)
    {
        Vector3 visualOdds = new Vector3(0.45f, 0.45f, 0.1f);
        float roll = Random.value;
        if (roll < visualOdds.x) return parentA.Visual;
        else if (roll < visualOdds.x + visualOdds.y) return parentB.Visual;
        else return TraitSystem.PickVisual();
    }

    private static List<TraitDef> CalculateTraits(Horse parentA, Horse parentB)
    {
        Dictionary<TraitDef, float> potentialTraits = new Dictionary<TraitDef, float>();
        List<TraitDef> foalTraits = new List<TraitDef>();

        foreach (TraitDef trait in parentA.Traits)
            potentialTraits[trait] = trait.BaseInheritChance;
        foreach (TraitDef trait in parentB.Traits)
        {
            if (potentialTraits.ContainsKey(trait))
                potentialTraits[trait] += trait.BaseInheritChance * 1.2f;
            else
                potentialTraits[trait] = trait.BaseInheritChance;
        }
     
        foreach(var kvp in potentialTraits)
        {
            if (Random.value < kvp.Value)
                foalTraits.Add(kvp.Key);
        }

        // Roll mutations if necessary
        float mutMultA = 1;
        float mutMultB = 1;
        foreach (TraitDef trait in parentA.Traits)
            mutMultA += trait.MutationMultiplier - 1f;
        foreach (TraitDef trait in parentB.Traits)
            mutMultB += trait.MutationMultiplier - 1f;

        float initialChance = parentA.Tier.MutationChance * ((mutMultA + mutMultB) * 0.5f);
        while(Random.value < initialChance)
        {
            var t = TraitSystem.PickTraits(foalTraits);
            if (t != null)
            {
                foalTraits.Add(t);
            }
            initialChance *= 0.5f;
        }

        return foalTraits;
    }
}
