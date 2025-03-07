"use strict";

// Import SignalR
// Add this line to import the signalR library.  It's assumed this is available globally.  If not, adjust accordingly.
// If using a module bundler, you'll need to import it appropriately for your setup.

// Get game state from hidden fields
let gameId = document.getElementById("gameId").value;
let gameMode = document.getElementById("gameMode").value;
let gameStatus = document.getElementById("gameStatus").value;
let xIsNext = document.getElementById("xIsNext").value === "true";

// SignalR connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/gameHub")
    .withAutomaticReconnect()
    .build();

// Start the connection
connection.start().then(() => {
    console.log("Connected to SignalR hub");
    connection.invoke("JoinGame", gameId);
}).catch(err => console.error(err));

// SignalR event handlers
connection.on("GameJoined", (joinedGameId) => {
    console.log(`Joined game: ${joinedGameId}`);
});

connection.on("ReceiveMove", (updatedGameState) => {
    updateGameUI(updatedGameState);
});

connection.on("GameReset", (updatedGameState) => {
    updateGameUI(updatedGameState);
});

connection.on("GameModeChanged", (updatedGameState) => {
    updateGameUI(updatedGameState);
    gameMode = updatedGameState.gameMode;
    document.getElementById("gameMode").value = gameMode;

    // Update mode buttons
    document.getElementById("singlePlayerBtn").className = gameMode === "single"
        ? "btn btn-primary"
        : "btn btn-outline-primary";
    document.getElementById("multiPlayerBtn").className = gameMode === "multi"
        ? "btn btn-primary"
        : "btn btn-outline-primary";
});

// DOM event handlers
document.querySelectorAll(".game-square").forEach(square => {
    square.addEventListener("click", () => {
        const index = parseInt(square.getAttribute("data-index"));
        makeMove(index);
    });
});

document.getElementById("resetBtn").addEventListener("click", () => {
    resetGame();
});

document.getElementById("singlePlayerBtn").addEventListener("click", () => {
    changeGameMode("single");
});

document.getElementById("multiPlayerBtn").addEventListener("click", () => {
    changeGameMode("multi");
});

// Game functions
function makeMove(position) {
    // Don't allow moves if game is over
    if (gameStatus !== "playing") {
        return;
    }

    fetch("/Game/MakeMove", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ position: position, gameId: gameId })
    })
        .then(response => response.json())
        .then(data => {
            updateGameUI(data);
        })
        .catch(error => console.error("Error:", error));
}

function resetGame() {
    fetch("/Game/ResetGame", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ gameId: gameId })
    })
        .then(response => response.json())
        .then(data => {
            updateGameUI(data);
        })
        .catch(error => console.error("Error:", error));
}

function changeGameMode(mode) {
    fetch("/Game/ChangeGameMode", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ mode: mode })
    })
        .then(response => response.json())
        .then(data => {
            updateGameUI(data);
            gameMode = mode;
            document.getElementById("gameMode").value = mode;

            // Update mode buttons
            document.getElementById("singlePlayerBtn").className = mode === "single"
                ? "btn btn-primary"
                : "btn btn-outline-primary";
            document.getElementById("multiPlayerBtn").className = mode === "multi"
                ? "btn btn-primary"
                : "btn btn-outline-primary";
        })
        .catch(error => console.error("Error:", error));
}

function updateGameUI(gameState) {
    // Update board
    const squares = document.querySelectorAll(".game-square");
    for (let i = 0; i < 9; i++) {
        const mark = gameState.board[i] || "";
        squares[i].querySelector(".game-mark").textContent = mark;
        squares[i].setAttribute("data-mark", mark);

        // Reset winning squares
        squares[i].setAttribute("data-winning", "false");
    }

    // Update winning line if any
    if (gameState.winningLine) {
        gameState.winningLine.forEach(index => {
            squares[index].setAttribute("data-winning", "true");
        });
    }

    // Update game status
    const statusElement = document.getElementById("gameStatus");
    if (gameState.gameStatus === "playing") {
        statusElement.innerHTML = `<span>${gameState.xIsNext ? "X" : "O"}'s turn</span>`;
        statusElement.className = document.body.classList.contains('dark-mode') ? "text-light" : "text-dark";
    } else if (gameState.gameStatus === "won") {
        statusElement.innerHTML = `<span>${gameState.winner} wins!</span>`;
        statusElement.className = "text-success";
    } else {
        statusElement.innerHTML = `<span>Game ended in a draw!</span>`;
        statusElement.className = "text-warning";
    }

    // Update scores
    document.getElementById("scoreX").textContent = gameState.scores.x;
    document.getElementById("scoreO").textContent = gameState.scores.o;
    document.getElementById("scoreTies").textContent = gameState.scores.ties;

    // Update AI label
    document.getElementById("aiLabel").textContent = gameState.gameMode === "single" ? "(AI)" : "";

    // Update hidden fields
    document.getElementById("gameStatus").value = gameState.gameStatus;
    document.getElementById("xIsNext").value = gameState.xIsNext.toString().toLowerCase();

    // Update game state variables
    gameStatus = gameState.gameStatus;
    xIsNext = gameState.xIsNext;
}

// Add event listener for dark mode changes
document.addEventListener('DOMContentLoaded', (event) => {
    const darkModeToggle = document.getElementById('darkModeToggle');
    darkModeToggle.addEventListener('click', () => {
        // Update game status text color when dark mode is toggled
        const statusElement = document.getElementById("gameStatus");
        if (statusElement.classList.contains("text-dark") || statusElement.classList.contains("text-light")) {
            statusElement.classList.toggle("text-dark");
            statusElement.classList.toggle("text-light");
        }
    });
});