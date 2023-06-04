using Newtonsoft.Json;
using server;
using System;
using System.Collections.Concurrent;
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
        private static ConcurrentDictionary<Guid, User> socketIdToUser = new ConcurrentDictionary<Guid, User>();

        /// <summary>
        /// keeps a list of maps
        /// </summary>
        private static ConcurrentDictionary<Int64, Map> mapIdToMaps = new ConcurrentDictionary<Int64, Map>();

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
            socketIdToUser.TryAdd(socketId, user);
            //add the user to the map.
            Int64 mapId = user.Map_Id;
            if (!mapIdToMaps.ContainsKey(mapId))
            {
                Map newMap = new Map(mapId);
                if (newMap != null)
                {
                    mapIdToMaps.TryAdd(mapId, newMap);
                }          
            }
            if (mapIdToMaps.ContainsKey(mapId)) {
                SocketServer.SendOutMap(socketId, mapIdToMaps[mapId]);
                // will need to get the x and y at this point.
                // coords need to be checked to make sure user can be placed in the right place or near by.
                mapIdToMaps[mapId].AddUser(user, user.X_Coord, user.Y_Coord);
            }
        }

        public static void PlayerLeave(Guid socketId, User user)
        {
            socketIdToUser.TryRemove(socketId, out _);
            if (mapIdToMaps.ContainsKey(user.Map_Id))
            {
                mapIdToMaps[user.Map_Id].RemoveUser(user);
            }
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
