using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TraitUI : MonoBehaviour, ITooltipProvider
{
    public GameObject tooltipPrefab;
    public Image traitImage;
    public Image borderImage;

    private TraitDef trait;

    public void InitTrait(TraitDef trait)
    {
        traitImage.sprite = trait.Icon;
        borderImage.sprite = trait.BorderIcon;

        this.trait = trait;
    }

    public GameObject GetTooltipPrefab()
    {
        return tooltipPrefab;
    }

    public void PopulateTooltip(GameObject tooltipInstance)
    {

        var ui = tooltipInstance.GetComponent<TraitTooltipUI>();
        if (ui == null)
        {
            Debug.LogWarning($"PopulateTooltip: The instantiated prefab does not have a TraitTooltipUI component.");
            return;
        }

        ui.InitTraitUI(trait);
    }
}
