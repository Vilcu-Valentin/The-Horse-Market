using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class HorseShopPanelUI : MonoBehaviour
{
    public TextMeshProUGUI horseName;
    public TextMeshProUGUI horseTier;
    public TextMeshProUGUI price;
    public Image foreground;
    public Image background;
    public Image horseSprite;

    public void InitHorseUI(Horse horse)
    {
        horseName.text = horse.horseName;
        horseTier.text = horse.Tier.TierName;
        horseTier.color = horse.Tier.HighlightColor;
        price.text = horse.GetPrice().ToShortString();
        foreground.color = horse.Tier.ForegroundColor;
        background.color = horse.Tier.BackgroundColor;
        horseSprite.sprite = horse.Visual.sprite2D;
    }
}
