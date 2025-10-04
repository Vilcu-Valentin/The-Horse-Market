using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AlmanachTraitsEntryUI : MonoBehaviour
{
    public TMP_Text traitName;
    public TMP_Text traitDescription;
    public TMP_Text traitStats;

    public Image traitIcon;
    public Image traitBorder;

    public GameObject undiscoveredPanel;
    
    public void InitUI(TraitDef trait, bool discovered = false)
    {
        undiscoveredPanel.SetActive(!discovered);

        traitName.text = trait.DisplayName;
        traitDescription.text = trait.Description;
        traitStats.text = trait.ModifiersDescription;

        traitIcon.sprite = trait.Icon;
        traitBorder.sprite = trait.BorderIcon;
    }

    public void Discover()
    {
        undiscoveredPanel.SetActive(false);
    }
}