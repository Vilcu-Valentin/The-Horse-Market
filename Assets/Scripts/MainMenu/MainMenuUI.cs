using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuSaveUI : MonoBehaviour
{
    [Header("Main Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject newGamePanel;
    public GameObject loadGamePanel;

    [Header("New Game")]
    public TMP_InputField saveNameInput;
    public Button createGameButton;
    public Button cancelNewGameButton;
    public TextMeshProUGUI newGameErrorText;

    [Header("Load Game")]
    public Transform saveSlotParent;
    public GameObject saveSlotPrefab;
    public Button backToMainButton;
    public TextMeshProUGUI noSavesText;

    [Header("Main Menu Buttons")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button quitButton;

    [Header("Scene Management")]
    public string gameSceneName = "GameScene";

    private List<SaveSlotUI> currentSaveSlots = new List<SaveSlotUI>();

    private void Start()
    {
        SetupButtons();
        ShowMainMenu();
    }

    private void SetupButtons()
    {
        // Main menu buttons
        newGameButton.onClick.AddListener(ShowNewGamePanel);
        loadGameButton.onClick.AddListener(ShowLoadGamePanel);
        quitButton.onClick.AddListener(QuitGame);

        // New game buttons
        createGameButton.onClick.AddListener(CreateNewGame);
        cancelNewGameButton.onClick.AddListener(ShowMainMenu);
        saveNameInput.onValueChanged.AddListener(OnSaveNameChanged);

        // Load game buttons
        backToMainButton.onClick.AddListener(ShowMainMenu);

        // Subscribe to save manager events
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnSaveListChanged += OnSaveListChanged;
        }
    }

    private void OnDestroy()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnSaveListChanged -= OnSaveListChanged;
        }
    }

    #region Panel Management

    private void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(false);

        // Clear any error messages
        if (newGameErrorText != null)
            newGameErrorText.text = "";
    }

    private void ShowNewGamePanel()
    {
        mainMenuPanel.SetActive(false);
        newGamePanel.SetActive(true);
        loadGamePanel.SetActive(false);

        saveNameInput.text = "";
        saveNameInput.Select();

        if (newGameErrorText != null)
            newGameErrorText.text = "";

        OnSaveNameChanged(saveNameInput.text);
    }

    private void ShowLoadGamePanel()
    {
        mainMenuPanel.SetActive(false);
        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(true);

        RefreshSavesList();
    }

    #endregion

    #region New Game

    private void OnSaveNameChanged(string saveName)
    {
        bool isValid = IsValidSaveName(saveName);
        createGameButton.interactable = isValid;

        if (newGameErrorText != null)
        {
            if (string.IsNullOrWhiteSpace(saveName))
            {
                newGameErrorText.text = "";
            }
            else if (SaveManager.Instance.SaveExists(saveName))
            {
                newGameErrorText.text = "A save with this name already exists";
                newGameErrorText.color = Color.red;
            }
            else if (!isValid)
            {
                newGameErrorText.text = "Invalid save name";
                newGameErrorText.color = Color.red;
            }
            else
            {
                newGameErrorText.text = "✓ Valid name";
                newGameErrorText.color = Color.green;
            }
        }
    }

    private bool IsValidSaveName(string saveName)
    {
        if (string.IsNullOrWhiteSpace(saveName))
            return false;

        if (saveName.Length < 1 || saveName.Length > 50)
            return false;

        if (SaveManager.Instance.SaveExists(saveName))
            return false;

        return true;
    }

    private void CreateNewGame()
    {
        string saveName = saveNameInput.text.Trim();

        if (!IsValidSaveName(saveName))
        {
            if (newGameErrorText != null)
                newGameErrorText.text = "Please enter a valid save name";
            return;
        }

        if (SaveManager.Instance.CreateNewSave(saveName))
        {
            LoadGameScene();
        }
        else
        {
            if (newGameErrorText != null)
                newGameErrorText.text = "Failed to create new game";
        }
    }

    #endregion

    #region Load Game

    private void RefreshSavesList()
    {
        // Clear existing slots
        foreach (var slot in currentSaveSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        currentSaveSlots.Clear();

        // Get available saves
        var saves = SaveManager.Instance.GetAvailableSaves();

        if (saves.Count == 0)
        {
            noSavesText.gameObject.SetActive(true);
            return;
        }

        noSavesText.gameObject.SetActive(false);

        // Sort saves by last saved time (most recent first)
        saves.Sort((a, b) => b.lastSaved.CompareTo(a.lastSaved));

        // Create UI slots for each save
        foreach (var save in saves)
        {
            GameObject slotObj = Instantiate(saveSlotPrefab, saveSlotParent);
            SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();

            if (slotUI != null)
            {
                slotUI.Setup(save, OnLoadSaveClicked, OnDeleteSaveClicked);
                currentSaveSlots.Add(slotUI);
            }
        }
    }

    private void OnSaveListChanged(List<SaveSlotInfo> saves)
    {
        if (loadGamePanel.activeInHierarchy)
        {
            RefreshSavesList();
        }
    }

    private void OnLoadSaveClicked(SaveSlotInfo saveSlot)
    {
        if (SaveManager.Instance.LoadSave(saveSlot))
        {
            LoadGameScene();
        }
        else
        {
            Debug.LogError($"Failed to load save: {saveSlot.saveName}");
            // You might want to show an error message to the user here
        }
    }

    private void OnDeleteSaveClicked(SaveSlotInfo saveSlot)
    {
        // You might want to add a confirmation dialog here
        SaveManager.Instance.DeleteSave(saveSlot);
    }

    #endregion

    #region Scene Management

    private void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion
}