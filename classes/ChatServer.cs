using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class ChatServer
{
    private List<TcpClient> _clients = new List<TcpClient>();
    private List<ClientInfo> clients = new List<ClientInfo>();

    public void RunMainLoop(int port)
    {

        TcpListener _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        Console.WriteLine($"Server running on port {port}...");

        while (true)
        {
            ClientInfo cinfo = new ClientInfo();
            cinfo.tcpClient = _listener.AcceptTcpClient();
            lock (clients) clients.Add(cinfo);
            Console.WriteLine($"A new client has connected on IP address: {cinfo.tcpClient.Client.RemoteEndPoint}");
            Thread thread = new Thread(() => HandleClient(cinfo.tcpClient));
            thread.Start();
        }
    }

    private void HandleClient(TcpClient client)
    {
        using var reader = new StreamReader(client.GetStream());
        using var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };

        string? username = reader.ReadLine();
        var clientInfo = new ClientInfo { tcpClient = client, Username = username };
        lock (clients) clients.Add(clientInfo);
        Broadcast($"*** {username} has connected to the server ***", client);
        Console.WriteLine($"{username} is connected!");


        try
        {
            string? message;
            while ((message = reader.ReadLine()) != null)
            {
                if (message.StartsWith("/"))
                {
                    HandleCommand(message, clientInfo);
                }
                else
                {
                    Broadcast($"{username}: {message}", client);
                    Console.WriteLine($"{username}: {message}");
                }
            }
        }
        catch { }
        lock (clients) clients.Remove(clientInfo);
        Broadcast($"*** {username} has diconnected from the server! ***", client);
        client.Close();
    }

    private void Broadcast(string? message, TcpClient exclude)
    {
        lock (clients)
        {
            foreach (var client in clients)
            {
                if (client.tcpClient != exclude)
                {
                    var writer = new StreamWriter(client.tcpClient.GetStream()) { AutoFlush = true };
                    writer.WriteLine(message);
                }
            }
        }
    }

    private void HandleCommand(string? command, ClientInfo? sender)
    {
        using var writer = new StreamWriter(sender.tcpClient.GetStream()) { AutoFlush = true };
        var parts = command?.Split(' ', 3);

        switch (parts[0])
        {
            case "/users":
                lock (clients)
                {
                    string list = string.Join(", ", clients.Select(c => c.Username));
                    writer.WriteLine("*** list of connected users: " + list);
                }
                break;

            case "/whisper":
                if (parts.Length < 3)
                {
                    writer.WriteLine("*** Usage of the whisper command: /whisper <username> <message>");
                    return;
                }
                string target = parts[1];
                string msg = parts[2];
                var reciever = clients.FirstOrDefault(c => c.Username == target);
                if (reciever != null)
                {
                    var reciverWriter = new StreamWriter(reciever.tcpClient.GetStream()) { AutoFlush = true };
                    reciverWriter.WriteLine($"[Direct message] {sender.Username}: {msg}");
                }
                else
                {
                    writer.WriteLine("*** The username was not found!");
                }
                break;
            case "/exit":
                writer.WriteLine("*** You have successfully disconnected from the server!");
                sender.tcpClient.Close();
                break;
            default:
                writer.WriteLine("Unknown command!");
                break;
        }
    }
}