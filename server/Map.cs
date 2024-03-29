﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using server.mapObjects;
using System.Collections.Concurrent;
using System.Reflection.Metadata.Ecma335;
using server.monsters;

namespace server
{
    public class Map
    {
        /// <summary>
        /// keep the map database data thread safe.
        /// </summary>
        private object databaseLock = new object();

        /// <summary>
        /// keep the user list safe.
        /// </summary>
        private object userListLock = new object();

        private SQLiteDataAdapter adapter;

        /// <summary>
        /// used when getting and saving the map data
        /// </summary>
        private SQLiteCommandBuilder builder;

        /// <summary>
        /// the maps data set. used for saving changes
        /// </summary>
        private DataSet data;

        /// <summary>
        /// the maps dat row
        /// </summary>
        private DataRow row;

        /// <summary>
        /// user that are on the map
        /// </summary>
        private List<User> users = new List<User>();


        /// <summary>
        /// all the sound affects to send out on next update.
        /// </summary>
        private ConcurrentQueue<SoundAffect> soundAffects = new ConcurrentQueue<SoundAffect>();

        /// <summary>
        /// All the sounds effects that cove the full map.
        /// </summary>
        private ConcurrentQueue<FullMapSoundEffect> fullMapSoundEffects = new ConcurrentQueue<FullMapSoundEffect>();


        /// <summary>
        /// all the visual effects to send out on the next update.
        /// </summary>
        private ConcurrentQueue<VisualEffect> visualEffects = new ConcurrentQueue<VisualEffect>();

        /// <summary>
        /// all the damage to be sent out on next update
        /// </summary>
        private ConcurrentQueue<Damage> damages = new ConcurrentQueue<Damage>();

        /// <summary>
        /// The map id
        /// </summary>
        public Int64 Id
        {
            get
            {
                lock (databaseLock)
                {
                    return (Int64)row["Map_Id"];
                }
            }
        }

        /// <summary>
        /// The map Name
        /// </summary>
        public string Name
        {
            get
            {
                lock (databaseLock)
                {
                    return (string)row["MapName"];
                }
            }
        }

        /// <summary>
        /// The map height
        /// </summary>
        public Int64 Height
        {
            get
            {
                lock (databaseLock)
                {
                    return (Int64)row["Height"];
                }
            }
        }

        /// <summary>
        /// The map width
        /// </summary>
        public Int64 Width
        {
            get
            {
                lock (databaseLock)
                {
                    return (Int64)row["Width"];
                }
            }
        }

        /// <summary>
        /// The mape image path
        /// </summary>
        public string ImagePath
        {
            get
            {
                lock (databaseLock)
                {
                    return (string)row["ImagePath"];
                }
            }
        }

        /// <summary>
        /// true if map is an outside map.
        /// affected by time of day.
        /// </summary>
        public bool Outside
        {
            get
            {
                lock (databaseLock)
                {
                    return (Int64)row["Outside"] == 1;
                }
            }
        }
        

        /// <summary>
        /// keep the solids thread friendly.
        /// </summary>
        private object solidsLock = new object();
        /// <summary>
        /// a list of solid objects on the map.
        /// </summary>
        private List<ISolid> solids = new List<ISolid>();

        /// <summary>
        /// lock to keep the ports list safe
        /// </summary>
        private object portalsLock = new object();

        /// <summary>
        /// a list of all the portals
        /// </summary>
        private List<Portal> portals = new List<Portal>();

        /// <summary>
        /// lock to keep the ports list safe
        /// </summary>
        private object MapSolidsLock = new object();
        /// <summary>
        /// a list of all the portals
        /// </summary>
        private List<MapSolid> MapSolids = new List<MapSolid>();

        private object MapVisualsLock = new object();
        private List<MapVisual> MapVisuals = new List<MapVisual>();

        private object MapSoundsLock = new object();
        private List<MapSound> MapSounds = new List<MapSound>();

