using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.PlasticSCM.Editor.WebApi;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    public PlayerData Current { get; private set; }

    [SerializeField] PlayerData template;      // drag in PlayerDataTemplate.asset

    string SavePath => Path.Combine(Application.persistentDataPath, "player.json");

    void Awake()
    {
        // Enforce the singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    public void Load()
    {
        // 1) Clone the SO so you never touch the asset on disk
        Current = Instantiate(template);

        // 2) If we have saved JSON, overwrite the clone
        if (File.Exists(SavePath))
        {
            var json = File.ReadAllText(SavePath);
            JsonUtility.FromJsonOverwrite(json, Current);
        }
    }

    public void Save()
    {
        var json = JsonUtility.ToJson(Current, true);
        File.WriteAllText(SavePath, json);
    }

    void OnApplicationQuit() => Save();
    void OnApplicationPause(bool pause)
    {
        if (pause) Save();
    }
}
