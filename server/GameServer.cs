using Newtonsoft.Json;
using server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace server
{

    static class GameServer
    {
        private static int Rate = 500;

        /// <summary>
        /// Logged in users
        /// </summary>
        private static Dictionary<Guid, User> socketIdToUser = new Dictionary<Guid, User>();


        private static Timer? PlayerUpdateTimer;

        public static void PlayerJoing(Guid socketId, User user)
        {
            socketIdToUser.Add(socketId, user);
        }

        public static void PlayerLeave(Guid socketId, User user) 
        {
            socketIdToUser.Remove(socketId);
        }

        public static void StartPlayerUpdate()
        {
            if (PlayerUpdateTimer == null)
            {
                PlayerUpdateTimer = new Timer(new TimerCallback(UpdatePlayers),null,0,Rate);
            }
        }

        private static void UpdatePlayers(object? state)
        {
            // build the state of the games.
            // right now just players update
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.None;
                writer.WriteStartObject();
                writer.WritePropertyName("Players");
                writer.WriteStartArray();
                foreach (User user in socketIdToUser.Values)
                {
                    writer.WriteRawValue(user.GetJson());
                }
                writer.WriteEnd();
                writer.WriteEndObject();
            }
            foreach (KeyValuePair<Guid, User> keyValuePair in socketIdToUser)
            {
                   SocketServer.SendOutUpdate(keyValuePair.Key, sb.ToString());
            }
        }

    }
}
