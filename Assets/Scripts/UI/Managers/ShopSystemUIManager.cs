using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ShopSystemUIManager : MonoBehaviour
{
    public List<HorseShopPanelUI> horsePanels;
    public TextMeshProUGUI refreshOffersPriceUI;

    public HorseInfoPanelUI horseInfoPanel;


    void Start()
    {
        RefreshOffers();

        foreach (var panel in horsePanels)
        {
            panel.OnClicked += BuyHorse;
            panel.InfoClicked += OpenInfoPanel;
        }
    }

    public void RefreshOffers()
    {
        long offerPrice;
        List<Horse> horses;

        /*DEBUG, REMOVE AFTER TESTING
        foreach(var currentTier in HorseMarketDatabase.Instance._allTiers)
        {
            List<long> prices = new List<long>();
            const int SAMPLES = 1000;
            for (int i = 0; i < SAMPLES; i++)
            {
                int traits = UnityEngine.Random.Range(2, 7);
                Horse h = HorseFactory.CreateRandomHorse(currentTier, traits);
                prices.Add(h.GetMaxPrice());
            }
            prices.Sort();

            // Number to trim from each end (floor of 10% of SAMPLES):
            int trimCount = (int)(SAMPLES * 0.20);  // e.g. 10
            long sum = 0;
            int count = 0;
            // Sum from index trimCount to index (SAMPLES - trimCount - 1):
            for (int i = trimCount; i < SAMPLES - trimCount; i++)
            {
                sum += prices[i];
                count++;
            }
            double trimmedMean = (double)sum / count;
            Debug.Log($"Tier {currentTier} – 20% trimmed mean: {trimmedMean}");
        } */
        
        (horses, offerPrice) = ShopSystem.GenerateOffers(horsePanels.Count);

        for (int i = 0; i < horses.Count; i++)
            horsePanels[i].InitHorseUI(horses[i]);

        if (offerPrice <= 0)
            refreshOffersPriceUI.text = "Free";
        else
            refreshOffersPriceUI.text = offerPrice.ToShortString();
    }

    public void BuyHorse(Horse horse)
    {
        ShopSystem.BuyHorse(horse);

        refreshOffersPriceUI.text = "Free";
    }

    public void OpenInfoPanel(Horse horse, bool inventoryMode)
    {
        horseInfoPanel.gameObject.SetActive(true);
        horseInfoPanel.HorseUIInit(horse, inventoryMode);
    }
}
