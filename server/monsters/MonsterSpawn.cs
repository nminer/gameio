using System.Data.SQLite;
using System.Data;
using server.mapObjects;
using System.Collections.Concurrent;

namespace server.monsters
{
    public class MonsterSpawn
    {
        private object dbDataLock = new object();

        private SQLiteDataAdapter? adapter;

        private SQLiteCommandBuilder? builder;

        private DataSet? data;

        private DataRow? row;

        public Point MapPosition = new Point(0, 0);

        private List<MonsterSpawnLink> links = new List<MonsterSpawnLink>();
        private Object linksLock = new Object();

        private object monsterLock = new object();
        private List<Monster> monsters = new List<Monster>();

        private ConcurrentQueue<DateTime> spawnTimes = new ConcurrentQueue<DateTime>();

        private bool weightSet = false;
        private long monsterweight = 0;
        private long getTotalWeight()
        {
            lock (linksLock)
            {
                if (!weightSet) { 
                    monsterweight = 0;
                    foreach (MonsterSpawnLink link in links)
                    {
                        monsterweight += link.MonsterSpawnWeight;
                    }
                    weightSet = true;
                }
                return monsterweight;
            }
        }

        public bool CheckCollisionWithMonster(Circle circle, ICreature? selfCreature = null)
        {
            lock (monsterLock)
            {
                foreach (Monster m in monsters)
                {
                    if (m == selfCreature)
                    {
                        continue;
                    }
                    if (m.Solid.DoesInterceptCircle(circle))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// The monster spawn Id in the database.
        /// </summary>
        public Int64 MonsterSpawnId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Monster_Spawn_Id"];
                }
            }
        }

