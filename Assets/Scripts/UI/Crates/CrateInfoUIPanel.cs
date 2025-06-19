using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class CrateInfoUIPanel : MonoBehaviour
{
    public TMP_Text crateName;
    public Image crateIcon;
    public Image crateBackground;

    public List<CrateTierInfoUIPanel> tierPanels;

    public void InitUI(CrateDef crate)
    {
        crateName.text = crate.CrateName;
        crateIcon.sprite = crate.Icon;
        crateBackground.color = crate.crateColor;

        foreach (var tierPanel in tierPanels)
            tierPanel.gameObject.SetActive(false);

        for(int i = 0; i < crate.TierChances.Count; i++)
        {
            tierPanels[i].gameObject.SetActive(true);
            tierPanels[i].InitUI(crate.TierChances[i].Tier, crate.getTierChance(crate.TierChances[i]));
        }
    }
}
