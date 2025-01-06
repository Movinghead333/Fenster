using UnityEngine;
using Unity.Services.Leaderboards.Models;
using Unity.Services.Leaderboards;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine.Events;

public class LeaderboardManager : MonoBehaviour
{
    public UnityEvent<List<LeaderboardEntry>?> OnScoresReloaded = new ();

    private async void Awake()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        LoadScores();
    }

    public async Task UpdatePlayerName(string playerName)
    {
        await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
    }

    public async Task AddScore(int score)
    {
        var result = await LeaderboardsService.Instance.AddPlayerScoreAsync("Fewest_Resets_Leaderboard", score);
    }

    public async void LoadScores()
    {
        List<LeaderboardEntry>? result = null;
        try
        {
            GetScoresOptions options = new GetScoresOptions();
            options.Offset = 0;
            options.Limit = 10;

            var getScoresAsyncResult = await LeaderboardsService.Instance.GetScoresAsync("Fewest_Resets_Leaderboard");
            result = getScoresAsyncResult.Results;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        OnScoresReloaded.Invoke(result);
    }
}
