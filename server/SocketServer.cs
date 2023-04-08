
using WatsonWebsocket;

/// <summary>
/// this is a class to keep all the web socket stuff in.
/// </summary>
internal class SocketServer
{
    /// <summary>
    /// the server
    /// </summary>
    private WatsonWsServer wsserver;

    /// <summary>
    /// create a new Socketserver with passed in port.
    /// </summary>
    /// <param name="port"></param>
    public SocketServer(int port)
    {
        wsserver = new WatsonWsServer("localhost", port);
        wsserver.ClientConnected += ClientConnected;
        wsserver.ClientDisconnected += ClientDisconnected;
    }

    /// <summary>
    /// start the server
    /// </summary>
    public void start()
    {
        wsserver.Start();
    }

    /// <summary>
    /// called when a new client connects
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    static void ClientConnected(object sender, WatsonWebsocket.ConnectionEventArgs args)
    {
        Console.WriteLine("Client connected: " + args.Client.ToString());
    }

    /// <summary>
    /// called when a client dis connects.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    static void ClientDisconnected(object sender, DisconnectionEventArgs args)
    {
        Console.WriteLine("Client disconnected: " + args.Client.ToString());
    }
}

