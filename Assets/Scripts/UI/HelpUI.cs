using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TooltipOnHover))]
public class HelpUI : MonoBehaviour, ITooltipProvider
{
    public GameObject tooltipPrefab;
    [TextArea]public string Information;

    public GameObject GetTooltipPrefab()
    {
        return tooltipPrefab;
    }

    public void PopulateTooltip(GameObject tooltipInstance)
    {

        var ui = tooltipInstance.GetComponent<HelpTooltipUI>();
        if (ui == null)
        {
            Debug.LogWarning($"PopulateTooltip: The instantiated prefab does not have a TraitTooltipUI component.");
            return;
        }

        ui.InitUI(Information);
    }
}
