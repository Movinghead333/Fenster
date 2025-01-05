using System.Collections.Generic;
using System;
using System.Linq;

namespace FensterGame
{
    [Serializable]
    public class FensterGameManager
    {
        public int Seed = 0;
        public readonly static int[,] FensterLayout =
        {
            { 0,0,1,2,2,2,1 },
            { 1,0,2,0,2,0,2 },
            { 2,2,2,2,2,2,2 },
            { 0,0,2,0,2,0,2 },
            { 0,0,1,2,2,2,1 },
        };

        public Card?[,] Field;
        public List<Card> Deck;
        public int IncorrectGuesses { get; private set; } = 0;
        public int CardsRerolled { get; private set; } = 0;
        public bool WaitingForContinueAfterIncorrectGuess { get; private set; } = false;
        public (int x, int y)? LastGuessCoordinates { get; private set; } = null;

        private readonly System.Random _rng;

        public FensterGameManager()
        {
            _rng = new System.Random(Seed);
            Field = new Card?[FensterLayout.GetLength(0), FensterLayout.GetLength(1)];
            Deck = new List<Card>();

            for (int type = 0; type < 4; type++)
            {
                for (int symbol = 0; symbol < 9; symbol++)
                {
                    Deck.Add(new Card(type, symbol, false));
                }
            }

            Deck.Shuffle(_rng);

            FillFieldWithCards();
        }

        public bool ProcessRevealCardCommand(RevealCardCommand command)
        {

            if (!IsCardSelectableForReveal(command.X, command.Y) || WaitingForContinueAfterIncorrectGuess)
            {
                return false;
            }

            List<Card> neighbours = GetRevealedNeighbouringCards(command.X, command.Y);

            // A unrevealed card cannot be uncovered if it is surrounded by unrevealed cards only.
            if (neighbours.Count == 0)
            {
                return false;
            }

            LastGuessCoordinates = (command.X, command.Y);
            Card cardToReveal = Field[command.Y, command.X].Value;
            int symbolToReveal = cardToReveal.Symbol;

            bool guessIsCorrect = false;
            if (neighbours.Count == 1)
            {
                // Lower - Equal - Higher - Game
                Card neighbourCard = neighbours[0];
                int neighbourSymbol = neighbourCard.Symbol;
                switch (command.GuessType)
                {
                    case GuessType.BelowOrWithinBounds:
                        guessIsCorrect = symbolToReveal < neighbourSymbol;
                        break;
                    case GuessType.EqualOrOnBounds:
                        guessIsCorrect = symbolToReveal == neighbourSymbol;
                        break;
                    case GuessType.AboveOrOutsideBounds:
                        guessIsCorrect = symbolToReveal > neighbourSymbol;
                        break;
                }
            }
            else
            {
                // Within - On - Outside - Boundsgame
                List<int> symbols = neighbours.Select(x => x.Symbol).ToList();
                int minSymbol = symbols.Min();
                int maxSymbol = symbols.Max();

                //Debug.Log($"symbol: {symbolToReveal}, minSymbol: {minSymbol}, maxSymbol: {maxSymbol}");

                switch (command.GuessType)
                {
                    case GuessType.BelowOrWithinBounds:
                        guessIsCorrect = minSymbol < symbolToReveal && symbolToReveal < maxSymbol;
                        break;
                    case GuessType.EqualOrOnBounds:
                        guessIsCorrect = symbolToReveal == minSymbol || maxSymbol == symbolToReveal;
                        break;
                    case GuessType.AboveOrOutsideBounds:
                        guessIsCorrect = symbolToReveal < minSymbol || maxSymbol < symbolToReveal;
                        break;
                }
            }

            cardToReveal.Revealed = true;
            Field[command.Y, command.X] = cardToReveal;
            WaitingForContinueAfterIncorrectGuess = !guessIsCorrect;

            return guessIsCorrect;
        }

