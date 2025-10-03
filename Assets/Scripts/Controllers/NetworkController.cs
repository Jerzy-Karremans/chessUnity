using System.Collections;
using System.Collections.Generic;
using NativeWebSocket;
using UnityEngine;

public class NetworkController : MonoBehaviour
{
    private WebSocket websocket;
    private string pendingMoveJson = null;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    
    private async void InitializeWebSocket()
    {
        websocket = new WebSocket("wss://chess.jerzykarremans.com/ws");

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connection opened!");
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError($"WebSocket error: {e}");
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log($"WebSocket connection closed: {e}");
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            pendingMoveJson = message;
        };

        await websocket.Connect();
    }
}
