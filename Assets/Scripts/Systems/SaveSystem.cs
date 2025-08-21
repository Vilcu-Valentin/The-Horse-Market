using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class HorseDTO
{
    public string id;
    public string horseName;
    public bool favorite;
    public string tierID;
    public string visualID;
    public List<string> traitIDs;
    public int currentTrainingEnergy;
    public int remainingCompetitions;
    public Stat[] Current;
    public Stat[] Max;
}

[Serializable]
public class ItemDTO
{
    public string id;
    public string itemDefId;
    public int quantity;
}

[Serializable]
public class AlmanachItemDTO
{
    public string id;
    public bool unlocked;
}

[Serializable]
public class AlmanachDTO
{
    public List<AlmanachItemDTO> unlockedTraits = new List<AlmanachItemDTO>();
    public List<AlmanachItemDTO> unlockedVisuals = new List<AlmanachItemDTO>();
}

[Serializable]
public class PlayerDataDTO
{
    public long emeralds;
    public List<HorseDTO> horses = new List<HorseDTO>();
    public List<ItemDTO> items = new List<ItemDTO>();

    public AlmanachDTO almanach = new AlmanachDTO();
}

[DefaultExecutionOrder(-100)]
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    public PlayerData Current { get; private set; }

    [SerializeField] private PlayerData template;

    public string CurrentSaveName { get; private set; } = "default"; // fallback

    private string SaveDirectory => Application.persistentDataPath;
    private string SavePath => Path.Combine(SaveDirectory, $"{CurrentSaveName}.json");

    public event Action<PlayerData> OnPlayerDataChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager missing!");
            return;
        }

        string saveName = SaveManager.Instance.CurrentSaveName ?? "default";
        Load(saveName);
    }

    public void SetCurrentSave(string saveName)
    {
        CurrentSaveName = saveName;
    }

    public void Save()
    {
        var dto = new PlayerDataDTO
        {
            emeralds = Current.emeralds,
            horses = Current.horses.Select(h => new HorseDTO
            {
                id = h.Id.ToString(),
                horseName = h.horseName,
                favorite = h.favorite,
                tierID = h.Tier.ID,
                visualID = h.Visual.ID,
                traitIDs = new List<string>(h.Traits.Select(t => t.ID)),
                currentTrainingEnergy = h.currentTrainingEnergy,
                remainingCompetitions = h.remainingCompetitions,
                Current = h.Current,
                Max = h.Max
            }).ToList(),
            items = Current.items.Select(i => new ItemDTO
            {
                id = i.Id.ToString(),
                itemDefId = i.Def.ID,
                quantity = i.Quantity,
            }).ToList()
        };

        if (AlmanachSystem.Instance != null)
        {
            dto.almanach.unlockedTraits = AlmanachSystem.Instance.unlockedTraits
                .Select(t => new AlmanachItemDTO { id = t.definition.ID, unlocked = t.unlocked })
                .ToList();

            dto.almanach.unlockedVisuals = AlmanachSystem.Instance.unlockedVisuals
                .Select(v => new AlmanachItemDTO { id = v.definition.ID, unlocked = v.unlocked })
                .ToList();
        }

        string json = JsonUtility.ToJson(dto, true);
        File.WriteAllText(SavePath, json);

        OnPlayerDataChanged?.Invoke(Current);
    }

    public void Load(string saveName)
    {
        CurrentSaveName = saveName;

        if (Current == null)
            Current = Instantiate(template);

        string path = Path.Combine(SaveDirectory, $"{saveName}.json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerDataDTO dto = JsonUtility.FromJson<PlayerDataDTO>(json);

            // Apply to Current...
            Current.emeralds = dto.emeralds;
            Current.horses.Clear();

            foreach (HorseDTO h in dto.horses)
            {
                TierDef tier = HorseMarketDatabase.Instance.GetTier(h.tierID);
                VisualDef visual = HorseMarketDatabase.Instance.GetVisual(h.visualID);
                List<TraitDef> traits = h.traitIDs
                    .Select(id => HorseMarketDatabase.Instance.GetTrait(id))
                    .Where(t => t != null)
                    .ToList();

                Horse horse = new Horse(Guid.Parse(h.id), tier, visual, traits)
                {
                    horseName = h.horseName,
                    favorite = h.favorite,
                    currentTrainingEnergy = h.currentTrainingEnergy,
                    remainingCompetitions = h.remainingCompetitions,
                    Current = h.Current,
                    Max = h.Max
                };

                Current.AddHorse(horse);
            }

            Current.items.Clear();
            foreach (ItemDTO it in dto.items)
            {
                var def = ItemDatabase.Instance.GetItemDef(it.itemDefId);
                if (def == null) continue;

                Current.AddItem(def, it.quantity);
            }

            AlmanachSystem almanach = AlmanachSystem.Instance ?? FindObjectOfType<AlmanachSystem>();
            if (almanach != null)
                almanach.Initialize(dto.almanach);
        }
        else
        {
            // First-time save
            Current = Instantiate(template);

            AlmanachSystem almanach = AlmanachSystem.Instance ?? FindObjectOfType<AlmanachSystem>();
            if (almanach != null)
            {
                if (!almanach.HasSeededData())
                    almanach.SeedLists();

                almanach.Initialize(null);
            }

            Save(); // Immediately create a file
        }

        OnPlayerDataChanged?.Invoke(Current);
    }

    public static List<string> GetAllSaveNames()
    {
        var files = Directory.GetFiles(Application.persistentDataPath, "*.json");
        return files.Select(Path.GetFileNameWithoutExtension).ToList();
    }

    public void AddHorse(Horse h)
    {
        Current.AddHorse(h);
        Save();
    }

    public void RemoveHorse(Horse h)
    {
        Current.RemoveHorse(h);
        Save();
    }

    public void AddItem(ItemDef item)
    {
        Current.AddItem(item);
        Save();
    }

    public void RemoveItem(ItemDef item)
    {
        Current.RemoveItem(item);
        Save();
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            Save();
    }
}