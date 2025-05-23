using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BreedingSystem : MonoBehaviour
{
    public static BreedingSystem Instance { get; private set; }

    [Tooltip("All available TierDefs, sorted by TierIndex ascending")]
    public List<TierDef> allTiers;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else Destroy(gameObject);

        allTiers = allTiers.OrderBy(t => t.TierIndex).ToList();
    }

    public void Breed(Horse parentA, Horse parentB) // also add a list of breeding items later
    {
        // Calculates upgrade odds
        Vector3 parentAOdds = Vector3.zero;
        Vector3 parentBOdds = Vector3.zero;
        Vector3 foalOdds = Vector3.zero;
        (parentAOdds.x, parentAOdds.y, parentAOdds.z) = CalculateUpgradeOdds(parentA);
        (parentBOdds.x, parentBOdds.y, parentBOdds.z) = CalculateUpgradeOdds(parentB);
        foalOdds.x = (parentAOdds.x + parentBOdds.x) / 2;
        foalOdds.y = (parentAOdds.y + parentBOdds.y) / 2;
        foalOdds.z = (parentAOdds.z + parentBOdds.z) / 2;

        // Selects new tier based on those odds
        float roll = Random.value;
        int delta;
        if (roll < foalOdds.x) delta = +1;
        else if (roll < foalOdds.x + foalOdds.y) delta = 0;
        else delta = -1;

        TierDef childTier = CalculateTier(parentA, parentB, delta);
        // Calculates visual inheritence

        // Calculates trait inheritence

        // Roll mutations if necessary

        // Remove parents and add new foal to the player inventory
    }

    private (float baseUp, float baseSame, float baseDown) CalculateUpgradeOdds(Horse horse)
    {
        float _baseUp = horse.Tier.UpgradeChance;
        float _baseSame = horse.Tier.SameChance;
        float _baseDown = horse.Tier.DowngradeChance;

        float multUp = 1;
        float multDown = 1;
        foreach(TraitDef trait in horse.Traits)
        {
            multUp += trait.UpgradeMult - 1f;
            multDown += trait.DowngradeMult - 1f;
        }

        _baseUp *= multUp;
        _baseDown *= multDown;
        float totalRaw = _baseUp + _baseDown + _baseSame;
        return (_baseUp / totalRaw, _baseSame / totalRaw, _baseDown / totalRaw);
    }

    private TierDef CalculateTier(Horse parentA, Horse parentB, int tierDelta)
    {
        int delta = tierDelta;

        int newIndex = Mathf.Clamp(parentA.Tier.TierIndex + delta,
                             allTiers.First().TierIndex,
                             allTiers.Last().TierIndex);
        return allTiers[newIndex];
    }
}
