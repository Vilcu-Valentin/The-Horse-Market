using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EconomySystem : MonoBehaviour
{
    public AudioClip buyingAudio;
    public AudioClip sellingAudio;
    public AudioClip notEnoughMoneyAudio;
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
    }

    public void AddEmeralds(long amount)
    {
        AudioManager.Instance.PlaySound(sellingAudio, 0.5f, 0.25f);
        SaveSystem.Instance.Current.emeralds += amount;
        SaveSystem.Instance.Save();
    }

    public bool RemoveEmeralds(long amount)
    {
        AudioManager.Instance.PlaySound(buyingAudio, 0.5f, 0.25f);
        long cur = SaveSystem.Instance.Current.emeralds;
        if ((cur -= amount) >= 0)
        {
            SaveSystem.Instance.Current.emeralds -= amount;
            return true;
        }
        else
        {
            AudioManager.Instance.PlaySound(notEnoughMoneyAudio, 0.5f);
            emeraldsUI.NotifyInsufficientEmeralds();
            return false;
        }
    }

    public bool EnoughEmeralds(long amount)
    {
        long cur = SaveSystem.Instance.Current.emeralds;
        if ((cur -= amount) >= 0)
            return true;
        else
            return false;
    }
}
