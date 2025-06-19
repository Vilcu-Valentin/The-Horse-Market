using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum AppState
{
    Inventory,
    Breeding,
    Shop,
    Competition
}

/// <summary>
/// Central controller for top‐bar navigation and mode switching.
/// </summary>
public class MainUIController : MonoBehaviour
{
    public AudioClip buttonPress;
    [Header("Nav Buttons")]
    public Button inventoryButton;
    public Button breedingButton;
    public Button shopButton;
    public Button competitionButton;

    [Header("Mode Panels")]
    public GameObject inventoryPanel;
    public GameObject breedingPanel;
    public GameObject shopPanel;
    public GameObject competitionPanel;

    [Header("Mode Managers")]
    public InventoryUIManager inventoryUI;
    public BreedingUIManager breedingUI;
    public ShopSystemUIManager shopUI;   
    public CompetitionUIManager competitionUI; 

    private AppState currentState;

    public static MainUIController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Wire nav buttons
        inventoryButton.onClick.AddListener(() => { SetState(AppState.Inventory); AudioManager.Instance.PlaySound(buttonPress, 0.75f, 0.3f); });
        breedingButton.onClick.AddListener(() => { SetState(AppState.Breeding); AudioManager.Instance.PlaySound(buttonPress, 0.75f, 0.3f); });
        shopButton.onClick.AddListener(() => { SetState(AppState.Shop); AudioManager.Instance.PlaySound(buttonPress, 0.75f, 0.3f); });
        competitionButton.onClick.AddListener(() => { SetState(AppState.Competition); AudioManager.Instance.PlaySound(buttonPress, 0.75f, 0.3f); });

        // Start in inventory
        SetState(AppState.Shop);
    }

    /// <summary>
    /// Switch to the given app state, update UI and managers.
    /// </summary>
    public void SetState(AppState newState)
    {
        currentState = newState;

        // 1) Toggle panels
        inventoryPanel.SetActive(newState == AppState.Inventory);
        breedingPanel.SetActive(newState == AppState.Breeding);
        shopPanel.SetActive(newState == AppState.Shop);
        competitionPanel.SetActive(newState == AppState.Competition);

        // 2) Toggle nav buttons
        inventoryButton.interactable = newState != AppState.Inventory;
        breedingButton.interactable = newState != AppState.Breeding;
        shopButton.interactable = newState != AppState.Shop;
        competitionButton.interactable = newState != AppState.Competition;

        // 3) Initialize or refresh the active mode
        switch (newState)
        {
            case AppState.Inventory:
                inventoryUI.OpenForInventory();
                break;
            case AppState.Breeding:
                breedingUI.InitUI();
                break;
            case AppState.Competition:
                competitionUI.RefreshUI();
                break;
        }
    }

    /// <summary>
    /// Exposed methods for other managers to activate modes.
    /// </summary>
    public void ActivateInventory() => SetState(AppState.Inventory);
    public void ActivateBreeding() => SetState(AppState.Breeding);
    public void ActivateShop() => SetState(AppState.Shop);
    public void ActivateCompetition() => SetState(AppState.Competition);
}
