﻿

Console.WriteLine("Hello, World!");

DatabaseBuilder db = new DatabaseBuilder();

SocketServer wsServer = new SocketServer(9000);
wsServer.start();
WebServer webserver = new WebServer(9000);
webserver.start();



Console.WriteLine("Press ENTER to exit");
Console.ReadLine();