        public bool ProcessContinueCommand()
        {
            if (!WaitingForContinueAfterIncorrectGuess)
            {
                return false;
            }

            // Perform (partial) reset
            IncorrectGuesses++;
            HashSet<(int, int)> currentCoords = new HashSet<(int, int)>() { (LastGuessCoordinates.Value.x, LastGuessCoordinates.Value.y) };
            HashSet<(int, int)> nextCoords = new HashSet<(int, int)>();

            while (currentCoords.Count > 0)
            {
                foreach ((int x, int y) in currentCoords)
                {
                    Card c = Field[y, x].Value;
                    c.Revealed = false;
                    Deck.Add(c);
                    Field[y, x] = null;
                    CardsRerolled++;
                    nextCoords.UnionWith(GetRevealedNeighbouringCoords(x, y));
                    //Debug.Log($"Setting (x: {x}, y: {y}) to null");
                }

                currentCoords = nextCoords;
                nextCoords = new HashSet<(int, int)>();
            }

            Deck.Shuffle(_rng);
            FillFieldWithCards();
            WaitingForContinueAfterIncorrectGuess = false;

            return true;
        }

        /// <summary>
        /// The field must: <br/>
        /// 1. be populated by a card and thus not be null <br/>
        /// 2. not be revealed since only unrevealed cards can be revealed <br/>
        /// 3. have at least one revealed neighbouring card from which a move can be made
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsCardSelectableForReveal(int x, int y)
        {
            return Field[y, x].HasValue && !Field[y, x].Value.Revealed && GetRevealedNeighbouringCards(x, y).Count > 0;
        }

        public bool IsCardSelectableForReset(int x, int y)
        {
            return Field[y, x].HasValue &&
                LastGuessCoordinates.HasValue &&
                WaitingForContinueAfterIncorrectGuess &&
                LastGuessCoordinates.Value.x == x &&
                LastGuessCoordinates.Value.y == y;
        }

        public bool IsGameFinished()
        {
            bool allRevealed = true;

            foreach (Card? card in Field)
            {
                if (card != null)
                {
                    allRevealed &= card.Value.Revealed;
                }
            }

            return allRevealed;
        }

        private void FillFieldWithCards()
        {
            for (int y = 0; y < FensterLayout.GetLength(0); y++)
            {
                for (int x = 0; x < FensterLayout.GetLength(1); x++)
                {
                    // Skip filled card slots so we can reuse this method for resets
                    if (Field[y, x].HasValue)
                    {
                        continue;
                    }

                    switch (FensterLayout[y, x])
                    {
                        case 0:
                            Field[y, x] = null;
                            break;
                        case 1:
                            Card c = Deck[0];
                            c.Revealed = true;
                            Field[y, x] = c;
                            Deck.RemoveAt(0);
                            break;
                        case 2:
                            Field[y, x] = Deck[0];
                            Deck.RemoveAt(0);
                            break;
                        default:
                            throw new ArgumentException("Invalid layout data");
                    }
                }
            }
        }

        private List<Card> GetRevealedNeighbouringCards(int x, int y)
        {
            (int, int)[] directions = { (0, 1), (1, 0), (0, -1), (-1, 0) };
            List<Card> neighbours = new List<Card>();

            foreach ((int dx, int dy) in directions)
            {
                int newX = x + dx;
                int newY = y + dy;
                if (newX < 0 || newY < 0 || newX >= Field.GetLength(1) || newY >= Field.GetLength(0))
                {
                    continue;
                }

                if (Field[newY, newX].HasValue && Field[newY, newX].Value.Revealed)
                {
                    neighbours.Add(Field[newY, newX].Value);
                }
            }

            return neighbours;
        }

        private List<(int x, int y)> GetRevealedNeighbouringCoords(int x, int y)
        {
            (int, int)[] directions = { (0, 1), (1, 0), (0, -1), (-1, 0) };
            List<(int, int)> neighbours = new List<(int, int)>();

            foreach ((int dx, int dy) in directions)
            {
                int newX = x + dx;
                int newY = y + dy;
                if (newX < 0 || newY < 0 || newX >= Field.GetLength(1) || newY >= Field.GetLength(0))
                {
                    continue;
                }

                if (Field[newY, newX].HasValue && Field[newY, newX].Value.Revealed)
                {
                    neighbours.Add((newX, newY));
                }
            }

            return neighbours;
        }
    }

    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list, System.Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
