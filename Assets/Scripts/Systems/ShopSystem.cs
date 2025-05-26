using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    public List<TierDef> _allTiers;
    public List<HorseShopPanelUI> horsePanels;

    public void RefreshOffers()
    {
        foreach(var panel in horsePanels)
        {
            TierDef chosenTier = PickHorseTier();
            int amount = UnityEngine.Random.Range(2, 7);
            Horse pickedH = HorseFactory.CreateRandomHorse(chosenTier, amount);

            panel.InitHorseUI(pickedH);
        }
    }

    private TierDef PickHorseTier()
    {
        var values = new List<(TierDef tier, int ticket)>();

        foreach(var tier in _allTiers)
        {
            int tickets = TicketDistribution(tier.TierIndex, SaveSystem.Instance.Current.GetHighestHorseTier());
            if(tickets > 0 )
            {
                values.Add((tier, tickets));
                Debug.Log(tier.TierName);
            }
        }

        return WeightedSelector<TierDef>.Pick(values);  
    }

    /// <summary>
    /// Gives a number of tickets for a particular Tier based on the max horse tier the player currently has
    /// </summary>
    /// <param name="x">The tier you want to get tickets for</param>
    /// <param name="v">The current max tier</param>
    /// <returns></returns>
    private int TicketDistribution(int x, int v)
    {
        // constants as floats
        const float a = 50f;
        const float r = 2.8f;
        const float s = 1.26f;
        const float k = -0.3f;

        // do all math in floats
        float diff = x - v;                          // x and v are ints, promoted to float
        float denom = 1f + k * MathF.Sign(diff);     // sgn(x-v)
        float exponent = MathF.Pow(MathF.Abs(diff) / denom, s);
        float value = a * MathF.Pow(r, -exponent);

        // round to nearest int
        return (int)MathF.Round(value);
    }
}
