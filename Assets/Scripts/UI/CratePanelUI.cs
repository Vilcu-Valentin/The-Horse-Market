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

    public void InitCrateUI(CrateDef crate)
    {
        long _price = crate.CostInEmeralds;
        this.crateName.text = crate.CrateName;
        this.price.text = _price.ToShortString();
        crateSprite.sprite = crate.Icon;
        backgroundColor.color = crate.crateColor;
    }
}
