using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class CompetitionUIManager : MonoBehaviour
{
    public CompetitionLobbyUI competitionLobby;
    [Header("UI Panels")]
    public List<CompetitionUIPanel> competitionPanels;

    private void Start()
    { 
        RefreshUI();

        foreach(var uiPanel in  competitionPanels)
        {
            uiPanel.OnCompeteClicked += OpenCompetitionLobby;
        }
    }

    public void RefreshUI()
    {
        var comps = HorseMarketDatabase.Instance._allCompetitions;
        int count = Mathf.Min(comps.Count, competitionPanels.Count);
        for (int i = 0; i < count; i++)
            competitionPanels[i].InitUI(comps[i]);
        // Hide extra panels
        for (int i = count; i < competitionPanels.Count; i++)
            competitionPanels[i].gameObject.SetActive(false);
    }

    public void OpenCompetitionLobby(CompetitionDef competition)
    {
        competitionLobby.gameObject.SetActive(true);
        competitionLobby.InitUI(competition);
    }
}
