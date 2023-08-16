using Newtonsoft.Json;
using server;
using server.mapObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
            Map map = GetMapById(user.Map_Id);
            SocketServer.SendOutMap(socketId, map);
            // will need to get the x and y at this point.
            // coords need to be checked to make sure user can be placed in the right place or near by.
            map.AddUser(user, user.X_Coord, user.Y_Coord);
        }

        public static void ChangeUserMap(User user, Int64 targetMapId, double targetX, double targetY)
        {
            Map oldMap = GetMapById(user.Map_Id);
            Map newMap = GetMapById(targetMapId);
            oldMap.RemoveUser(user);
            Guid? socketId = UserSystem.GetSocketIdFromUserName(user.UserName);
            if (socketId != null)
            {
                SocketServer.SendOutMap((Guid)socketId, newMap);
            }     
            newMap.AddUser(user, targetX, targetY);
        }

        public static void CheckForPortal(User user)
        {
            Map playersMap = GetMapById(user.Map_Id);
            Portal portalToUser = playersMap.CheckUsePortal(user);
            if (portalToUser != null)
            {
                ChangeUserMap(user, portalToUser.TargetMapId, portalToUser.Target_X_Coord, portalToUser.Target_Y_Coord);
            }
        }

        public static void AddGameSoundAffect(Int64 mapId, SoundAffect sound)
        {
            Map map = GetMapById(mapId);
            map.AddSoundAffect(sound);
        }

        public static Map GetMapById(Int64 mapId)
        {
            if (!mapIdToMaps.ContainsKey(mapId))
            {
                Map newMap = new Map(mapId);
                if (newMap != null)
                {
                    mapIdToMaps.TryAdd(mapId, newMap);
                }
            }
            return mapIdToMaps[mapId];
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
            foreach (Map map in mapIdToMaps.Values)
            {
                map.TickGameUpdate();
            }
        }

        private static void UpdatePlayersFrames(object? state)
        {
            foreach (Map map in mapIdToMaps.Values)
            {
                List<User> mapUsers = map.GetUsers();
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
                    foreach (User user in mapUsers)
                    {
                        writer.WriteRawValue(user.GetJson());
                    }
                    writer.WriteEnd();
                    writer.WritePropertyName("soundAffects");
                    writer.WriteRawValue(map.GetAllJsonSoundAffects());
                    writer.WriteEndObject();
                }
                foreach (User user in mapUsers)
                {
                    Guid? guid = UserSystem.GetSocketIdFromUserName(user.UserName);
                    if (guid != null)
                    {
                        SocketServer.SendOutUpdate((Guid)guid, sb.ToString());
                    }      
                }
            }
        }

    }
}
