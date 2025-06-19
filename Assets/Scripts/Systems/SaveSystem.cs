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
public class PlayerDataDTO
{
    public long emeralds;
    public List<HorseDTO> horses = new List<HorseDTO>();
}

[DefaultExecutionOrder(-100)]
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    public PlayerData Current { get; private set; }

    [SerializeField] private PlayerData template;
    private string SavePath => Path.Combine(Application.persistentDataPath, "player.json");

    public event Action<PlayerData> OnPlayerDataChanged;

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

    private void Start()
    {
        Load();
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
            }).ToList()
        };

        string json = JsonUtility.ToJson(dto, true);
        File.WriteAllText(SavePath, json);

        OnPlayerDataChanged?.Invoke(Current);
    }

    public void Load()
    {
        if (Current == null)
            Current = Instantiate(template);

        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            PlayerDataDTO dto = JsonUtility.FromJson<PlayerDataDTO>(json);

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
        }

        OnPlayerDataChanged?.Invoke(Current);
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