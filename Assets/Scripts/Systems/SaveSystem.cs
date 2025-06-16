using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    public PlayerData Current { get; private set; }

    [SerializeField] private PlayerData template;

    private string SavePath => Path.Combine(Application.persistentDataPath, "player.json");

    // Event for any UI or logic to subscribe to when data changes
    public event System.Action<PlayerData> OnPlayerDataChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    /// <summary>
    /// Loads saved JSON into the existing Current instance (or creates it once).
    /// </summary>
    public void Load()
    {
        // Only instantiate once; after that, reuse the same ScriptableObject instance
        if (Current == null)
        {
            Current = Instantiate(template);
        }

        // Overwrite fields on the existing instance
        if (File.Exists(SavePath))
        {
            var json = File.ReadAllText(SavePath);
            JsonUtility.FromJsonOverwrite(json, Current);
        }

        // Notify listeners that data is fresh
        OnPlayerDataChanged?.Invoke(Current);
    }

    /// <summary>
    /// Saves the Current instance to disk and refreshes it in-place.
    /// </summary>
    public void Save()
    {
        var json = JsonUtility.ToJson(Current, true);
        File.WriteAllText(SavePath, json);

        // Refresh in-place so any binding to Current sees updates
        Load();
    }

    /// <summary>
    /// Helper: add a horse, persist, and notify.
    /// </summary>
    public void AddHorse(Horse horse)
    {
        Current.AddHorse(horse);
        Save();
    }

    /// <summary>
    /// Helper: remove a horse, persist, and notify.
    /// </summary>
    public void RemoveHorse(Horse horse)
    {
        Current.RemoveHorse(horse);
        Save();
    }

    void OnApplicationQuit() => Save();

    void OnApplicationPause(bool pause)
    {
        if (pause) Save();
    }
}
