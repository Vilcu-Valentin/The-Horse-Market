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

    public event Action<Horse> OnClicked;

    public void InitHorseUI(Horse horse)
    {
        horseName.text = horse.horseName;
        horseTier.text = horse.Tier.TierName;
        horseTier.color = horse.Tier.HighlightColor;
        price.text = horse.GetPrice().ToShortString();
        foreground.color = horse.Tier.ForegroundColor;
        background.color = horse.Tier.BackgroundColor;
        horseSprite.sprite = horse.Visual.sprite2D;

        soldPanel.GetComponent<Image>().color = new Color(horse.Tier.BackgroundColor.r, horse.Tier.BackgroundColor.g, horse.Tier.BackgroundColor.b, 0.85f);
        soldText.color = horse.Tier.HighlightColor;
        soldPanel.SetActive(false);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => HandleBuyClick(horse));
    }

    private void HandleBuyClick(Horse horse)
    {
        soldPanel.SetActive(true);
        OnClicked?.Invoke(horse);
    }
}
