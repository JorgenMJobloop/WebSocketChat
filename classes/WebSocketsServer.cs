using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;

public class WebSocketsServer
{
    private List<WebSocket> _clients = new List<WebSocket>();
    public async Task RunWebSocket()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5000/");
        listener.Start();
        Console.WriteLine("Websocket server is running on ws://localhost:5000/chat");

        while (true)
        {
            var context = await listener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                var webSocketContext = await context.AcceptWebSocketAsync(null);
                var ws = webSocketContext.WebSocket;
                lock (_clients) _clients.Add(ws);
                _ = Task.Run(() => HandleClient(ws));
            }
        }
    }
    /// <summary>
    /// Optional logging on the server.
    /// </summary>
    /// <param name="content">the content to pass along to the log-file</param>
    private void Logs(string content)
    {
        Logger.LogToFile("logs.txt", content);
    }

    private async Task HandleClient(WebSocket socket)
    {
        var buffer = new byte[1024];

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine(">>" + message);
                await Broadcast(message, socket);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("An error occured:" + e.Message);
        }
        finally
        {
            lock (_clients) _clients.Remove(socket);
        }
    }

    private async Task Broadcast(string? message, WebSocket excludeSocket)
    {
        var data = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(data);
        lock (_clients)
        {
            foreach (var client in _clients)
            {
                if (client.State == WebSocketState.Open)
                {
                    client.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}