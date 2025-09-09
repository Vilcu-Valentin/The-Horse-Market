using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AscensionSystem 
{
    public static Horse AscendHorse(Horse horse)
    {
        if (horse.CanAscend())
        {
            Horse ascendedHorse = HorseFactory.AscendHorse(horse);
            EconomySystem.Instance.AddLiquidEmeralds(horse.GetLE_Reward());

            SaveSystem.Instance.RemoveHorse(horse);
            SaveSystem.Instance.AddHorse(ascendedHorse);

            return ascendedHorse;
        }

        return null;
    }
}
