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

    public Button button;

    public event Action<CrateDef> OnClicked;

    public void InitCrateUI(CrateDef crate)
    {
        long _price = crate.CostInEmeralds;
        this.crateName.text = crate.CrateName;
        this.price.text = _price.ToShortString();
        crateSprite.sprite = crate.Icon;
        backgroundColor.color = crate.crateColor;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnClicked?.Invoke(crate));
    }
}
