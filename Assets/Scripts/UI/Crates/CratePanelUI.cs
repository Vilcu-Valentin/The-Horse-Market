using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CratePanelUI : MonoBehaviour
{
    public TextMeshProUGUI crateName;
    public TextMeshProUGUI price;
    public Image crateSprite;
    public Image backgroundColor;

    public Button infoButton;
    public Button buyButton;

    public event Action<CrateDef> OnClicked;
    public event Action<CrateDef> OnInfoClicked;

    public void InitCrateUI(CrateDef crate)
    {
        long _price = crate.CostInEmeralds;
        this.crateName.text = crate.CrateName;
        this.price.text = _price.ToShortString();
        crateSprite.sprite = crate.Icon;
        backgroundColor.color = crate.crateColor;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => OnClicked?.Invoke(crate));

        infoButton.onClick.RemoveAllListeners();
        infoButton.onClick.AddListener(() => OnInfoClicked?.Invoke(crate));
    }
}
