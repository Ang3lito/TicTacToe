using TicTacToe.Models;

namespace TicTacToe.Services
{
    public interface IGameService
    {
        GameState GetGame(string gameId);
        GameState CreateGame();
        GameState MakeMove(string gameId, int position);
        GameState ResetGame(string gameId);
        GameState ChangeGameMode(string gameId, string mode);
        int GetAIMove(GameState gameState);
    }
}