        /// <summary>
        /// lock to keep the lights list safe
        /// </summary>
        private object lightsLock = new object();
        private List<MapLight> MapLights = new List<MapLight>();

        /// <summary>
        /// lock to keep the SoulStones list safe
        /// </summary>
        private object SoulStoneLock = new object();
        private List<SoulStone> MapSoulStones = new List<SoulStone>();

        /// <summary>
        /// lock to keep the SoulStones list safe
        /// </summary>
        private object SpawnLock = new object();
        private List<MonsterSpawn> Spawns = new List<MonsterSpawn>();

        /// <summary>
        /// Load a user from its user id in the database.
        /// </summary>
        /// <param name="userId"></param>
        public Map(Int64 mapId)
        {
            LoadMap(mapId);
        }


        private void LoadMap(Int64 mapId)
        {
            lock (databaseLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findUser = $"SELECT * FROM Maps WHERE Map_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findUser, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", mapId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                row = data.Tables[0].Rows[0];    
            }
            // add the solid outline.
            lock (solidsLock)
            {
                solids.Add(new Solid(new Point(0, 0), Height, Width, false));
            }
            LoadPortals();
            LoadMapSolids();
            LoadMapVisuals();
            LoadMapSounds();
            LoadMapLights();
            LoadSoulStones();
            LoadSpawns();
        }

        private void LoadPortals()
        {
            lock (portalsLock)
            {
                SQLiteDataAdapter adapterPortals = new SQLiteDataAdapter();

                SQLiteCommandBuilder builderPortals = new SQLiteCommandBuilder(adapterPortals);

                DataSet dataPortals = new DataSet();
                string queryPortals = "SELECT * FROM Portals WHERE Map_Id=$id;";
                SQLiteCommand commandPortals = new SQLiteCommand(queryPortals, DatabaseBuilder.Connection);
                commandPortals.Parameters.AddWithValue("$id", Id);
                adapterPortals.SelectCommand = commandPortals;
                adapterPortals.Fill(dataPortals);
                if (dataPortals.Tables.Count > 0 && dataPortals.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow r in dataPortals.Tables[0].Rows)
                    {
                        portals.Add(new Portal((Int64)r["Portal_Id"]));
                    }
                    
                }
            }            
        }

