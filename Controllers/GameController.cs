using Microsoft.AspNetCore.Mvc;
using TicTacToe.Models;
using TicTacToe.Services;
using Microsoft.AspNetCore.SignalR;
using TicTacToe.Hubs;
using Microsoft.Extensions.Logging;

namespace TicTacToe.Controllers
{
    public class GameController : Controller
    {
        private readonly IGameService _gameService;
        private readonly IHubContext<GameHub> _hubContext;
        private readonly ILogger<GameController> _logger;

        public GameController(IGameService gameService, IHubContext<GameHub> hubContext, ILogger<GameController> logger)
        {
            _gameService = gameService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public IActionResult Index()
        {
            string gameId = HttpContext.Session.GetString("GameId") ?? "";
            _logger.LogInformation($"Index action called. GameId from session: {gameId}");
            var gameState = _gameService.GetGame(gameId);

            // Save the game ID in session
            HttpContext.Session.SetString("GameId", gameState.GameId);
            _logger.LogInformation($"New GameId set in session: {gameState.GameId}");

            return View(gameState);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult MakeMove([FromBody] MoveRequest request)
        {
            _logger.LogInformation($"MakeMove action called. GameId: {request.GameId}, Position: {request.Position}");
            var gameState = _gameService.MakeMove(request.GameId, request.Position);

            // If it's single player mode and it's O's turn, make AI move
            if (gameState.GameMode == "single" && !gameState.XIsNext && gameState.GameStatus == "playing")
            {
                int aiMove = _gameService.GetAIMove(gameState);
                if (aiMove != -1)
                {
                    _logger.LogInformation($"AI making move at position: {aiMove}");
                    gameState = _gameService.MakeMove(request.GameId, aiMove);
                }
            }

            // Notify all clients about the move
            _hubContext.Clients.Group(request.GameId).SendAsync("ReceiveMove", gameState);

            return Json(gameState);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult ResetGame([FromBody] MoveRequest request)
        {
            _logger.LogInformation($"ResetGame action called. GameId: {request.GameId}");
            var gameState = _gameService.ResetGame(request.GameId);
            // Notify all clients about the reset
            _hubContext.Clients.Group(request.GameId).SendAsync("GameReset", gameState);
            return Json(gameState);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult ChangeGameMode([FromBody] GameModeRequest request)
        {
            string gameId = HttpContext.Session.GetString("GameId") ?? "";
            _logger.LogInformation($"ChangeGameMode action called. GameId: {gameId}, New Mode: {request.Mode}");
            var gameState = _gameService.ChangeGameMode(gameId, request.Mode);
            // Notify all clients about the mode change
            _hubContext.Clients.Group(gameId).SendAsync("GameModeChanged", gameState);
            return Json(gameState);
        }
    }
}

