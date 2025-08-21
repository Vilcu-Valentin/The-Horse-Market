using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ItemInventoryUIManager : MonoBehaviour
{
    public Transform itemParent;
    public GameObject itemPrefab;
    public GameObject noItemsPanel;

    private Action<Item> onSelectCallback;

    //called only once
    public void InitUI(Action<Item> onPick, bool trainingItems)
    {
        gameObject.SetActive(true);

        foreach (Transform child in itemParent)
        {
            Destroy(child.gameObject);
        }

        onSelectCallback = onPick;

        var items = SaveSystem.Instance.Current.items;

        if(items.Count <= 0)
        {
            noItemsPanel.SetActive(true);
            return;
        }
        noItemsPanel.SetActive(false);

        var sortedItems = items.OrderByDescending(i => i.Def.quality).ThenBy(i => i.Def.name); 

        foreach (var item in sortedItems)
        {
            if (item.Def.isTrainingItem == trainingItems)
            {
                GameObject tr = Instantiate(itemPrefab, itemParent);
                tr.GetComponent<ItemUI>().InitItem(item);
                tr.GetComponent<ItemUI>().SelectClicked += HandleSelect;
            }
        }
    }

    private void HandleSelect(Item item)
    {
            onSelectCallback(item);
            // auto-close or revert to browsing
            onSelectCallback = null;
            gameObject.SetActive(false);
    }
}
