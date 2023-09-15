using server;
using server.mapObjects;
using System.Drawing.Imaging;

int port = 9000;

Console.WriteLine("Here we go! Hello, World!");

//RandomCurve curv = new RandomCurve(4, 20, 200, 5);
//List<int> list = new List<int>();
//for (double i = 0; i < 100; i = i + 0.01)
//{
//    list.Add((int)curv.GetY(i));
//}

DatabaseBuilder db = new DatabaseBuilder();

MapBuilder.BuildOutside();

List<string> hostNames = new List<string> { "127.0.0.1", "localhost" };
string? localip = WebServer.GetIpAddress();
if (localip != null)
{
    hostNames.Add(localip);
}

SocketServer wsServer = new SocketServer(hostNames, port + 2);
wsServer.start();
WebServer webserver = new WebServer(hostNames, port);
webserver.start();
GameServer.StartPlayerUpdate();
Console.WriteLine("Server up on:");
Console.WriteLine(string.Join(", ", hostNames.ToArray()));
Console.WriteLine("port: " + port);


Console.WriteLine("Type 'exit' to stop server and exit");
bool exitLoop = false;
while (!exitLoop)
{
    string line = Console.ReadLine();
    if (line != null && line.Equals("exit")) {
        exitLoop = true;
    } else
    {
        SocketServer.SendServerMessage(line, "Server");
    }
}

