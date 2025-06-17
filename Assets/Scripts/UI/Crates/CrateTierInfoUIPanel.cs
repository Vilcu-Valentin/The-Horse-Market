using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrateTierInfoUIPanel : MonoBehaviour
{
    public Image tierImage;
    public Image background;

    public TMP_Text tierName;
    public TMP_Text tierOdds;

    public void InitUI(TierDef tierDef, float odds)
    {
        tierImage.sprite = tierDef.tierIcon;
        background.color = tierDef.BackgroundColor;

        tierName.color = tierDef.HighlightColor;
        tierOdds.color = tierDef.HighlightColor;

        tierName.text = tierDef.TierName;
        if (odds > 0.01f)
            tierOdds.text = odds.ToString("# %");
        else
            tierOdds.text = "Less than 1%";
    }
}
