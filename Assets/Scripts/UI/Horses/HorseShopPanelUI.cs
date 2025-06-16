using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System;

public class HorseShopPanelUI : MonoBehaviour
{
    public TextMeshProUGUI horseName;
    public TextMeshProUGUI horseTier;
    public TextMeshProUGUI price;
    public Image foreground;
    public Image background;
    public Image horseSprite;

    public GameObject soldPanel;
    public TextMeshProUGUI soldText;

    public Button buyButton;
    public Button infoButton;

    public event Action<Horse, HorseShopPanelUI> OnClicked;
    public event Action<Horse, bool> InfoClicked;

    public void InitHorseUI(Horse horse)
    {
        horseName.text = horse.horseName;
        horseTier.text = horse.Tier.TierName;
        horseTier.color = horse.Tier.HighlightColor;
        price.text = horse.GetMarketPrice().ToShortString();
        foreground.color = horse.Tier.ForegroundColor;
        background.color = horse.Tier.BackgroundColor;
        horseSprite.sprite = horse.Visual.sprite2D;

        soldPanel.GetComponent<Image>().color = new Color(horse.Tier.BackgroundColor.r, horse.Tier.BackgroundColor.g, horse.Tier.BackgroundColor.b, 0.85f);
        soldText.color = horse.Tier.HighlightColor;
        soldPanel.SetActive(false);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => HandleBuyClick(horse));
        infoButton.onClick.AddListener(() => HandleInfoClick(horse, false));
    }

    public void SetSoldPanelTrue()
    {
        soldPanel.SetActive(true);
    }

    private void HandleBuyClick(Horse horse)
    {
        OnClicked?.Invoke(horse, this);
    }

    private void HandleInfoClick(Horse horse, bool inventoryMode)
    {
       InfoClicked?.Invoke(horse, inventoryMode);
    }
}
