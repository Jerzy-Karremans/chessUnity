const web_socket = require("ws");
const express = require("express");
const path = require("path");
const http = require("http");
const chalk = require("chalk");

const app = express();
const PORT = process.env.SERVER_PORT || process.env.PORT

// Serve static files
app.use(express.static(path.join(__dirname, "public")));

// Create HTTP server
const httpServer = http.createServer(app);

// Create WebSocket server
const socket = new web_socket.Server({ 
    server: httpServer,
    path: '/ws',
    perMessageDeflate: false,
    maxPayload: 16 * 1024
});

// Start server
httpServer.listen(PORT, () => {
    console.log(chalk.green.bold(`Combined HTTP + WebSocket server running on port ${PORT}`));
    console.log(chalk.blue(`HTTP: http://localhost:${PORT}`));
    console.log(chalk.blue(`WebSocket: ws://localhost:${PORT}/ws`));
    console.log(chalk.gray('═'.repeat(50)));
});

let waitingClients = [];
let gamePairs = [];

socket.on("connection", function connection(ws, request) {
    ws.id = Math.random().toString(36).substr(2, 9);
    ws.isAlive = true;
    ws.on('pong', () => { ws.isAlive = true; });
    
    const clientInfo = {
        id: ws.id,
        ip: request.socket.remoteAddress,
        userAgent: request.headers['user-agent']?.substring(0, 50) + '...' || 'Unknown'
    };
    
    console.log(chalk.green(`Client connected:`), chalk.cyan(`ID: ${clientInfo.id}`), chalk.gray(`IP: ${clientInfo.ip}`));
    
    pairClient(ws);

    ws.on("message", (data) => {
        const message = data.toString();
        const truncatedMessage = message.length > 100 ? message.substring(0, 100) + '...' : message;
        
        console.log(chalk.blue(`Message from ${chalk.cyan(ws.id)}:`), chalk.white(truncatedMessage));
        forwardMoveToOpponent(ws, message);
    });

    ws.on("close", (code, reason) => {
        const reasonText = reason ? reason.toString() : 'No reason provided';
        console.log(chalk.red(`Client ${chalk.cyan(ws.id)} disconnected:`), chalk.yellow(`Code: ${code}`), chalk.gray(`Reason: ${reasonText}`));
        removeClient(ws);
    });

    ws.on("error", (error) => {
        console.log(chalk.red.bold(`WebSocket error for client ${chalk.cyan(ws.id)}:`), chalk.red(error.message));
        removeClient(ws);
    });
});

// Heartbeat with better logging
const heartbeat = setInterval(() => {
    let activeClients = 0;
    let terminatedClients = 0;
    
    socket.clients.forEach((ws) => {
        if (ws.isAlive === false) {
            console.log(chalk.yellow(`Terminating inactive client: ${chalk.cyan(ws.id)}`));
            terminatedClients++;
            return ws.terminate();
        }
        ws.isAlive = false;
        ws.ping();
        activeClients++;
    });
    
    if (activeClients > 0 || terminatedClients > 0) {
        console.log(chalk.gray(`Heartbeat: ${chalk.green(activeClients)} active, ${chalk.red(terminatedClients)} terminated`));
    }
}, 30000);

function forwardMoveToOpponent(sender, moveData) {
    const pair = gamePairs.find(p => p.player1 === sender || p.player2 === sender);
    
    if (pair) {
        const opponent = pair.player1 === sender ? pair.player2 : pair.player1;
        
        if (opponent.readyState === web_socket.OPEN) {
            const truncatedData = moveData.length > 50 ? moveData.substring(0, 50) + '...' : moveData;
            console.log(chalk.magenta(`Forwarding move:`), 
                       chalk.cyan(`${sender.id}`), chalk.gray('→'), chalk.cyan(`${opponent.id}`),
                       chalk.white(`Data: ${truncatedData}`));
            opponent.send(moveData);
        } else {
            console.log(chalk.red(`Opponent ${chalk.cyan(opponent.id)} connection not open, removing...`));
            removeClient(opponent);
        }
    } else {
        console.log(chalk.yellow(`No game pair found for client ${chalk.cyan(sender.id)}`));
    }
}

function pairClient(ws) {
    if (waitingClients.length > 0) {
        const opponent = waitingClients.shift();
        const gameId = Math.random().toString(36).substr(2, 9);
        const pair = { gameId, player1: opponent, player2: ws };
        gamePairs.push(pair);
        
        if (opponent.readyState === web_socket.OPEN && ws.readyState === web_socket.OPEN) {
            opponent.send("connected");
            ws.send("connected");
            console.log(chalk.green.bold(`Game ${chalk.yellow(gameId)} created:`), 
                       chalk.cyan(`Player1: ${opponent.id}`), chalk.gray('vs'), chalk.cyan(`Player2: ${ws.id}`));
        } else {
            gamePairs.pop();
            console.log(chalk.red(`Failed to create game - one client disconnected`));
        }
    } else {
        waitingClients.push(ws);
        ws.send("waiting");
        console.log(chalk.blue(`Client ${chalk.cyan(ws.id)} added to waiting queue`), 
                   chalk.gray(`(${waitingClients.length} waiting)`));
    }
}

function removeClient(ws) {
    // Remove from waiting list
    const wasWaiting = waitingClients.includes(ws);
    waitingClients = waitingClients.filter(client => client !== ws);
    
    if (wasWaiting) {
        console.log(chalk.yellow(`Removed ${chalk.cyan(ws.id)} from waiting queue`));
    }
    
    // Remove from game pairs
    const pairIndex = gamePairs.findIndex(p => p.player1 === ws || p.player2 === ws);
    if (pairIndex !== -1) {
        const pair = gamePairs[pairIndex];
        const opponent = pair.player1 === ws ? pair.player2 : pair.player1;
        
        if (opponent && opponent.readyState === web_socket.OPEN) {
            opponent.send("opponent_disconnected");
            console.log(chalk.hex('#FFA500')(`Notified ${chalk.cyan(opponent.id)} that opponent disconnected`));
        }
        
        gamePairs.splice(pairIndex, 1);
        console.log(chalk.red(`Game ${chalk.yellow(pair.gameId)} ended due to disconnection`));
    }
    console.log(chalk.gray(`Server status: ${chalk.green(waitingClients.length)} waiting, ${chalk.blue(gamePairs.length)} active games`));
}

process.on('SIGINT', () => {
    console.log(chalk.red.bold('SIGINT received, shutting down gracefully...'));
    clearInterval(heartbeat);
    socket.close();
    httpServer.close();
});