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
        private static int FrameRate = 20;
        private static int UpdateRate = 20;

        /// <summary>
        /// Logged in users
        /// </summary>
        private static Dictionary<Guid, User> socketIdToUser = new Dictionary<Guid, User>();

        /// <summary>
        /// timer to send frames to players
        /// </summary>
        private static Timer? PlayerFrameUpdateTimer;

        /// <summary>
        /// timer to update game (moves monsters...)
        /// </summary>
        private static Timer? GameUpdateTimer;

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
            if (PlayerFrameUpdateTimer == null)
            {
                PlayerFrameUpdateTimer = new Timer(new TimerCallback(UpdatePlayersFrames),null,0,FrameRate);
            }
            if (GameUpdateTimer == null)
            {
                GameUpdateTimer = new Timer(new TimerCallback(UpdateGame), null, 0, UpdateRate);
            }
        }

        private static void UpdateGame(object? state)
        {
            foreach (User user in socketIdToUser.Values)
            {
                user.TickGameUpdate();
            }
        }

        private static void UpdatePlayersFrames(object? state)
        {
            // build the state of the games.
            // right now just players update
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.None;
                writer.WriteStartObject();
                writer.WritePropertyName("players");
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
