using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Unity.VisualScripting;

public class HorseInventoryPanelUI : MonoBehaviour
{
    public TextMeshProUGUI horseName;
    public TextMeshProUGUI horseTier;
    public TextMeshProUGUI price;
    public Image foreground;
    public Image background;
    public Image horseSprite;

    public Button sellButton;
    public Button selectButton;

    public Button infoButton;
    public Button favoriteButton;

    public Slider trainingBar;
    public TextMeshProUGUI trainingAmount;

    public event Action<Horse> OnClicked;
    public event Action<Horse, bool> InfoClicked;
    public event Action<Horse, bool> FavoriteToggled;
    public event Action<Horse> SelectClicked;

    public void InitHorseUI(Horse horse, InventoryMode mode)
    {
        horseName.text = horse.horseName;
        horseTier.text = horse.Tier.TierName;
        horseTier.color = horse.Tier.HighlightColor;
        price.text = horse.GetCurrentPrice().ToShortString();
        foreground.color = horse.Tier.ForegroundColor;
        background.color = horse.Tier.BackgroundColor;
        horseSprite.sprite = horse.Visual.sprite2D;

        sellButton.onClick.RemoveAllListeners();
        sellButton.gameObject.SetActive(false);
        selectButton.onClick.RemoveAllListeners();
        selectButton.gameObject.SetActive(false);

        infoButton.onClick.RemoveAllListeners();
        favoriteButton.onClick.RemoveAllListeners();
        infoButton.onClick.AddListener(() => HandleInfoClick(horse, true));

        if (mode == InventoryMode.Inventory)
        {
            sellButton.gameObject.SetActive(true);
            sellButton.onClick.AddListener(() => HandleSellClick(horse));
        }else
        {
            selectButton.gameObject.SetActive(true);
            selectButton.onClick.AddListener(() => HandleSelectClick(horse));
        }

        favoriteButton.onClick.AddListener(() =>
        {
            bool newFavState = !horse.favorite; 
            SetFavoriteIndicator(newFavState);
            FavoriteToggled?.Invoke(horse, newFavState);
        });

        SetFavoriteIndicator(horse.favorite);

        trainingAmount.text = horse.GetAverageMax().ToShortString();

        Debug.Log(horse.horseName + " favorite: " + horse.favorite);
    }

    private void HandleSellClick(Horse horse)
    {
        OnClicked?.Invoke(horse);
    }

    private void HandleSelectClick(Horse horse)
    {
        SelectClicked?.Invoke(horse);
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
