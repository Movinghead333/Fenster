using FensterGame;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Stopwatch = System.Diagnostics.Stopwatch;

public class FensterUIC : MonoBehaviour
{
    #region Singleton
    public static FensterUIC Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }
    #endregion

    public GameObject GameCanvas;
    public SpriteManager SpriteManager;
    public StatsPanelUIC StatsPanelUIC;
    public GameObject PlayCardPrefab;
    public PlayCardUIC[,] PlayCardUICs;
    public FensterGameManager FensterGameManagerInstance;
    public GameObject GuessButtonsPanel;

    public Vector2Int? SelectedCardCoordinates = null;
    public GuessType? SelectedGuessType = null;

    private readonly Stopwatch _stopwatch = new Stopwatch();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FensterGameManagerInstance = new FensterGameManager();

        PlayCardUICs = new PlayCardUIC[FensterGameManager.FensterLayout.GetLength(0), FensterGameManager.FensterLayout.GetLength(1)];
        for (int y = 0; y < FensterGameManager.FensterLayout.GetLength(0); y++)
        {
            for (int x = 0; x < FensterGameManager.FensterLayout.GetLength(1); x++)
            {
                Vector3 rotationAngles = new Vector3(0, 0, x % 2 == 1 ? 90 : 0);
                GameObject cardGO = Instantiate(PlayCardPrefab, Vector3.zero, Quaternion.Euler(rotationAngles), GameCanvas.transform);

                if (FensterGameManager.FensterLayout[y, x] != 0)
                {

                    PlayCardUICs[y, x] = cardGO.GetComponentInChildren<PlayCardUIC>();
                    PlayCardUICs[y, x].CardCoordinates = new Vector2Int(x, y);
                }
                else
                {
                    cardGO.GetComponentInChildren<PlayCardUIC>().gameObject.SetActive(false);
                    PlayCardUICs[y, x] = null;
                }
            }
        }

        UpdateCards();
        _stopwatch.Start();
    }

    private void Update()
    {
        int seconds = _stopwatch.Elapsed.Seconds;
        StatsPanelUIC.SetTimeText(new DateTime(_stopwatch.Elapsed.Ticks).ToString("mm:ss"));
    }

    public void OnCardSelected(Vector2Int cardCoordinates)
    {
        if (FensterGameManagerInstance.IsCardSelectableForReset(cardCoordinates.x, cardCoordinates.y))
        {
            FensterGameManagerInstance.ProcessContinueCommand();
            UpdateCards();
            StatsPanelUIC.SetIncorrectGuessesText(FensterGameManagerInstance.IncorrectGuesses);
            StatsPanelUIC.SetCardsRerolledText(FensterGameManagerInstance.CardsRerolled);
            return;
        }
        else if (FensterGameManagerInstance.IsCardSelectableForReveal(cardCoordinates.x, cardCoordinates.y))
        {
            if (SelectedCardCoordinates.HasValue)
            {
                PlayCardUICs[SelectedCardCoordinates.Value.y, SelectedCardCoordinates.Value.x].SetCardHighlightStatus(CardHighlightStatus.None);

                if (cardCoordinates == SelectedCardCoordinates.Value)
                {
                    SelectedCardCoordinates = null;
                    GuessButtonsPanel.gameObject.SetActive(false);
                    UpdateCards();
                    Debug.Log($"Deselecting selected card (x: {cardCoordinates.x}, y: {cardCoordinates.y})");
                    return;
                }
            }

            GuessButtonsPanel.gameObject.SetActive(true);
            GuessButtonsPanel.transform.position = Input.mousePosition;

            SelectedCardCoordinates = cardCoordinates;
            PlayCardUICs[SelectedCardCoordinates.Value.y, SelectedCardCoordinates.Value.x].SetCardHighlightStatus(CardHighlightStatus.Selected);
            UpdateCards();

            Debug.Log($"Successfully selected card (x: {cardCoordinates.x}, y: {cardCoordinates.y})");
        }
        else
        {
            Debug.LogWarning($"Cannot select card (x: {cardCoordinates.x}, y: {cardCoordinates.y})");
        }

    }

    public void OnLowerButtonPressed()
    {
        SelectedGuessType = GuessType.BelowOrWithinBounds;
        ProcessGuess();
    }

    public void OnEqualButtonPressed()
    {
        SelectedGuessType = GuessType.EqualOrOnBounds;
        ProcessGuess();
    }

    public void OnHigherButtonPressed()
    {
        SelectedGuessType = GuessType.AboveOrOutsideBounds;
        Debug.Log("Guessing higher");
        ProcessGuess();
    }

    private void ProcessGuess()
    {
        if (SelectedCardCoordinates.HasValue && SelectedGuessType.HasValue)
        {
            bool guessCorrect = FensterGameManagerInstance.ProcessRevealCardCommand(new RevealCardCommand(
                SelectedCardCoordinates.Value.x, SelectedCardCoordinates.Value.y, SelectedGuessType.Value
            ));

            Debug.Log($"Guess correct: {guessCorrect}");

            PlayCardUICs[SelectedCardCoordinates.Value.y, SelectedCardCoordinates.Value.x].SetCardHighlightStatus(CardHighlightStatus.None);
            SelectedCardCoordinates = null;
            GuessButtonsPanel.gameObject.SetActive(false);
            UpdateCards();
        }
    }

    private void UpdateCards()
    {
        for (int y = 0; y < FensterGameManager.FensterLayout.GetLength(0); y++)
        {
            for (int x = 0; x < FensterGameManager.FensterLayout.GetLength(1); x++)
            {
                if (!FensterGameManagerInstance.Field[y, x].HasValue)
                {
                    continue;
                }

                PlayCardUICs[y, x].SetCardHighlightStatus(CardHighlightStatus.None);

                if (FensterGameManagerInstance.IsCardSelectableForReveal(x, y) && SelectedCardCoordinates == null && !FensterGameManagerInstance.WaitingForContinueAfterIncorrectGuess)
                {
                    PlayCardUICs[y, x].SetCardHighlightStatus(CardHighlightStatus.Selectable);
                }

                if (FensterGameManagerInstance.Field[y, x].Value.Revealed)
                {
                    Card c = FensterGameManagerInstance.Field[y, x].Value;
                    PlayCardUICs[y, x].SetSprite(SpriteManager.Sprites[c.Type * 9 + c.Symbol]);
                }
                else
                {
                    PlayCardUICs[y, x].SetSprite(SpriteManager.Sprites.Last());
                }
            }
        }

        if (SelectedCardCoordinates != null)
        {
            PlayCardUICs[SelectedCardCoordinates.Value.y, SelectedCardCoordinates.Value.x].SetCardHighlightStatus(CardHighlightStatus.Selected);
        }

        if (FensterGameManagerInstance.WaitingForContinueAfterIncorrectGuess)
        {
            (int x, int y) coords = FensterGameManagerInstance.LastGuessCoordinates.Value;
            PlayCardUICs[coords.y, coords.x].SetCardHighlightStatus(CardHighlightStatus.IncorrectGuess);
        }
    }
}
