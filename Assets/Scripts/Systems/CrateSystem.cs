using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateSystem : MonoBehaviour
{
    public List<CrateDef> crates;
    // TEMPORARY FIELDS
    public CrateDef selectedCrate;
    public Horse openedHorse;

    public Transform crateUIContents;
    public GameObject crateUIPrefab;
    public CrateOpeningUI opener;

    void Start()
    {
        PopulateUI();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            OpenCrate(selectedCrate);
    }

    //Temporary creation function might need to be moved somewhere else
    public void PopulateUI()
    {
        foreach(var crate in crates) 
        {
            GameObject cr = Instantiate(crateUIPrefab, crateUIContents);
            cr.GetComponent<CratePanelUI>().InitCrateUI(crate.name, crate.CostInEmeralds, crate.Icon, crate.crateColor);
        }
    }

    public void OpenCrate(CrateDef crate)
    {
        var values = new List<(WeightedTier tier, int weight)>();

        foreach (var tier in crate.TierChances)
        {
            values.Add((tier, tier.Tickets));
        }

        WeightedTier chosen = WeightedSelector<WeightedTier>.Pick(values);
        TierDef chosenTier = chosen.Tier;

        int amount = Random.Range(chosen.MinTraits, chosen.MaxTraits);

        Horse pickedH = HorseFactory.CreateRandomHorse(chosenTier, amount);
        SaveSystem.Instance.Current.horses.Add(pickedH);
        opener.PlayOpenAnimation(pickedH.Tier.tierIcon);
    }
}
