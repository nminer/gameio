﻿using server;
using server.mapObjects;
using System.Drawing.Imaging;

int port = 9000;

Console.WriteLine("Here we go! Hello, World!");

DatabaseBuilder db = new DatabaseBuilder();

Shape ts = new Shape();
ts.AddPoint(1,2).AddPoint(3,4);
ts.IsClosedShape = true;
string j = ts.ToJson();
ts.Save("test shape");

Shape tsreload = new Shape(1);

//UserSystem.CreateNewUser("nminer", "caya");
//UserSystem.CreateNewUser("test", "test");

//Map.Create("Home", "img/maps/houseinside.png");
//Map.Create("outside", "img/maps/main_map.png");
//Portal.Create("Door out", 1, 60, 720, 2, 1205, 2692);
//Portal.Create("Door In", 2, 1205, 2670, 1, 60, 700);

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

