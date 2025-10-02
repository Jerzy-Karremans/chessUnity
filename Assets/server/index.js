const web_socket = require("ws");
const express = require("express")
const path = require("path")

const app = express();
const PORT = 25561;
const http_port = 25562;

app.use(express.static(path.join(__dirname, "public")));
app.listen(http_port, () => {
    console.log(`Server is running on http://localhost:${PORT}`);
});


const socket = new web_socket.Server({ port:PORT }, () => {
    console.log("server started");
})


let waitingClients = [];
let gamePairs = [];

socket.on("connection", function connection(ws) {
    console.log("client connected");
    
    // Add unique ID to the client
    ws.id = Math.random().toString(36).substr(2, 9);
    
    pairClient(ws);

    ws.on("message", (data) => {
        console.log(`Message from ${ws.id}: ${data.toString()}`);
        forwardMoveToOpponent(ws, data.toString());
    })

    ws.on("close", () => {
        console.log(`Client ${ws.id} disconnected`);
        removeClient(ws);
    })
})

function forwardMoveToOpponent(sender, moveData) {
    // Find the game pair this client belongs to
    const pair = gamePairs.find(p => p.player1 === sender || p.player2 === sender);
    
    if (pair) {
        const opponent = pair.player1 === sender ? pair.player2 : pair.player1;
        console.log(`Forwarding move from ${sender.id} to ${opponent.id}`);
        opponent.send(moveData);
    } else {
        console.log(`No pair found for client ${sender.id}`);
    }
}

function pairClient(ws) {
    if (waitingClients.length > 0) {
        console.log("Pairing clients");
        const opponent = waitingClients.shift();
        const gameId = Math.random().toString(36).substr(2, 9);

        const pair = {
            gameId: gameId,
            player1: opponent,
            player2: ws
        };

        gamePairs.push(pair);
        
        // Notify both clients they are connected
        opponent.send("connected");
        ws.send("connected");
        
        console.log(`Game ${gameId} created with players ${opponent.id} and ${ws.id}`);
    } else {
        waitingClients.push(ws);
        ws.send("waiting");
        console.log(`Client ${ws.id} is waiting for opponent`);
    }
}

function removeClient(ws) {
    // Remove from waiting list
    waitingClients = waitingClients.filter(client => client !== ws);
    
    // Remove from game pairs
    const pairIndex = gamePairs.findIndex(p => p.player1 === ws || p.player2 === ws);
    if (pairIndex !== -1) {
        const pair = gamePairs[pairIndex];
        const opponent = pair.player1 === ws ? pair.player2 : pair.player1;
        
        // Notify opponent that their partner disconnected
        if (opponent && opponent.readyState === web_socket.OPEN) {
            opponent.send("opponent_disconnected");
            console.log(`Notified ${opponent.id} that opponent disconnected`);
        }
        
        gamePairs.splice(pairIndex, 1);
        console.log(`Game ${pair.gameId} ended due to disconnection`);
    }
}

socket.on("listening", () => {
    console.log(`listening on ${PORT}`);
})