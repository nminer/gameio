using server;
using server.mapObjects;
using System.Drawing.Imaging;

int port = 9000;

Console.WriteLine("Here we go! Hello, World!");

DatabaseBuilder db = new DatabaseBuilder();

//Shape ts = new Shape();
//ts.AddPoint(1,2).AddPoint(3,4);
//ts.IsClosedShape = true;
//string j = ts.ToJson();
//ts.Save("test shape");

//Shape tsreload = new Shape(1);

//Point p1 = new Point(1,2);
//Point p2 = new Point(3,4);
//Point point3 = p1 + p2;

//object o = new { bing = "bing" };
//object o2 = new { bing = "bing" };
//object[] list = { o, o2 };


//UserSystem.CreateNewUser("nminer", "caya");
//UserSystem.CreateNewUser("test", "test");

//Map.Create("Home", "img/maps/houseinside.png");
//Map? outside = Map.Create("outside", "img/maps/main_map.png");
//Portal.Create("Door out", 1, 60, 720, 2, 1205, 2692);
//Portal.Create("Door In", 2, 1205, 2670, 1, 60, 700);
//GameImage? tree = GameImage.CreateNewImage("tree1", "img/maps/objects/tree1.png");
//Shape shape = new Shape(20, 100);
//shape.Save("20x100");
//Solid? solid = Solid.Create(shape, shapePosition: new Point(88, 225), imageId: tree.ImageId, drawOrder: new Point(240, 125));
//MapSolid? mapSolid = MapSolid.Create(outside, solid, new Point(900, 2690));
//MapSolid? mapSolid2 = MapSolid.Create(outside, solid, new Point(1020, 3000));


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

