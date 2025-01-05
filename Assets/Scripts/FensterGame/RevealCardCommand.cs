public struct RevealCardCommand
{
    public int X;
    public int Y;
    public GuessType GuessType;

    public RevealCardCommand(int x, int y, GuessType guessType)
    {
        X = x;
        Y = y;
        GuessType = guessType;
    }
}
