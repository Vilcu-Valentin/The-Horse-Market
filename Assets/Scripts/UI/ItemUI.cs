using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemUI : MonoBehaviour, ITooltipProvider
{
    public GameObject tooltipPrefab;
    public Image itemImage;
    public Image borderImage;
    public TMP_Text quantityText;
    public Button selectItemButton;

    private ItemDef item;

    public event Action<Item> SelectClicked;

    public void InitItem(Item item)
    {
        itemImage.sprite = item.Def.Icon;
        borderImage.sprite = item.Def.BorderIcon;
        quantityText.text = ((long)(item.Quantity)).ToShortString();
        selectItemButton.onClick.AddListener(() => HandleSelectClick(item));

        this.item = item.Def;
    }

    public GameObject GetTooltipPrefab()
    {
        return tooltipPrefab;
    }

    private void HandleSelectClick(Item item)
    {
        SelectClicked?.Invoke(item);
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
