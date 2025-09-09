using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public static class BreedingSystem 
{
    public static Horse Breed(Horse parentA, Horse parentB, List<Item> items) // also add a list of breeding items later
    {
        bool preventParentConsumption = false;
        bool guaranteeUpgrade = false;
        bool guaranteeSameTier = false;
        bool guaranteeNoDowngrade = false;
        bool randomVisuals = false;
        float upgradeItemMultiplier = 1.0f;
        float downgradeItemMultiplier = 1.0f;
        float mutationItemMultiplier = 1.0f;
        int guaranteedTraits = 99;

        List<StatMod> statMods = new(4) { new() { Stat = StatType.Speed, Modifier = 1.0f},
        new() { Stat = StatType.Stamina, Modifier = 1.0f},
        new() { Stat = StatType.JumpHeight, Modifier = 1.0f},
        new() { Stat = StatType.Strength, Modifier = 1.0f},};

        // Calculate item modifiers
        foreach (var item in items) 
        {
            if (item == null)
                continue;

            if(item.Def.preventParentConsumption)
                preventParentConsumption = true; 
            if (item.Def.guaranteeUpgrade)
                guaranteeUpgrade = true;
            if(item.Def.guaranteeSameTier)
                guaranteeSameTier = true;
            if (item.Def.guaranteeNoDowngrade)
                guaranteeNoDowngrade = true;
            if (item.Def.randomVisuals)
                randomVisuals = true;

            if (item.Def.keepHigherThenQualityItems < guaranteedTraits)
                guaranteedTraits = item.Def.keepHigherThenQualityItems;

            upgradeItemMultiplier *= item.Def.UpgradeMult;
            downgradeItemMultiplier *= item.Def.DowngradeMult;
            mutationItemMultiplier *= item.Def.mutationMultiplier;

            for(int i = 0; i < statMods.Count; i++)
            {
                var sm = statMods[i];
                sm.Modifier *= item.Def.StatCapMods[i].Modifier;
                statMods[i] = sm; 
            }
        }

        // Calculates upgrade odds
        Vector3 foalOdds = Vector3.zero;
        (foalOdds.x, foalOdds.y, foalOdds.z) = CalculateFoalOdds(parentA, parentB, upgradeItemMultiplier, downgradeItemMultiplier);

        // Selects new tier based on those odds
        float roll = Random.value;
        int delta;
        if (roll < foalOdds.x) delta = +1;
        else if (roll < foalOdds.x + foalOdds.y) delta = 0;
        else delta = -1;

        if (guaranteeNoDowngrade)
            if (delta < 0) delta = 0;
        if (guaranteeSameTier)
            delta = 0;
        if (guaranteeUpgrade)
            delta = 1;

        TierDef childTier = CalculateTier(parentA, parentB, delta);

        VisualDef childVisual;
        if (!randomVisuals)
            childVisual = CalculateVisual(parentA, parentB);
        else
            childVisual = TraitSystem.PickVisual();

            // Calculates trait inheritence
            List<TraitDef> childTraits = CalculateTraits(parentA, parentB, guaranteedTraits, mutationItemMultiplier);

        float avgCurrent = (parentA.GetAverageCurrent() + parentB.GetAverageCurrent()) / 2f;
        float avgMax = (parentA.GetAverageMax() + parentB.GetAverageMax()) / 2f;
        float ratio = avgCurrent / avgMax;

        int ascensionLevel = Mathf.FloorToInt((parentA.ascensions + parentB.ascensions) /2f);

        // Remove parents and add new foal to the player inventory
        Horse breededFoal = HorseFactory.CreateFoal(childTier, childVisual, childTraits, ascensionLevel);

        if (!preventParentConsumption)
        {
            SaveSystem.Instance.RemoveHorse(parentA);
            SaveSystem.Instance.RemoveHorse(parentB);
        }
        breededFoal.BoostStartingStats(ratio * 0.25f);

        foreach(var stat in statMods)
        {
            breededFoal.BoostStartingStat(stat.Stat, stat.Modifier);
        }

        SaveSystem.Instance.AddHorse(breededFoal);
        return breededFoal;
    }

    public static (float baseUp, float baseSame, float baseDown) CalculateFoalOdds(Horse parentA, Horse parentB, float upgradeOutsideMultiplier, float downgradeOutsideMultiplier)
    {
        Vector3 parentAOdds = Vector3.zero;
        Vector3 parentBOdds = Vector3.zero;
        Vector3 foalOdds = Vector3.zero;
        (parentAOdds.x, parentAOdds.y, parentAOdds.z) = CalculateUpgradeOdds(parentA, upgradeOutsideMultiplier, downgradeOutsideMultiplier);
        (parentBOdds.x, parentBOdds.y, parentBOdds.z) = CalculateUpgradeOdds(parentB, upgradeOutsideMultiplier, downgradeOutsideMultiplier);
        foalOdds.x = (parentAOdds.x + parentBOdds.x) / 2;
        foalOdds.y = (parentAOdds.y + parentBOdds.y) / 2;
        foalOdds.z = (parentAOdds.z + parentBOdds.z) / 2;

        return (foalOdds.x, foalOdds.y, foalOdds.z);
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

    public static (float baseUp, float baseSame, float baseDown) CalculateUpgradeOdds(Horse horse, float upgradeOutsideMultiplier, float downgradeOutsideMultiplier)
    {
        float _baseUp = horse.Tier.UpgradeChance;
        float _baseSame = horse.Tier.SameChance;
        float _baseDown = horse.Tier.DowngradeChance;

        float multUp = 1 * upgradeOutsideMultiplier;
        float multDown = 1 * downgradeOutsideMultiplier;
        foreach (TraitDef trait in horse.Traits)
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

        int volatilityNo = Mathf.CeilToInt((CalculateTierVolatility(parentA)
                                          + CalculateTierVolatility(parentB)) / 2f);
        Debug.Log("Volatilityno: " + volatilityNo);
        float volatilityMult = Mathf.Pow(volatilityNo, 0.68f);

        const float baseX1 = 0.83f;
        const float baseX2 = 0.14f;
        const float baseX3 = 0.02f;

        float w1 = baseX1;
        float w2 = baseX2 * volatilityMult;
        float w3 = baseX3 * volatilityMult;

        float total = w1 + w2 + w3;
        float pick = Random.value * total;

        if (pick <= w1) delta *= 1;  // small or no volatility → ~100% here
        else if (pick <= w1 + w2) delta *= 2;  // higher volatility boosts this
        else delta *= 3;  // and this

        var allTiers = HorseMarketDatabase.Instance._allTiers;
        int minIdx = allTiers.First().TierIndex;
        int maxIdx = allTiers.Last().TierIndex;
        int newIndex = Mathf.Clamp(parentA.Tier.TierIndex + delta, minIdx, maxIdx);
        return allTiers[newIndex - 1];
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

    private static List<TraitDef> CalculateTraits(Horse parentA, Horse parentB, int guaranteedTraitsQuality, float itemMutation = 1f)
    {
        Dictionary<TraitDef, float> potentialTraits = new Dictionary<TraitDef, float>();
        foreach (TraitDef trait in parentA.Traits)
            potentialTraits[trait] = trait.BaseInheritChance;
        foreach (TraitDef trait in parentB.Traits)
        {
            if (potentialTraits.ContainsKey(trait))
                potentialTraits[trait] = trait.BaseInheritChance + 0.20f;
            else
                potentialTraits[trait] = trait.BaseInheritChance;
        }

        List<TraitDef> foalTraits = new List<TraitDef>();

        //Sort by highest quality first
        foreach (var kvp in potentialTraits.OrderByDescending(k => k.Key.quality))
        {
            var trait = kvp.Key;
            var chance = kvp.Value;

            // check against all already‐added traits for conflicts in either direction
            bool conflicts = foalTraits.Any(existing =>
                existing.IsConflict(trait) || trait.IsConflict(existing));
            // roll inheritance
            if(!conflicts)
            {
                if (trait.quality < guaranteedTraitsQuality)
                {
                    if (Random.value < chance)
                    {
                        foalTraits.Add(trait);
                    }
                }
                else
                {
                    foalTraits.Add(trait);
                }
            }
            // else: skip this trait because it would clash
        }

        // Roll mutations if necessary
        float mutMultA = 1;
        float mutMultB = 1;
        foreach (TraitDef trait in parentA.Traits)
            mutMultA += trait.MutationMultiplier - 1f;
        foreach (TraitDef trait in parentB.Traits)
            mutMultB += trait.MutationMultiplier - 1f;

        float initialChance = parentA.Tier.MutationChance * ((mutMultA + mutMultB) * 0.5f) * itemMutation;
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
