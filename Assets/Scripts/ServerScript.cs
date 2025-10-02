using WebSocketSharp;
using UnityEngine;
using System.Net;

public class ServerScript : MonoBehaviour
{
    WebSocket ws;
    void Start()
    {
        ws = new WebSocket("ws://localhost:8080");
        ws.OnMessage += (sender, e) =>
        {
            Debug.Log($"message recieved from {((WebSocket)sender).Url}, Data : {e.Data}");
        };
        ws.Connect();
    }

    public void Ping()
    {
        if (ws == null) return;

        ws.Send("ping");
    }

    public void SendMove(string boardJson)
    {
        if (ws == null) throw new WebException();

        ws.Send(boardJson);
    }
}
