namespace CaseppCompiler
{
    public record struct Position(int Line, int Column)
    {
        public void operator ++() => Column++;

        public void operator --() => Column--;

        public void operator +=(int offset) => Column += offset;

        public void operator -=(int offset) => Column -= offset;

        public void GoToNextLine()
        {
            Line++;
            Column = 0;
        }

        public void GoToPreviousLine()
        {
            Line--;
            Column = 0;
        }

        public static Position operator +(Position position, int offset) => new Position(position.Line, position.Column + offset);

        public static Position operator -(Position position, int offset) => new Position(position.Line, position.Column - offset);
    }
}
