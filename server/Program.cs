using server;
int port = 9000;

Console.WriteLine("Hello, World!");

DatabaseBuilder db = new DatabaseBuilder();

UserSystem.CreateNewUser("nminer", "caya");
UserSystem.CreateNewUser("test", "test");
SocketServer wsServer = new SocketServer(port + 2);
wsServer.start();
WebServer webserver = new WebServer(port);
webserver.start();



Console.WriteLine("Press ENTER to exit");
Console.ReadLine();
