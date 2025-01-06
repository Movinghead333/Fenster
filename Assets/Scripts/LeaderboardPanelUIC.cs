using System.Collections.Generic;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.Events;

public class LeaderboardPanelUIC : MonoBehaviour
{
    public LeaderboardManager LeaderboardManager;

    public List<LeaderboardRowItemUIC> LeaderboardRowItemUICs;

    private void Start()
    {
        LeaderboardManager.OnScoresReloaded.AddListener((List<LeaderboardEntry>? entries) =>
        {
            RefreshLeaderboard(entries);
        });
    }

    private void RefreshLeaderboard(List<LeaderboardEntry>? entries)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            LeaderboardEntry entry = entries[i];
            LeaderboardRowItemUICs[i].SetContent((entry.Rank + 1).ToString(), entry.PlayerName, entry.Score.ToString());
        }
    }
}
