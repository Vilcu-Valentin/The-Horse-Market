using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AlmanachVisualEntryUI : MonoBehaviour
{
    public TMP_Text visualName;
    public TMP_Text priceMultiplier;
    public TMP_Text rarity;

    public Image icon;

    public GameObject undiscoveredPanel;

    public void InitUI(VisualDef visual, bool discovered = false)
    {
        undiscoveredPanel.SetActive(!discovered);

        icon.sprite = visual.sprite2D;

        visualName.text = visual.horseColor.ToString();
        visualName.color = visual.textColor;

        priceMultiplier.text = $"Price Multiplier: x{visual.PriceScalar.ToString("#.#")}";

        if (visual.rarityTickets <= 7)
            rarity.text = "Very Rare";
        if (visual.rarityTickets > 7 && visual.rarityTickets <= 12)
            rarity.text = "Rare";
        if (visual.rarityTickets > 12 && visual.rarityTickets <= 38)
            rarity.text = "Uncommon";
        if (visual.rarityTickets > 38)
            rarity.text = "Common";
    }

    public void Discover()
    {
        undiscoveredPanel.SetActive(false);
    }
}