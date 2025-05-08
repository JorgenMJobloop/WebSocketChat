using System.Net.Sockets;

public class ClientInfo
{
    public TcpClient? tcpClient { get; set; }
    public string? Username { get; set; } = "Guest";
}