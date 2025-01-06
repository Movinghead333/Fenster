using UnityEngine;
using TMPro;

public class LeaderboardRowItemUIC : MonoBehaviour
{
    public TMP_Text RankText;
    public TMP_Text PlayerNameText;
    public TMP_Text ScoreText;

    public void SetContent(string rank, string playerName, string score)
    {
        RankText.text = rank.ToString();
        PlayerNameText.text = playerName;
        ScoreText.text = score.ToString();
    }
}
