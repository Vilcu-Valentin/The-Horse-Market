using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CrateSystem 
{
    /// <summary>
    /// Pure game-logic: picks a tier, spawns a horse, adds it to save, and starts the opening animation.
    /// </summary>
    public static (Horse, List<(WeightedTier tier, int weight)>) OpenCrate(CrateDef crate)
    {
        // Check if enough money, if there are enough remove them, otherwise raise money error

        var values = new List<(WeightedTier tier, int weight)>();

        foreach (var tier in crate.TierChances)
        {
            values.Add((tier, tier.Tickets));
        }

        WeightedTier chosen = WeightedSelector<WeightedTier>.Pick(values);
        TierDef chosenTier = chosen.Tier;

        int amount = Random.Range(chosen.MinTraits, chosen.MaxTraits);

        Horse pickedH = HorseFactory.CreateRandomHorse(chosenTier, amount);
        SaveSystem.Instance.Current.horses.Add(pickedH);

        return (pickedH, values);
    }
}
