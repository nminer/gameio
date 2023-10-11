using server;
using server.mapObjects;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

int port = 9000;

Console.WriteLine("Here we go! Hello, World!");

//RandomCurve curv = new RandomCurve(4, 20, 200, 5);
//List<int> list = new List<int>();
//for (double i = 0; i < 100; i = i + 0.01)
//{
//    list.Add((int)curv.GetY(i));
//}
Shape testShap = new Shape();
testShap.AddPoint(1,1).AddPoint(1,5).AddPoint(10,5).AddPoint(10,1);
bool inshap = Shape.IsPointInShape(testShap, new Point());
bool inshap2 = Shape.IsPointInShape(testShap, new Point(2,2));
Shape testShap2 = new Shape();
testShap2.AddPoint(1, 1).AddPoint(1, 5).AddPoint(1.5, 1.5).AddPoint(10, 5).AddPoint(10, 1);
bool inshap3 = Shape.IsPointInShape(testShap2, new Point());
bool inshap4 = Shape.IsPointInShape(testShap2, new Point(2, 2));
bool inshap5= Shape.IsPointInShape(testShap2, new Point(4, 2));

Circle c = new Circle(new Point(2, 2), 1);
Point p = c.PointOnCircumference(360);


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

Dictionary<string, string> commands = new Dictionary<string, string>
        {
            { "/help | /?", "Returns all commands" },
            { "/setTime #", "set the in-game time in hours of the day. 0-24." },
            { "/exit", "Stop and exit the server. This will shut down the server!" }
        };

Console.WriteLine("Type 'exit' to stop server and exit");
bool exitLoop = false;
while (!exitLoop)
{
    string line = Console.ReadLine();
    if (line != null && line.Equals("/exit")) {
        exitLoop = true;
    } else
    {
        if (line.StartsWith("/setTime"))
        {
            Match match = Regex.Match(line, @"\d+");
            if (match.Success)
            {
                int amount = Int32.Parse(match.Value);
                GameServer.AdminSetGameTimeHour(amount);
            }
        } else if (line.StartsWith("/?") || line.StartsWith("/help")) {
            Console.WriteLine("Commands to be used server side:");
            foreach (KeyValuePair<string,string> kvp in commands)
            {
                string spaces = "                    ";
                spaces = spaces.Substring(0, spaces.Length - kvp.Key.Length);
                Console.WriteLine($"    {kvp.Key} {spaces} {kvp.Value}");
            }            
        }
        else
        {
            SocketServer.SendServerMessage(line, "Server");
        }        
    }
}

