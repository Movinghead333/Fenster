using UnityEngine;
using TMPro;
using System.Diagnostics;
using System;

public class GameFinishedPanelUIC : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _scoreText;

    [SerializeField]
    private TMP_InputField _playerNameInputField;

    public void OnEnable()
    {
        string time = FensterUIC.Instance.GetTimerString();
        int incorrectGuesses = FensterUIC.Instance.FensterGameManagerInstance.IncorrectGuesses;
        int cardsRerolled = FensterUIC.Instance.FensterGameManagerInstance.CardsRerolled;
        _scoreText.text = $"You took a time of {time}, {incorrectGuesses} incorrect guesses and {cardsRerolled} cards rerolled to open the Fenster!";
    }

    public void OnSaveAndResetButtonPressed()
    {
        FensterUIC.Instance.OnSaveAndReset();
        gameObject.SetActive(false);
    }
}
