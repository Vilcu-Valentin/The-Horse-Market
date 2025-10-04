using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class ItemSystem 
{
    public static ItemDef PickItem()
    {
        var items = ItemDatabase.Instance._allItems.Select(i => (item: i, ticket: i.rarityTickets)).ToList();
        var pick = WeightedSelector<ItemDef>.Pick(items);

        return pick;
    }
}