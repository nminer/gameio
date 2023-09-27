using Newtonsoft.Json;
using server.mapObjects;
using server.mods;
using server.world;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
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

        private static GameDayTimer? DayTimer;

        private static Storm? CurentStorm;

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

        /// <summary>
        /// check the map to see if the user can use any thing close by
        /// </summary>
        /// <param name="user"></param>
        public static void CheckForUserUse(User user)
        {
            Map playersMap = GetMapById(user.Map_Id);
            Portal? portalToUser = playersMap.CheckUsePortal(user);
            if (portalToUser != null)
            {
                ChangeUserMap(user, portalToUser.TargetMapId, portalToUser.Target_X_Coord, portalToUser.Target_Y_Coord);
                return;
            }
            SoulStone? soulStone = playersMap.CheckUseSoulStone(user);
            if (soulStone != null)
            {
                user.SetSoulStone();
                return;
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
            if (DayTimer == null)
            {
                DayTimer = new GameDayTimer(1200000); //1200000 = 20 minute days
            }
        }

        private static object getWorldTimeUpdate()
        {
            TimeSpan gameRunTime = DayTimer.GetGameTime();
            return new { day = gameRunTime.Days, hour = gameRunTime.Hours, minutes = gameRunTime.Minutes, seconds = gameRunTime.Seconds };
        }

        public static TimeSpan GetWorldTime()
        {
            return DayTimer.GetGameTime();
        }

        public static TimeSpan MillisecondsToGameTime(long milliseconds)
        {
            return DayTimer.MillisecondsToGameTime(milliseconds);
        }

        public static long GameTimeSpanToMilliseconds(TimeSpan time)
        {
             return DayTimer.GameTimeSpanToMilliseconds(time);
        }

        private static object getWorldSkyUpdate(Map map)
        {
            string skyColor = "#003";
            double amount = 0;
            if (!map.Outside)
            {
                return new { color = skyColor, amount = amount };
            }
            TimeSpan gametime = DayTimer.GetGameTime();
            if (gametime.Hours >= 6 && gametime.Hours < 20)
            {
                if (CurentStorm != null && !CurentStorm.Finished)
                {
                    skyColor = CurentStorm.skyColor;
                }
                amount = 0;
            } else if (gametime.Hours >= 20 && gametime.Hours <= 21)
            {
                int spanseconds = ((gametime.Hours - 20) * 60 * 60) + (gametime.Minutes * 60);
                int totaleSeconds = 2 * 60 * 60;
                amount = ((double)spanseconds / (double)totaleSeconds) * 0.6;
            } else if ((gametime.Hours >= 22 && gametime.Hours <= 24) || (gametime.Hours >= 0 && gametime.Hours < 4))
            {
                amount = 0.6;
            } else if (gametime.Hours >= 4 && gametime.Hours <= 5)
            {
                int spanseconds = ((gametime.Hours - 4) * 60 * 60) + (gametime.Minutes * 60);
                int totaleSeconds = 2 * 60 * 60;
                amount = 0.6 - (((double)spanseconds / (double)totaleSeconds) * 0.6);
            }
            if (CurentStorm != null && !CurentStorm.Finished)
            {
                amount += Mods.ConvertRange(0, 200, 0, 0.2, CurentStorm.GetLastAmount());
                if (amount > 0.6)
                {
                    amount = 0.6;
                }
            }
            return new { color = skyColor, amount = amount};
        }

        private static object getStormUpdate(Map map)
        {
            if (CurentStorm is null)
            {
                CurentStorm = Storm.GetNextStorm();
            } else if (CurentStorm.Finished)
            {
                CurentStorm = null;
            }
            if (!map.Outside || CurentStorm is null)
            {
                return new {amount = 0};
            }
            return new { amount = CurentStorm.GetStormAmount()};
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
            LightningStrike? strike = null;
            if (CurentStorm is not null && !CurentStorm.Finished)
            {
                strike = CurentStorm.GetLightning();
            }
            foreach (Map map in mapIdToMaps.Values)
            {
                // add lightning sound to map if it is outside.
                if (map.Outside && strike is not null)
                {
                    map.AddFullMapSoundEffect(strike.Thunder);
                }
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
                    writer.WritePropertyName("fullMapSoundEffects");
                    writer.WriteRawValue(map.GetAllJsonFullMapSoundEffects());
                    writer.WritePropertyName("visualEffects");
                    writer.WriteRawValue(map.GetAllJsonVisualEffects());
                    writer.WritePropertyName("damages");
                    writer.WriteRawValue(map.GetAllJsonDamages());
                    writer.WritePropertyName("monsters");
                    writer.WriteRawValue(map.getAllJsonMonster());                    
                    writer.WritePropertyName("time");
                    writer.WriteRawValue(JsonConvert.SerializeObject(getWorldTimeUpdate()));
                    writer.WritePropertyName("sky");
                    writer.WriteRawValue(JsonConvert.SerializeObject(getWorldSkyUpdate(map)));
                    if (map.Outside && strike is not null && strike.FlashAmount > 0)
                    {
                        writer.WritePropertyName("lightning");
                        writer.WriteRawValue(strike.getJasonString());
                    }
                    writer.WritePropertyName("storm");
                    writer.WriteRawValue(JsonConvert.SerializeObject(getStormUpdate(map)));
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

        public static void AdminSetGameTimeHour(int hour)
        {
            DayTimer.setTimeOfDay(hour);
        }
    }
}
