﻿
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using server.monsters;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Net.Sockets;
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
        private static Dictionary<string, string> PlayerCommands = new Dictionary<string, string>{
            { "/help | /?", "Returns all commands" },
            { "/location", "Returns the players location and map. Copies the location to clipboard." },
            { "/deaths", "Return the number of deaths for the player." },
            { "/time", "Return the in-game time of day." }
        };

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
            if (UserSystem.Logout(user.UserName))
            {
                SendServerMessage($"{user.UserName} has logged out.", "Server");
                string data = JsonConvert.SerializeObject(new { userDisconnect = user.UserName });
                foreach (ClientMetadata clientData in wsserver.ListClients())
                {
                    wsserver.SendAsync(clientData.Guid, data);
                }
            }
        }


        static void ClientMessage(object sender, MessageReceivedEventArgs args)
        {
            string message = Encoding.UTF8.GetString(args.Data);
            //Console.WriteLine("Client Message: " + message);
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
                case "privateMessage":
                    SendPrivateMessage(args.Client.Guid.ToString(), json);
                    break;
                case "setId":
                    SetId(args.Client.Guid, json);
                    break;
                case "movement":
                    PlayerMove(args.Client.Guid, json);
                    break;
                case "command":
                    PlayerCommand(args.Client.Guid, json);
                    break;
                case "monsterRequest":
                    PlayerMonsterRequest(args.Client.Guid, json);
                    break;
            }
            //foreach (ClientMetadata clientData in wsserver.ListClients())
            //{
            //    wsserver.SendAsync(clientData.Guid, message);
            //}
                
            
        }

        static void PlayerMonsterRequest(Guid guid, JObject jsonMessage)
        {
            string command = (string)jsonMessage["monsterRequest"];
            long  x = 0;
            if (Int64.TryParse(command, out x))
            {
                MonsterType mt = new MonsterType(x);
                wsserver.SendAsync(guid, mt.GetJsonMonsterTypeString());
            }
        }

        static void PlayerCommand(Guid guid, JObject jsonMessage)
        {
            User? user = UserSystem.GetUserFromSocketId(guid.ToString());
            if (user == null)
            {
                return;
            }
            string command = (string)jsonMessage["command"];
            if (command.StartsWith("/location")) {
                string locStr = $"Location - ({Convert.ToInt32(user.X_Coord)} , {Convert.ToInt32(user.Y_Coord)}) x:{user.X_Coord}, y:{user.Y_Coord}, map:{GameServer.GetMapById(user.Map_Id).Name}";
                SendServerMessage(locStr, "Information", guid);
            } else if (command.StartsWith("/time"))
            {
                TimeSpan t = GameServer.GetWorldTime();
                SendServerMessage($"In Game Time: {t.Hours.ToString("00")}:{t.Minutes.ToString("00")}" , "Information", guid);
            } else if (command.StartsWith("/deaths"))
            {
                SendServerMessage($"Deaths: {user.Deaths}, Death Penalty: %{(int)(user.Death_Points * 100)}", "Information", guid);
            } else if (command.StartsWith("/?") || command.StartsWith("/help")){
                string re = "Commands:";
                foreach (KeyValuePair<string, string> kvp in PlayerCommands)
                {
                    string spaces = "                    ";
                    spaces = spaces.Substring(0, spaces.Length - kvp.Key.Length);
                    spaces = spaces.Replace(" ", "&nbsp;");
                   re += "<br />" + $"{kvp.Key} {spaces} {kvp.Value}";
                }
                SendServerMessage(re, "Information", guid);
            }
        }

        static void PlayerMove(Guid guid, JObject jsonMessage)
        {
            User? user = UserSystem.GetUserFromSocketId(guid.ToString());
            if (user != null)
            {
               user.UpdateFromPlayer((JObject)jsonMessage["movement"]);
            }
        }

        static void SetId(Guid guid, JObject jsonMessage)
        {
            User? user = UserSystem.AssignSocketToSession((string)jsonMessage["setId"], guid);
            if (user != null)
            {
                // we have a user for the token
                wsserver.SendAsync(guid, JsonConvert.SerializeObject(new { setUser = user.UserName }));
                SendServerMessage($"{user.UserName} has logged in.", "Server");
                string data = JsonConvert.SerializeObject(new { userConnect = user.UserName });
                foreach (ClientMetadata clientData in wsserver.ListClients())
                {
                    wsserver.SendAsync(clientData.Guid, data);
                    if (clientData.Guid != guid)
                    {
                        // add all logged in user to new users list.
                        User? loggedInUser = UserSystem.GetUserFromSocketId(clientData.Guid.ToString());
                        if (loggedInUser != null)
                        {
                            string addUserData = JsonConvert.SerializeObject(new { userConnect = loggedInUser.UserName });
                            wsserver.SendAsync(guid, addUserData);
                        }
                    }
                }

            }
            else
            {
                // no user for the token make them sign in again.
                wsserver.DisconnectClient(guid);
                Console.WriteLine("Client was closed: No user for set id token.");
            }
        }

        /// <summary>
        /// send a private message from one user to another.
        /// </summary>
        /// <param name="socketId">senders sockt id string</param>
        /// <param name="jsonMessage">the json object with the private message</param>
        static void SendPrivateMessage(string socketId, JObject jsonMessage)
        {
            string message = (string)jsonMessage["privateMessage"];
            string reciverUserName = (string)jsonMessage["reciver"];
            User? user = UserSystem.GetUserFromSocketId(socketId);
            Guid? senderGuid = UserSystem.GetSocketIdFromUserName(user.UserName);
            Guid? reciverSocketId = UserSystem.GetSocketIdFromUserName(reciverUserName);
            if (reciverSocketId.HasValue && user != null && senderGuid.HasValue)
            {
                Guid sendto = reciverSocketId.Value; 
                // send the message to the user.
                string data = JsonConvert.SerializeObject(new { user = user.UserName, privateMessage = message });
                wsserver.SendAsync(sendto, data);
                if (sendto != senderGuid.Value)
                {
                    // send the message back to the sender to display.
                    string returnData = JsonConvert.SerializeObject(new { user = "You - " + reciverUserName, privateMessage = message });
                    wsserver.SendAsync(senderGuid.Value, returnData);
                }
            } else if (user != null && senderGuid.HasValue)
            {
                // server message that the user is not on.
                SendServerMessage($"{reciverUserName} is not logged in.", "Server", senderGuid.Value);
            }

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

        /// <summary>
        /// send a message to a user. pass in message and the from string.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <param name="messageFromString"></param>
        public static void SendMessageToUser(User user, string message, string messageFromString)
        {
            Guid? sendtoGuid = UserSystem.GetSocketIdFromUserName(user.UserName);
            SendServerMessage(message, messageFromString, sendtoGuid);
        }

        public static void SendServerMessage(string message, string fromUserName, Guid? socketId = null)
        {
            Console.WriteLine($"Client ({fromUserName}) Message: " + message);
            if (string.IsNullOrEmpty(message)) { return; };
            string data = JsonConvert.SerializeObject(new { user = fromUserName, serverMessage = message });
            if (socketId.HasValue)
            {
                wsserver.SendAsync(socketId.Value, data);
            } else
            {
                foreach (ClientMetadata clientData in wsserver.ListClients())
                {
                    wsserver.SendAsync(clientData.Guid, data);
                }
            }

        }

        private static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        /// <summary>
        /// send out the frame/update to a user
        /// </summary>
        /// <param name="socketId"></param>
        /// <param name="jsonUpdate"></param>
        public static void SendOutUpdate(Guid socketId, string jsonUpdate)
        {
            User? user = UserSystem.GetUserFromSocketId(socketId.ToString());
            if (user == null) { return; }
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.None;
                writer.WriteStartObject();
                writer.WritePropertyName("update");
                writer.WriteValue(GetTimestamp(DateTime.Now));
                writer.WritePropertyName("frame");
                writer.WriteRawValue(jsonUpdate);
                writer.WriteEndObject();
            }
            //string data = JsonConvert.SerializeObject(new { update = GetTimestamp(DateTime.Now), frame = jsonUpdate });
            wsserver.SendAsync(socketId, sb.ToString());
        }

        public static void SendOutMap(Guid socketId, Map map)
        {
            User? user = UserSystem.GetUserFromSocketId(socketId.ToString());
            if (user == null) { return; }
            wsserver.SendAsync(socketId, map.GetJson());
        }

    }

}