using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveSlotInfo
{
    public string saveName;
    public DateTime lastSaved;
    public long emeralds;
    public int horseCount;

    public SaveSlotInfo(string saveName, DateTime lastSaved, long emeralds, int horseCount)
    {
        this.saveName = saveName;
        this.lastSaved = lastSaved;
        this.emeralds = emeralds;
        this.horseCount = horseCount;
    }
}


public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public string GameScene;

    public event Action<List<SaveSlotInfo>> OnSaveListChanged;

    public string CurrentSaveName { get; private set; }

    private string SaveDirectory => Application.persistentDataPath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool SaveExists(string saveName)
    {
        string path = Path.Combine(SaveDirectory, $"{saveName}.json");
        return File.Exists(path);
    }

    public bool CreateNewSave(string saveName)
    {
        try
        {
            /*string path = Path.Combine(SaveDirectory, $"{saveName}.json");

            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            if (!File.Exists(path))
            {
                // Just create an empty PlayerDataDTO so the file exists.
                var dto = new PlayerDataDTO();
                string json = JsonUtility.ToJson(dto, true);
                File.WriteAllText(path, json);
            } */

            CurrentSaveName = saveName;

            TriggerSaveListChanged();
            UnityEngine.SceneManagement.SceneManager.LoadScene(GameScene); // hand over to SaveSystem
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create save: {e}");
            return false;
        }
    }

    public bool LoadSave(SaveSlotInfo saveSlot)
    {
        try
        {
            CurrentSaveName = saveSlot.saveName;
            UnityEngine.SceneManagement.SceneManager.LoadScene(GameScene);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load save {saveSlot.saveName}: {e}");
            return false;
        }
    }

    public void DeleteSave(SaveSlotInfo saveSlot)
    {
        string path = Path.Combine(SaveDirectory, $"{saveSlot.saveName}.json");
        if (File.Exists(path))
        {
            File.Delete(path);
            TriggerSaveListChanged();
        }
    }

    public List<SaveSlotInfo> GetAvailableSaves()
    {
        var result = new List<SaveSlotInfo>();
        var files = Directory.GetFiles(SaveDirectory, "*.json");

        foreach (string file in files)
        {
            string saveName = Path.GetFileNameWithoutExtension(file);
            DateTime lastModified = File.GetLastWriteTime(file);

            long emeralds = 0;
            int horseCount = 0;

            try
            {
                string json = File.ReadAllText(file);
                PlayerDataDTO dto = JsonUtility.FromJson<PlayerDataDTO>(json);

                if (dto != null)
                {
                    emeralds = dto.emeralds;
                    horseCount = dto.horses != null ? dto.horses.Count : 0;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not read save file {file}: {e}");
            }

            result.Add(new SaveSlotInfo(saveName, lastModified, emeralds, horseCount));
        }

        return result;
    }


    private void TriggerSaveListChanged()
    {
        OnSaveListChanged?.Invoke(GetAvailableSaves());
    }
}
