using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class InventoryUIManager : MonoBehaviour
{
    public List<HorseInventoryPanelUI> horsePanels;

    public HorseInfoPanelUI horseInfoPanel;


    void Start()
    {
        LoadHorses();

        foreach (var panel in horsePanels)
        {
            panel.OnClicked += SellHorse;
            panel.InfoClicked += OpenInfoPanel;
        }
    }
    
    public void LoadHorses()
    {
        for(int i = 0; i < horsePanels.Count; i++)
        {
            if (SaveSystem.Instance.Current.horses.Count > i)
            {
                horsePanels[i].gameObject.SetActive(true);
                horsePanels[i].InitHorseUI(SaveSystem.Instance.Current.horses[i]);
            }
            else
            {
                horsePanels[i].gameObject.SetActive(false);
            }
        }
    }

    public void SellHorse(Horse horse)
    {
        // Delete the horse from the inventory and give money to the player
        // Might need to move this into an inventory system or something
        SaveSystem.Instance.RemoveHorse(horse);
        SaveSystem.Instance.AddEmeralds(horse.GetCurrentPrice());
        LoadHorses();
    }

    public void OpenInfoPanel(Horse horse, bool inventoryMode)
    {
        horseInfoPanel.gameObject.SetActive(true);
        horseInfoPanel.HorseUIInit(horse, inventoryMode);
    }
}
