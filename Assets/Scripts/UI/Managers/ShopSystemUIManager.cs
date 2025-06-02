using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
