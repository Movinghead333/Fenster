namespace FensterGame
{
    public struct Card
    {
        public int Type;
        public int Symbol;
        public bool Revealed;

        public Card(int type, int symbol, bool revealed)
        {
            Type = type;
            Symbol = symbol;
            Revealed = revealed;
        }
    }
}