        /// <summary>
        /// the map id for the spawn
        /// </summary>
        public Int64 MonsterId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Monster_Id"];
                }
            }
        }

        /// <summary>
        /// distance around spawn location that a monster can spawn
        /// </summary>
        public Int64 SpawnDistance
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Spawn_Distance"];
                }
            }
        }

        /// <summary>
        /// the max number of monsters.
        /// </summary>
        public Int64 MonsterCount
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Monster_Count"];
                }
            }
        }

        /// <summary>
        /// time in milliseconds to respawn a monster.
        /// </summary>
        public Int64 SpawnTimer
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Spawn_Timer"];
                }
            }
        }

        /// <summary>
        /// the distance a monster can wonder from spawn when idle.
        /// </summary>
        public Int64 Wander_Distance
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Wander_Distance"];
                }
            }
        }

        public MonsterSpawn(long monsterSpawnId)
        {
            LoadFromId(monsterSpawnId);
        }

        private void LoadFromId(long monsterSpawnId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findShape = $"SELECT * FROM Monster_Spawns WHERE Monster_Spawn_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findShape, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", monsterSpawnId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                row = data.Tables[0].Rows[0];
                MapPosition = new Point((Int64)row["Map_X"], (Int64)row["Map_Y"]);
                LoadLinks();
            }
        }

        public void AddMonster(Monster monster, long weight = 1)
        {    
            MonsterSpawnLink? link = MonsterSpawnLink.Create(MonsterSpawnId, monster.DatabaseId, weight);
            if (link != null)
            {
                lock (linksLock)
                {
                    links.Add(link);
                    weightSet = false;
                }
            }
        }

        private void LoadLinks()
        {
            lock (linksLock)
            {
                links.Clear();
                weightSet = false;
                SQLiteDataAdapter adapterLinks = new SQLiteDataAdapter();

                SQLiteCommandBuilder builderLinks = new SQLiteCommandBuilder(adapterLinks);

                DataSet dataLinks = new DataSet();
                string queryAnimations = $"SELECT * FROM Monster_Spawn_Links WHERE Monster_Spawn_Id=$id;";
                SQLiteCommand commandMapSolids = new SQLiteCommand(queryAnimations, DatabaseBuilder.Connection);
                commandMapSolids.Parameters.AddWithValue("$id", MonsterSpawnId);
                adapterLinks.SelectCommand = commandMapSolids;
                adapterLinks.Fill(dataLinks);
                if (dataLinks.Tables.Count > 0 && dataLinks.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow r in dataLinks.Tables[0].Rows)
                    {
                        MonsterSpawnLink msl = new MonsterSpawnLink((Int64)r["Monster_Spawn_Link_Id"]);
                        links.Add(msl);
                    }

                }
            }
        }

        /// <summary>
        /// set the solids position on the map. 
        /// this is default 0 0 if left null.
        /// </summary>
        /// <param name="shapePosition"></param>
        static public MonsterSpawn? Create(long Map_Id, long Map_X, long Map_Y, long Spawn_Distance, long Monster_Count, long Spawn_Timer, long Wander_Distance)
        {
            string insertNewSolid = $"INSERT INTO Monster_Spawns (Map_Id, Map_X, Map_Y, Spawn_Distance, Monster_Count, Spawn_Timer, Wander_Distance)" +
                $" VALUES($Map_Id, $Map_X, $Map_Y, $Spawn_Distance, $Monster_Count, $Spawn_Timer, $Wander_Distance);";
            SQLiteCommand command = new SQLiteCommand(insertNewSolid, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$Map_Id", Map_Id);
            command.Parameters.AddWithValue("$Map_X", Map_X);
            command.Parameters.AddWithValue("$Map_Y", Map_Y);
            command.Parameters.AddWithValue("$Spawn_Distance", Spawn_Distance);
            command.Parameters.AddWithValue("$Monster_Count", Monster_Count);
            command.Parameters.AddWithValue("$Spawn_Timer", Spawn_Timer);
            command.Parameters.AddWithValue("$Wander_Distance", Wander_Distance);
            SQLiteTransaction transaction = null;
            try
            {
                transaction = DatabaseBuilder.Connection.BeginTransaction();
                if (command.ExecuteNonQuery() > 0)
                {
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    return new MonsterSpawn(rowID);
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

        private Monster? SpawnMonster()
        {
            long weight = getTotalWeight();
            if (weight > 0)
            {
                long pick = Mods.IntBetween(1, (int)weight);
                lock (linksLock)
                {
                    foreach (MonsterSpawnLink link in links)
                    {
                        pick -= link.MonsterSpawnWeight;
                        if (pick <= 0)
                        {
                            //TODO add in random position when we have a spawn dist.
                            Monster monst = new Monster(link.MonsterId, MapPosition, Wander_Distance);
                            monst.Spawn = this;
                            return monst;
                        }
                    }
                }
            }
            return null;
        }

        public void RemoveDeadMonster(Monster monsterToRemove)
        {
            lock (monsterLock)
            {
                monsters.Remove(monsterToRemove);
                spawnTimes.Enqueue(DateTime.Now);
                //TODO SPawn loot.
            }
        }


        public List<Monster> GetAllMonster()
        {
            List<Monster> list = new List<Monster>();
            lock(monsterLock)
            {
                list = monsters.ToList();
            }
            return list;
        }

        public List<Monster> CheckSpawnMonsters(Map map)
        {
            List<Monster> returnMonsters = new List<Monster>();
            long needToSpawn = MonsterCount - spawnTimes.Count - monsters.Count;
            if (needToSpawn < 0)
            {
                needToSpawn = 0;
            }
            DateTime temp;
            DateTime now = DateTime.Now;
            while (spawnTimes.TryPeek(out temp))
            {
                TimeSpan timetospawn = now - temp;
                if (timetospawn.TotalMilliseconds >= SpawnTimer && spawnTimes.TryDequeue(out temp))
                {
                    needToSpawn++;
                } else
                {
                    break;
                }
            }
            for (int i=0; i < needToSpawn; i++)
            {
                Monster? monst = SpawnMonster();
                if (monst != null)
                {
                    if (!map.CheckCollisionWithSolids(monst.Solid, monst)) { 
                        map.AddSoundAffect(new SoundAffect("sounds/monsters/spawn.mp3", false, monst.MapPosition, 200, 400));
                        Point spawnEffectSmokePoint = new Point(monst.MapPosition.X - 64, monst.MapPosition.Y - 68);
                        Point spawnEffectRingPoint = new Point(monst.MapPosition.X - 64, monst.MapPosition.Y - 62);
                        map.AddVisualEffect(new VisualEffect("img/spells/flat_effect_blueish_smoke.png", spawnEffectSmokePoint, new Point(0, 0), 24, 128, 128, 5, monst.MapPosition.Y));
                        map.AddVisualEffect(new VisualEffect("img/spells/ring_blueish_smoke.png", spawnEffectRingPoint, new Point(0, 0), 40, 128, 128, 2, monst.MapPosition.Y - 2));
                        monst.SetCoolDown(100);
                        returnMonsters.Add(monst);
                        lock (monsterLock)
                        {
                            monsters.Add(monst);
                        }
                    }
                }
            }
            return returnMonsters;
        }

        public List<object> GetAllMonsterJsonObjects()
        {
            List<object> list = new List<object>();
            lock (monsterLock)
            {
                foreach (Monster mon in monsters)
                {
                    list.Add(mon.GetJsonMonsterOject());
                }
            }
            return list;
        }
    }
}