        private void LoadMapSolids()
        {
            lock (MapSolidsLock)
            {
                SQLiteDataAdapter adapterMapSolids = new SQLiteDataAdapter();

                SQLiteCommandBuilder builderMapSolids = new SQLiteCommandBuilder(adapterMapSolids);

                DataSet dataMapSolids = new DataSet();
                string queryMapSolids = "SELECT * FROM Map_Solids WHERE Map_Id=$id;";
                SQLiteCommand commandMapSolids = new SQLiteCommand(queryMapSolids, DatabaseBuilder.Connection);
                commandMapSolids.Parameters.AddWithValue("$id", Id);
                adapterMapSolids.SelectCommand = commandMapSolids;
                adapterMapSolids.Fill(dataMapSolids);
                if (dataMapSolids.Tables.Count > 0 && dataMapSolids.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow r in dataMapSolids.Tables[0].Rows)
                    {
                        MapSolid ms = new MapSolid((Int64)r["Map_Solid_Id"]);
                        MapSolids.Add(ms);
                        solids.Add(ms);
                    }

                }
            }
        }

        private void LoadMapVisuals()
        {
            lock (MapVisualsLock)
            {
                SQLiteDataAdapter adapterMapSolids = new SQLiteDataAdapter();

                SQLiteCommandBuilder builderMapSolids = new SQLiteCommandBuilder(adapterMapSolids);

                DataSet dataMapSolids = new DataSet();
                string queryMapSolids = "SELECT * FROM Map_Visuals WHERE Map_Id=$id;";
                SQLiteCommand commandMapSolids = new SQLiteCommand(queryMapSolids, DatabaseBuilder.Connection);
                commandMapSolids.Parameters.AddWithValue("$id", Id);
                adapterMapSolids.SelectCommand = commandMapSolids;
                adapterMapSolids.Fill(dataMapSolids);
                if (dataMapSolids.Tables.Count > 0 && dataMapSolids.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow r in dataMapSolids.Tables[0].Rows)
                    {
                        MapVisual ms = new MapVisual((Int64)r["Map_Visual_Id"]);
                        MapVisuals.Add(ms);
                    }

                }
            }
        }
        private void LoadSpawns()
        {
            lock (SpawnLock)
            {
                SQLiteDataAdapter adapterSawns = new SQLiteDataAdapter();

                SQLiteCommandBuilder builderSpawns = new SQLiteCommandBuilder(adapterSawns);

                DataSet dataSpawns = new DataSet();
                string querySoulStones = "SELECT * FROM Monster_Spawns WHERE Map_Id=$id;";
                SQLiteCommand commandSoulStones = new SQLiteCommand(querySoulStones, DatabaseBuilder.Connection);
                commandSoulStones.Parameters.AddWithValue("$id", Id);
                adapterSawns.SelectCommand = commandSoulStones;
                adapterSawns.Fill(dataSpawns);
                if (dataSpawns.Tables.Count > 0 && dataSpawns.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow r in dataSpawns.Tables[0].Rows)
                    {
                        Spawns.Add(new MonsterSpawn((Int64)r["Monster_Spawn_Id"]));
                    }
                }
            }
        }

        private void LoadSoulStones()
        {
            lock (SoulStoneLock)
            {
                SQLiteDataAdapter adapterSoulStones = new SQLiteDataAdapter();

                SQLiteCommandBuilder builderSoulStones = new SQLiteCommandBuilder(adapterSoulStones);

                DataSet dataSoulStones = new DataSet();
                string querySoulStones = "SELECT * FROM Soul_Stones WHERE Map_Id=$id;";
                SQLiteCommand commandSoulStones = new SQLiteCommand(querySoulStones, DatabaseBuilder.Connection);
                commandSoulStones.Parameters.AddWithValue("$id", Id);
                adapterSoulStones.SelectCommand = commandSoulStones;
                adapterSoulStones.Fill(dataSoulStones);
                if (dataSoulStones.Tables.Count > 0 && dataSoulStones.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow r in dataSoulStones.Tables[0].Rows)
                    {
                        MapSoulStones.Add(new SoulStone((Int64)r["Soul_Stone_Id"]));
                    }

                }
            }
        }

        public void AddSoundAffect(SoundAffect soundAffect)
        {
            soundAffects.Enqueue(soundAffect);
        }

        public void AddFullMapSoundEffect(FullMapSoundEffect fullMapSoundEffect)
        {
            fullMapSoundEffects.Enqueue(fullMapSoundEffect);
        }

        public void AddVisualEffect(VisualEffect visualEffect)
        {
            visualEffects.Enqueue(visualEffect);
        }

        public void AddDamage(Damage damageIn)
        {
            damages.Enqueue(damageIn);
        }

        public void PlayerHit(User user)
        {
            foreach (MonsterSpawn mp in Spawns)
            {
                foreach (Monster m in mp.GetAllMonster())
                {
                    if (m.Health <=0 || user.Solid.Distance(m.Solid) > user.GetHitDistence()) 
                    {
                        continue;
                    }
                    double targetDirection = user.Location.Direction(m.MapPosition);
                    if (!user.InHitDirection(targetDirection))
                    {
                        continue;
                    }
                    long damage = user.GitHitDamage();
                    if (m.TakeDamage(damage, user, this))
                    {
                        AddDamage(new Damage(m.MapPosition, damage, 211, 0, 0));
                        return;
                    }
                }
            }
            
            // hard coded hit.
            foreach (User u in GetUsers())
            {
                if (u == user || user.Solid.Distance(u.Solid) > user.GetHitDistence())
                {
                    continue;
                }
                double targetDirection = user.Location.Direction(u.Location);
                if (!user.InHitDirection(targetDirection))
                {
                    continue;
                }
                long damage = user.GitHitDamage();
                if (u.Health <=0 || !u.TakeDamage(damage, this))
                {
                    continue;
                }
                AddDamage(new Damage(u.Location, damage, 211, 0, 0));
                AddSoundAffect(u.GetTakeHitSound(false));
                if (u.Health <= 0)
                {
                    SocketServer.SendMessageToUser(u, damage.ToString(), $"{user.UserName} killed you!");
                    SocketServer.SendMessageToUser(user, damage.ToString(), $"You Killed {u.UserName}.");
                } 
                else
                {
                    SocketServer.SendMessageToUser(u, damage.ToString(), $"Damage from {user.UserName}");
                    SocketServer.SendMessageToUser(user, damage.ToString(), $"You hit {u.UserName}");
                }
            }
        }

        private void LoadMapSounds()
        {
            lock (MapSoundsLock)
            {
                SQLiteDataAdapter adapterMapSolids = new SQLiteDataAdapter();

                SQLiteCommandBuilder builderMapSolids = new SQLiteCommandBuilder(adapterMapSolids);

                DataSet dataMapSolids = new DataSet();
                string queryMapSolids = "SELECT * FROM Map_Sounds WHERE Map_Id=$id;";
                SQLiteCommand commandMapSolids = new SQLiteCommand(queryMapSolids, DatabaseBuilder.Connection);
                commandMapSolids.Parameters.AddWithValue("$id", Id);
                adapterMapSolids.SelectCommand = commandMapSolids;
                adapterMapSolids.Fill(dataMapSolids);
                if (dataMapSolids.Tables.Count > 0 && dataMapSolids.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow r in dataMapSolids.Tables[0].Rows)
                    {
                        MapSound ms = new MapSound((Int64)r["Map_Sound_Id"]);
                        MapSounds.Add(ms);
                    }

                }
            }
        }

        private void LoadMapLights()
        {
            lock (lightsLock)
            {
                SQLiteDataAdapter adapterMapLights = new SQLiteDataAdapter();

                SQLiteCommandBuilder builderMapLights= new SQLiteCommandBuilder(adapterMapLights);

                DataSet dataMapLights = new DataSet();
                string queryMapLights = "SELECT * FROM Map_Lights WHERE Map_Id=$id;";
                SQLiteCommand commandMapLights = new SQLiteCommand(queryMapLights, DatabaseBuilder.Connection);
                commandMapLights.Parameters.AddWithValue("$id", Id);
                adapterMapLights.SelectCommand = commandMapLights;
                adapterMapLights.Fill(dataMapLights);
                if (dataMapLights.Tables.Count > 0 && dataMapLights.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow r in dataMapLights.Tables[0].Rows)
                    {
                        MapLight ml = new MapLight((Int64)r["Map_Light_Id"]);
                        MapLights.Add(ml);
                    }

                }
            }
        }

        static public Map? Create(string mapName, string imagePath, bool outside = true)
        {
            System.Drawing.Image img;
            try
            {
                img = System.Drawing.Image.FromFile($"html/{imagePath}");
            }
            catch (Exception)
            {
                return null;
            }

            Int64 width = img.Width;
            Int64 height = img.Height;
            // insert new user
            string insertNewMap = $"INSERT INTO Maps (MapName, ImagePath, Height, Width, Outside) VALUES($name, $path, $height, $width, $outside);";
            SQLiteCommand command = new SQLiteCommand(insertNewMap, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$name", mapName);
            command.Parameters.AddWithValue("$path", imagePath);
            command.Parameters.AddWithValue("$height", height);
            command.Parameters.AddWithValue("$width", width);
            command.Parameters.AddWithValue("$outside", outside ? 1 : 0);
            SQLiteTransaction transaction = null;
            try
            {
                transaction = DatabaseBuilder.Connection.BeginTransaction();
                if (command.ExecuteNonQuery() > 0)
                {
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    return new Map(rowID);
                }
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Commit();
                return null;
            }
            return null;
        }

        /// <summary>
        /// add a user to this mape 
        /// this will update the users map location.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddUser(User user, double x=0, double y=0)
        {
            user.Map_Id = Id;
            lock(userListLock) // need to lock lists
            {
                user.X_Coord = x;
                user.Y_Coord = y;
                Point center = new Point(x, y);
                long radiusMultiplier = 2;
                double angle = 10;
                bool userHasCollision = CheckCollisionWithSolids(user.Solid, user);
                while (userHasCollision)
                {
                    double r = user.Solid.Radius * radiusMultiplier;
                    Circle tempCheckCircle = new Circle(center, r);
                    for (double tempAngle = 0; tempAngle < 360; tempAngle = tempAngle + angle)
                    {
                        Point userNewCenter = tempCheckCircle.PointOnCircumference(tempAngle);
                        user.X_Coord = userNewCenter.X;
                        user.Y_Coord = userNewCenter.Y;
                        userHasCollision = CheckCollisionWithSolids(user.Solid, user);
                        if (!userHasCollision)
                        {
                            break;
                        }
                    }
                    radiusMultiplier += 2;
                    angle /= 2;
                    if (angle < 1)
                    {
                        angle = 1;
                    }
                }
                users.Add(user);
            }
        }


        public void RemoveUser(User user)
        {
            lock(userListLock)
            {
                users.Remove(user);
            }
        }

        /// <summary>
        /// return the list of users for this map.
        /// </summary>
        /// <returns></returns>
        public List<User> GetUsers()
        {
            lock(userListLock)
            {
                return new List<User>(users);
            }      
        }

        /// <summary>
        /// returns json string for Map.
        /// </summary>
        /// <returns></returns>
        public string GetJson()
        {
            //TODO: clean this up to build one time on load.
            List<object> tempMapSolids = new List<object>();
            foreach (MapSolid mapSolid in MapSolids)
            {
                if (mapSolid.HasImage())
                {
                    object toAdd = mapSolid.GetJsonImageObject();
                    if (toAdd != null)
                    {
                        tempMapSolids.Add(toAdd);
                    } 
                } 
            }
            List<object> tempMapAnimations = new List<object>();
            foreach (MapVisual mapVisual in MapVisuals)
            {
                if (mapVisual.HasImage())
                {
                    object toAdd = mapVisual.GetJsonImageObject();
                    if (toAdd != null)
                    {
                        tempMapSolids.Add(toAdd);
                    }
                }
                if (mapVisual.HasAnimation())
                {
                    object toAdd = mapVisual.GetJsonAnimationObject();
                    if (toAdd != null)
                    {
                        tempMapAnimations.Add(toAdd);
                    }
                }
            }
            List<Object> tempMapSounds = new List<Object>();
            foreach (MapSound mapSound in MapSounds)
            {
                tempMapSounds.Add(mapSound.GetJsonSoundObject());
            }
            List<Object> tempMapLights = new List<Object>();
            foreach (MapLight mapLight in MapLights)
            {
                tempMapLights.Add(mapLight.GetJsonLightObject());
            }
            return JsonConvert.SerializeObject(new {
                mapName = Name,
                width = Width,
                height = Height,
                image = ImagePath, 
                outside = Outside,
                mapImages = tempMapSolids.ToArray(),
                mapAnimations = tempMapAnimations.ToArray(),
                mapSounds = tempMapSounds.ToArray(),
                mapLights = tempMapLights.ToArray()
            });
        }

        public string GetAllJsonSoundAffects()
        {
            List<object> sounds = new List<object>();
            while (soundAffects.Count > 0)
            {
                SoundAffect? s;
                if (soundAffects.TryDequeue(out s))
                {
                    sounds.Add(s.GetJsonSoundObject());
                }
            }
            return JsonConvert.SerializeObject(sounds.ToArray());
        }

        public string GetAllJsonFullMapSoundEffects()
        {
            List<object> sounds = new List<object>();
            while (fullMapSoundEffects.Count > 0)
            {
                FullMapSoundEffect? s;
                if (fullMapSoundEffects.TryDequeue(out s))
                {
                    sounds.Add(s.GetJsonSoundObject());
                }
            }
            return JsonConvert.SerializeObject(sounds.ToArray());
        }

        public string GetAllJsonVisualEffects()
        {
            List<object> visuals = new List<object>();
            while (visualEffects.Count > 0)
            {
                VisualEffect? v;
                if (visualEffects.TryDequeue(out v))
                {
                    visuals.Add(v.GetJsonVisualObject());
                }
            }
            return JsonConvert.SerializeObject(visuals.ToArray());
        }


        public string GetAllJsonDamages()
        {
            List<object> damagesOut = new List<object>();
            while (damages.Count > 0)
            {
                Damage? d;
                if (damages.TryDequeue(out d))
                {
                    damagesOut.Add(d.GetJsonDamageObject());
                }
            }
            return JsonConvert.SerializeObject(damagesOut.ToArray());
        }

        public string getAllJsonMonster()
        {
            List<object> monsterlist = new List<object>();
            lock(SpawnLock)
            {
                foreach (MonsterSpawn mp in Spawns)
                {
                    monsterlist.AddRange(mp.GetAllMonsterJsonObjects());
                }
            }
            return JsonConvert.SerializeObject(monsterlist.ToArray());
        }

        public void TryAndMoveCreature(ICreature user)
        {
            Point nextMoveStep = user.GetNetMoveAmount();
            // if we are not moving skip this user.
            if (nextMoveStep.X == 0 && nextMoveStep.Y == 0) { return; }
            bool canMove = true;
            Point nextMove = new Point(user.X_Coord + nextMoveStep.X, user.Y_Coord + nextMoveStep.Y);
            Circle tempUser = new Circle(nextMove, user.Solid.Radius);
            canMove = !CheckCollisionWithSolids(tempUser, user);
            // if full move failed(canMove = false) check for just x move 
            if (!canMove && nextMoveStep.X != 0)
            {
                tempUser.Center.Y = user.Y_Coord;
                canMove = !CheckCollisionWithSolids(tempUser, user);
            }
            // if full move and x move fail check for just y move
            if (!canMove && nextMoveStep.Y != 0)
            {
                tempUser.Center.X = user.X_Coord;
                tempUser.Center.Y = user.Y_Coord + nextMoveStep.Y;
                canMove = !CheckCollisionWithSolids(tempUser, user);
            }
            // see if we can slid a little
            if (!canMove && nextMoveStep.X == 0 && nextMoveStep.Y != 0)
            {
                tempUser.Center.X = user.X_Coord + (nextMoveStep.Y / 2);
                double tempy = nextMoveStep.Y / 2;
                tempUser.Center.Y = user.Y_Coord + tempy;
                canMove = !CheckCollisionWithSolids(tempUser, user);
                if (!canMove)
                {
                    tempUser.Center.X = user.X_Coord - (nextMoveStep.Y / 2);
                    tempUser.Center.Y = user.Y_Coord + tempy;
                    canMove = !CheckCollisionWithSolids(tempUser, user);
                }
            }
            if (!canMove && nextMoveStep.Y == 0 && nextMoveStep.X != 0)
            {
                tempUser.Center.Y = user.Y_Coord + (nextMoveStep.X / 2);
                double tempx = nextMoveStep.X / 2;
                tempUser.Center.X = user.X_Coord + tempx;
                canMove = !CheckCollisionWithSolids(tempUser, user);
                if (!canMove)
                {
                    tempUser.Center.Y = user.Y_Coord - (nextMoveStep.X / 2);
                    tempUser.Center.X = user.X_Coord + tempx;
                    canMove = !CheckCollisionWithSolids(tempUser, user);
                }
            }
            if (canMove)
            {
                // set the user new position
                user.X_Coord = tempUser.Center.X;
                user.Y_Coord = tempUser.Center.Y;
            }
        }

        public void TickGameUpdate()
        {
            lock (userListLock)
            {
                // get each user and check for movement.
                foreach (User user in users)
                {
                    TryAndMoveCreature(user);
                }
                // check for dead players
                foreach (var user in GetUsers())
                {
                    if (user.Health <= 0 && !user.HasCoolDown)
                    {
                        user.Died();
                        GameServer.ChangeUserMap(user, user.Spawn_Map_Id, user.Spawn_X, user.Spawn_Y);
                    }
                }
                // update monster
                //TODO Break out player and monster updates.
                lock (SpawnLock)
                {
                    foreach (MonsterSpawn spawner in Spawns)
                    {
                        spawner.CheckSpawnMonsters(this);
                        List<Monster> monsters = spawner.GetAllMonster();
                        foreach (Monster monst in monsters)
                        {
                            monst.calculateNextMove(this);
                            TryAndMoveCreature(monst);
                        }
                    }
                }
            }
            
        }

        /// <summary>
        /// return true if we have a collision
        /// </summary>
        /// <param name="circle"></param>
        /// <returns></returns>
        public bool CheckCollisionWithSolids(Circle circle, ICreature? selfCreature = null)
        {
            lock (solidsLock)
            {
                foreach (ISolid solid in solids)
                {
                    if (solid.Distance(circle) > 0) {
                        continue; // if we are not near the solid skip checking all the lines.
                    }
                    bool centerInSolid = solid.PointInside(circle.Center);
                    if ((solid.IsSolidInside && centerInSolid) || (!solid.IsSolidInside && !centerInSolid))
                    {
                        return true;
                    }
                    foreach (Line line in solid.Lines())
                    {
                        if (ModCollision.DoesLineInterceptCircle(line, circle))
                        {
                            // we hit a solid and can not move.
                            return true;
                        }
                    }
                }
            }
            lock (SpawnLock)
            {
                foreach (MonsterSpawn spawn in Spawns)
                {
                    if (spawn.CheckCollisionWithMonster(circle, selfCreature))
                    {
                        return true;
                    }
                }
            }
            lock (userListLock)
            {
                foreach (User user in users)
                {
                    if (user == selfCreature)
                    {
                        continue;
                    }
                    if (user.Solid.DoesInterceptCircle(circle))
                    {
                        return true;
                    }
                }
            }
            return false;
        } 

        /// <summary>
        /// returns a portal if the player is in distance to use it.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public Portal? CheckUsePortal(User player)
        {
            List<Portal> tempPortals;
            lock (portalsLock)
            {
                tempPortals = new List<Portal>(portals);
            }
            Point userLocation = player.Location;
            foreach (Portal p in tempPortals)
            {
                double dist = Point.Distance(p.Location, userLocation);
                if (dist < 60) {
                    double portalDirection = player.Location.Direction(p.Location);
                    if (player.InHitDirection(portalDirection, 180))
                    {
                        return p;
                    }
                }
            }
            return null;
        }

        public SoulStone? CheckUseSoulStone(User player)
        {
            List<SoulStone> tempStones;
            lock (SoulStoneLock)
            {
                tempStones = new List<SoulStone>(MapSoulStones);
            }
            Point userLocation = player.Location;
            foreach (SoulStone ss in tempStones)
            {
                double dist = Point.Distance(ss.mapPosition, userLocation);
                if (dist <= ss.Radius)
                {
                    Point p = new Point(player.Location.X - 64, player.Location.Y - 54);
                    AddSoundAffect(new SoundAffect("sounds/spells/blessing.ogg", false, player.Location, 100, 300));
                    AddVisualEffect(new VisualEffect("img/spells/bind.png", p, new Point(0,0), 32, 128, 128, 5));
                    return ss;
                }
            }
            return null;
        }
    }


}
