using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AscensionSystem : MonoBehaviour
{
    public static AscensionSystem Instance { get; private set; }

    public List<Horse> mythicHorses;
    public int mythicIndex;

    public int chargeLevel = 0;

    public int currentRolls = 3;

    // internal shuffle bag
    private List<int> shuffleBag = new List<int>();

    [Header("Values")]
    [SerializeField] private Vector2 emeraldsToLE;
    [SerializeField] private Vector2 LEtoEmeralds;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // init shuffle bag
        RefillBag();
    }

    public Horse AscendHorse(Horse horse)
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

    public void Charge()
    {
        if (chargeLevel >= 10)
            return;
        if (EconomySystem.Instance.RemoveLiquidEmeralds(GetChargePrice()))
        {
            chargeLevel += 1;
            currentRolls = 3;
        }
    }

    public long GetChargePrice()
    {
        return Mathf.Max(1, Mathf.RoundToInt(Mathf.Pow(chargeLevel, 4f)));
    }

    public float GetChance()
    {
        return Mathf.Pow(chargeLevel / 10f, 7f) + 0.0000001f;
    }

    public int Roll()
    {
        Debug.Log(currentRolls);
        if(currentRolls <= 0)
            return 1;

        currentRolls -= 1;
        if (Random.value < GetChance())
        {
            return -1; 
        }
        return 0;
    }

    public Horse SelectMythical()
    {
        return HorseFactory.CreateCustomHorse(mythicHorses[mythicIndex]);
    }

    public void GenerateMythical()
    {
        if (shuffleBag.Count == 0)
            RefillBag();

        // take the next index from the bag
        mythicIndex = shuffleBag[0];
        shuffleBag.RemoveAt(0);

        currentRolls = 3;
        chargeLevel = 0;

        Debug.Log("Selected Mythical Horse: " + mythicHorses[mythicIndex].horseName);
    }

    private void RefillBag()
    {
        shuffleBag.Clear();
        for (int i = 0; i < mythicHorses.Count; i++)
            shuffleBag.Add(i);

        // shuffle in place
        for (int i = 0; i < shuffleBag.Count; i++)
        {
            int rand = Random.Range(i, shuffleBag.Count);
            int temp = shuffleBag[i];
            shuffleBag[i] = shuffleBag[rand];
            shuffleBag[rand] = temp;
        }
    }


    public void ConvertEmeralds()
    {
        if (EconomySystem.Instance.RemoveEmeralds((int)emeraldsToLE.x))
            EconomySystem.Instance.AddLiquidEmeralds((int)emeraldsToLE.y);
    }

    public void ConvertLE()
    {
        if (EconomySystem.Instance.RemoveLiquidEmeralds((int)LEtoEmeralds.x))
            EconomySystem.Instance.AddEmeralds((int)LEtoEmeralds.y);
    }
}
