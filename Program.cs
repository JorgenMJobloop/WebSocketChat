namespace ChatAppServerCMD;

class Program
{
    static async Task Main(string[] args)
    {
        WebSocketsServer webSocketsServer = new WebSocketsServer();
        // Create a new Task, and pass the RunWebSocket method as the argument.
        Task task = webSocketsServer.RunWebSocket();
        // Await the task when Program.cs is called.
        await task;
    }
}
