using Microsoft.AspNetCore.SignalR;
using TicTacToe.Models;
using TicTacToe.Services;

namespace TicTacToe.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameService _gameService;

        public GameHub(IGameService gameService)
        {
            _gameService = gameService;
        }

        public async Task JoinGame(string gameId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
            await Clients.Caller.SendAsync("GameJoined", gameId);
        }

        public async Task MakeMove(string gameId, int position)
        {
            var gameState = _gameService.MakeMove(gameId, position);
            await Clients.Group(gameId).SendAsync("ReceiveMove", gameState);

            // If it's single player mode and it's O's turn, make AI move
            if (gameState.GameMode == "single" && !gameState.XIsNext && gameState.GameStatus == "playing")
            {
                int aiMove = _gameService.GetAIMove(gameState);
                if (aiMove != -1)
                {
                    gameState = _gameService.MakeMove(gameId, aiMove);
                    await Clients.Group(gameId).SendAsync("ReceiveMove", gameState);
                }
            }
        }

        public async Task ResetGame(string gameId)
        {
            var gameState = _gameService.ResetGame(gameId);
            await Clients.Group(gameId).SendAsync("GameReset", gameState);
        }

        public async Task ChangeGameMode(string gameId, string mode)
        {
            var gameState = _gameService.ChangeGameMode(gameId, mode);
            await Clients.Group(gameId).SendAsync("GameModeChanged", gameState);
        }
    }
}