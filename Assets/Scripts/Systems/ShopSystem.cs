using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class ShopSystem : MonoBehaviour
{
    public List<TierDef> _allTiers;
    public List<HorseShopPanelUI> horsePanels;

    // TEMPORARY FIELDS - move to UI Manager
    public TextMeshProUGUI refreshOffersPriceUI;
    private long refreshOffersIncreases = -1;

    private void Start()
    {
        RefreshOffers();
    }

    public void RefreshOffers()
    {
        foreach(var panel in horsePanels)
        {
            TierDef chosenTier = PickHorseTier();
            int amount = UnityEngine.Random.Range(2, 7);
            Horse pickedH = HorseFactory.CreateRandomHorse(chosenTier, amount);

            panel.InitHorseUI(pickedH);
        }

        refreshOffersIncreases++;

        long offerPrice = GetRefreshOffer();
        Debug.Log("OFFER PRICE: " + offerPrice + " Increases: " + refreshOffersIncreases);

        if (offerPrice <= 0)
            refreshOffersPriceUI.text = "Free";
        else
            refreshOffersPriceUI.text = offerPrice.ToShortString();
    }

    public void BuyHorse()
    {
        refreshOffersIncreases = 0;
        refreshOffersPriceUI.text = "Free";
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

    private long GetRefreshOffer()
    {
        double u = SaveSystem.Instance.Current.GetHighestHorseTier();
        double x = refreshOffersIncreases;

        double expoExp = Math.Pow(u, 0.95) * (x / 10.0);
        double powPart = Math.Pow(x, expoExp);
        double cubicPart = 50.0 * x * Math.Pow(u, 3.2);

        double rawOffer = powPart + cubicPart;

        // guard against NaN
        if (double.IsNaN(rawOffer) || double.IsInfinity(rawOffer))
            rawOffer = 696969; // error

        long units = (long)Math.Round(rawOffer / 50.0);
        return units * 50L;
    }


}
