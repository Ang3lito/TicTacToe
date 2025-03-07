using TicTacToe.Models;

namespace TicTacToe.Services
{
    public class GameService : IGameService
    {
        private readonly Dictionary<string, GameState> _games = new Dictionary<string, GameState>();
        private readonly Random _random = new Random();

        public GameState GetGame(string gameId)
        {
            if (string.IsNullOrEmpty(gameId) || !_games.ContainsKey(gameId))
            {
                return CreateGame();
            }

            return _games[gameId];
        }

        public GameState CreateGame()
        {
            var gameState = new GameState();
            _games[gameState.GameId] = gameState;
            return gameState;
        }

        public GameState MakeMove(string gameId, int position)
        {
            if (!_games.ContainsKey(gameId))
            {
                return CreateGame();
            }

            var gameState = _games[gameId];

            // Don't allow moves on filled squares or when game is over
            if (position < 0 || position >= 9 || !string.IsNullOrEmpty(gameState.Board[position]) || gameState.GameStatus != "playing")
            {
                return gameState;
            }

            // Make the move
            gameState.Board[position] = gameState.XIsNext ? "X" : "O";

            // Check for winner
            var result = CalculateWinner(gameState.Board);
            if (result != null)
            {
                HandleGameEnd(gameState, result.Value.Winner, result.Value.Line);
            }
            else if (!gameState.Board.Any(square => string.IsNullOrEmpty(square)))
            {
                HandleGameEnd(gameState, null, null);
            }
            else
            {
                gameState.XIsNext = !gameState.XIsNext;
            }

            return gameState;
        }

        public GameState ResetGame(string gameId)
        {
            if (!_games.ContainsKey(gameId))
            {
                return CreateGame();
            }

            var gameState = _games[gameId];
            gameState.Board = new string[9];
            gameState.XIsNext = true;
            gameState.GameStatus = "playing";
            gameState.Winner = null;
            gameState.WinningLine = null;

            return gameState;
        }

        public GameState ChangeGameMode(string gameId, string mode)
        {
            if (!_games.ContainsKey(gameId))
            {
                return CreateGame();
            }

            var gameState = _games[gameId];
            gameState.GameMode = mode;
            gameState.Board = new string[9];
            gameState.XIsNext = true;
            gameState.GameStatus = "playing";
            gameState.Winner = null;
            gameState.WinningLine = null;
            gameState.Scores = new Scoreboard();

            return gameState;
        }

        public int GetAIMove(GameState gameState)
        {
            // First, check if there's a winning move for the AI
            for (int i = 0; i < gameState.Board.Length; i++)
            {
                if (string.IsNullOrEmpty(gameState.Board[i]))
                {
                    var boardCopy = gameState.Board.ToArray();
                    boardCopy[i] = "O";
                    if (CalculateWinner(boardCopy) != null)
                    {
                        return i; // Return winning move
                    }
                }
            }

            // Second, block any winning move for the opponent
            for (int i = 0; i < gameState.Board.Length; i++)
            {
                if (string.IsNullOrEmpty(gameState.Board[i]))
                {
                    var boardCopy = gameState.Board.ToArray();
                    boardCopy[i] = "X";
                    if (CalculateWinner(boardCopy) != null)
                    {
                        return i; // Block opponent's winning move
                    }
                }
            }

            // Try to take the center
            if (string.IsNullOrEmpty(gameState.Board[4]))
            {
                return 4;
            }

            // Try to take the corners
            var corners = new[] { 0, 2, 6, 8 };
            var availableCorners = corners.Where(i => string.IsNullOrEmpty(gameState.Board[i])).ToList();
            if (availableCorners.Any())
            {
                return availableCorners[_random.Next(availableCorners.Count)];
            }

            // Take any available side
            var sides = new[] { 1, 3, 5, 7 };
            var availableSides = sides.Where(i => string.IsNullOrEmpty(gameState.Board[i])).ToList();
            if (availableSides.Any())
            {
                return availableSides[_random.Next(availableSides.Count)];
            }

            // No good moves left
            return -1;
        }

        private void HandleGameEnd(GameState gameState, string? winner, int[]? line)
        {
            gameState.GameStatus = winner != null ? "won" : "draw";
            gameState.Winner = winner;
            gameState.WinningLine = line;

            // Update scores
            if (winner != null)
            {
                if (winner == "X")
                {
                    gameState.Scores.X++;
                }
                else
                {
                    gameState.Scores.O++;
                }
            }
            else
            {
                gameState.Scores.Ties++;
            }
        }

        private (string Winner, int[] Line)? CalculateWinner(string[] board)
        {
            int[][] lines = new int[][]
            {
                new int[] { 0, 1, 2 },
                new int[] { 3, 4, 5 },
                new int[] { 6, 7, 8 },
                new int[] { 0, 3, 6 },
                new int[] { 1, 4, 7 },
                new int[] { 2, 5, 8 },
                new int[] { 0, 4, 8 },
                new int[] { 2, 4, 6 },
            };

            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(board[line[0]]) &&
                    board[line[0]] == board[line[1]] &&
                    board[line[0]] == board[line[2]])
                {
                    return (board[line[0]], line);
                }
            }

            return null;
        }
    }
}

