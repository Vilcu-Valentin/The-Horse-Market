using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CratePanelUI : MonoBehaviour
{
    public TextMeshProUGUI crateName;
    public TextMeshProUGUI price;
    public Image crateSprite;
    public Image backgroundColor;

    public void InitCrateUI(string crateName, int price, Sprite sprite, Color bgColor)
    {
        long _price = price;
        this.crateName.text = crateName;
        this.price.text = _price.ToShortString();
        crateSprite.sprite = sprite;
        backgroundColor.color = bgColor;
    }
}
