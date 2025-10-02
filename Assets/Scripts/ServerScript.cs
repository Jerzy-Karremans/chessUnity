using WebSocketSharp;
using UnityEngine;

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
}
