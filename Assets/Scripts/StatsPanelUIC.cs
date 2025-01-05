using UnityEngine;
using TMPro;

public class StatsPanelUIC : MonoBehaviour
{
    public TMP_Text TimeText;
    public TMP_Text IncorrectGuessesText;
    public TMP_Text CardsRerolledText;

    public void SetTimeText(string timeText)
    {
        TimeText.text = timeText;
    }

    public void SetIncorrectGuessesText(int incorrectGuesses)
    {
        IncorrectGuessesText.text = incorrectGuesses.ToString();
    }

    public void SetCardsRerolledText(int  cardsRerolled)
    {
        CardsRerolledText.text = cardsRerolled.ToString();
    }
}
