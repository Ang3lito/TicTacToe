namespace TicTacToe.Models
{
    public class GameState
    {
        public string[] Board { get; set; } = new string[9];
        public bool XIsNext { get; set; } = true;
        public string GameMode { get; set; } = "single";
        public string GameStatus { get; set; } = "playing";
        public string? Winner { get; set; }
        public int[]? WinningLine { get; set; }
        public Scoreboard Scores { get; set; } = new Scoreboard();
        public string GameId { get; set; } = Guid.NewGuid().ToString();
    }

    public class Scoreboard
    {
        public int X { get; set; } = 0;
        public int O { get; set; } = 0;
        public int Ties { get; set; } = 0;
    }

    public class MoveRequest
    {
        public int Position { get; set; }
        public string GameId { get; set; } = string.Empty;
    }

    public class GameModeRequest
    {
        public string Mode { get; set; } = "single";
    }
}