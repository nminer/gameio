
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Text;
using WatsonWebserver;
using WatsonWebsocket;
namespace server
{
    /// <summary>
    /// this is a class to keep all the web socket stuff in.
    /// </summary>
    internal class SocketServer
    {
        /// <summary>
        /// the server
        /// </summary>
        private static WatsonWsServer wsserver;

        /// <summary>
        /// create a new Socketserver with passed in port.
        /// </summary>
        /// <param name="port"></param>
        public SocketServer(List<string> hostNames, int port)
        {
            wsserver = new WatsonWsServer(hostNames, port);
            wsserver.ClientConnected += ClientConnected;
            wsserver.ClientDisconnected += ClientDisconnected;
            wsserver.MessageReceived += ClientMessage;
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
            User? user = UserSystem.GetUserFromSocketId(args.Client.Guid.ToString());
            if (user == null) { return; }
            UserSystem.Logout(user.UserName);
        }


        static void ClientMessage(object sender, MessageReceivedEventArgs args)
        {
            string message = Encoding.UTF8.GetString(args.Data);
            Console.WriteLine("Client Message: " + message);
            JObject json = JObject.Parse(message);
            if (json == null || json.First == null) 
            {
                return;
            }
            switch (json.First.Path)
            {
                case "message":
                    SendOutMessage(args.Client.Guid.ToString(), json);
                    break;
                case "setId":
                    User? user = UserSystem.AssignSocketToSession((string)json["setId"], args.Client.Guid.ToString());
                    if (user != null)
                    {
                        wsserver.SendAsync(args.Client.Guid, JsonConvert.SerializeObject(new {setUser = user.UserName}));
                    }
                    break;
            }
            //foreach (ClientMetadata clientData in wsserver.ListClients())
            //{
            //    wsserver.SendAsync(clientData.Guid, message);
            //}
                
            
        }


        static void SendOutMessage(string socketId, JObject jsonMessage)
        {
            User? user = UserSystem.GetUserFromSocketId(socketId);
            if (user == null) { return; }
            string data = JsonConvert.SerializeObject(new {user = user.UserName, message = (string)jsonMessage["message"] });
            foreach (ClientMetadata clientData in wsserver.ListClients())
            {
                wsserver.SendAsync(clientData.Guid, data);
            }
        }


    }

}