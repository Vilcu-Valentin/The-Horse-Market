using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HorseInventoryPanelUI : MonoBehaviour
{
    public TextMeshProUGUI horseName;
    public TextMeshProUGUI horseTier;
    public TextMeshProUGUI price;
    public Image foreground;
    public Image background;
    public Image horseSprite;

    public Button sellButton;
    public Button infoButton;
    public Button favoriteButton;

    public InfoBarUI trainingBar;

    public event Action<Horse> OnClicked;
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

        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(() => HandleBuyClick(horse));
        infoButton.onClick.AddListener(() => HandleInfoClick(horse, true));

        float avgCaps = Mathf.Max(1f, (float)horse.Max.Average(t => t.Value));
        float avgCurrent = horse.Current.Average(t => (float)t.Value);

        trainingBar.UpdateBar("Training", (long)(avgCurrent / avgCaps), 0, (long)avgCaps, false);
    }

    private void HandleBuyClick(Horse horse)
    {
        OnClicked?.Invoke(horse);
    }

    private void HandleInfoClick(Horse horse, bool inventoryMode)
    {
        InfoClicked?.Invoke(horse, inventoryMode);
    }

}
