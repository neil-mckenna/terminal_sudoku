namespace TerminalSudoku_2
{
    public class RandomNumber
    {
        private readonly Random _random = new Random();

        public int Next(int min, int max)
        {
            return _random.Next(min, max);
        }
    }
}
