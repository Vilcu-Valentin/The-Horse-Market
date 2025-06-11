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

    public Slider trainingBar;
    public TextMeshProUGUI trainingAmount;

    public event Action<Horse> OnClicked;
    public event Action<Horse, bool> InfoClicked;
    public event Action<Horse, bool> FavoriteToggled;

    public void InitHorseUI(Horse horse)
    {
        horseName.text = horse.horseName;
        horseTier.text = horse.Tier.TierName;
        horseTier.color = horse.Tier.HighlightColor;
        price.text = horse.GetCurrentPrice().ToShortString();
        foreground.color = horse.Tier.ForegroundColor;
        background.color = horse.Tier.BackgroundColor;
        horseSprite.sprite = horse.Visual.sprite2D;

        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(() => HandleSellClick(horse));
        infoButton.onClick.RemoveAllListeners();
        infoButton.onClick.AddListener(() => HandleInfoClick(horse, true));

        favoriteButton.onClick.RemoveAllListeners();
        favoriteButton.onClick.AddListener(() =>
        {
            bool newFavState = !horse.favorite; 
            SetFavoriteIndicator(newFavState);
            FavoriteToggled?.Invoke(horse, newFavState);
        });

        SetFavoriteIndicator(horse.favorite);

        float avgCaps = Mathf.Max(1f, (float)horse.Max.Average(t => t.Value));
        float avgCurrent = horse.Current.Average(t => (float)t.Value);

        float ratio = avgCurrent / avgCaps;
        trainingBar.value = ratio;
        if (ratio <= 0.01)
            trainingAmount.text = "0%";
        else
            trainingAmount.text = ratio.ToString("# %");

        Debug.Log(horse.horseName + " favorite: " + horse.favorite);
    }

    private void HandleSellClick(Horse horse)
    {
        OnClicked?.Invoke(horse);
    }

    private void HandleInfoClick(Horse horse, bool inventoryMode)
    {
        InfoClicked?.Invoke(horse, inventoryMode);
    }

    public void SetFavoriteIndicator(bool isFav) 
    {
        // favoriteButton.image.sprite = isFav ? filledStarSprite : emptyStarSprite;

        // Or if you just want a tint:
        favoriteButton.image.color = isFav ? new Color(1f, 0.84f, 0f)  // gold
                                           : Color.white;
    }

}
