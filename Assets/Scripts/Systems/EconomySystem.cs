using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EconomySystem : MonoBehaviour
{
    public static EconomySystem Instance { get; private set; }

    public EmeraldsPanelUI emeraldsUI;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddEmeralds(long amount)
    {
        SaveSystem.Instance.Current.emeralds += amount;
        SaveSystem.Instance.Save();
    }

    public bool RemoveEmeralds(long amount)
    {
        long cur = SaveSystem.Instance.Current.emeralds;
        if ((cur -= amount) >= 0)
        {
            SaveSystem.Instance.Current.emeralds -= amount;
            return true;
        }
        else
        {
            emeraldsUI.NotifyInsufficientEmeralds();
            return false;
        }
    }
}
