const web_socket = require("ws")
const socket = new web_socket.Server({ port:8080 }, () => {
    console.log("server started");
})

socket.on("connection", function connection(ws) {
    console.log("client connected");

    ws.on("message", (data) => {
        if(data.toString() === "ping") {
            console.log("got pinged")
            ws.send("Pong");
        } else {
            console.log(data.toString())
        }


    })
})

socket.on("listening", () => {
    console.log("listening on 8080");
})