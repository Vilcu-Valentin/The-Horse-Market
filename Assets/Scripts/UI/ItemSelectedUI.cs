using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelectedUI : MonoBehaviour, ITooltipProvider
{
    public GameObject tooltipPrefab;
    private ItemDef item;

    public Image Icon;

    public void InitUI(Item item)
    {
        this.item = item.Def;

        Icon.sprite = item.Def.Icon;
    }

    public GameObject GetTooltipPrefab()
    {
        return tooltipPrefab;
    }

    public void PopulateTooltip(GameObject tooltipInstance)
    {

        var ui = tooltipInstance.GetComponent<ItemTooltipUI>();
        if (ui == null)
        {
            Debug.LogWarning($"PopulateTooltip: The instantiated prefab does not have a ItemTooltipUI component.");
            return;
        }

        ui.InitItemUI(item);
    }
}
