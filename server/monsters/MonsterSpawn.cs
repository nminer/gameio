using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using server.mapObjects;

namespace server.monsters
{
    internal class MonsterSpawn
    {
        private object dbDataLock = new object();

        private SQLiteDataAdapter? adapter;

        private SQLiteCommandBuilder? builder;

        private DataSet? data;

        private DataRow? row;

        public Point MapPosition = new Point(0, 0);


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
        public Int64 Map_Id
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Map_Id"];
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
        public Int64 Monster_Count
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
        public Int64 Spawn_Timer
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
            }
        }

        public void AddMonster(Monster monster, long weight = 1)
        {

        }

        /// <summary>
        /// set the solids position on the map. 
        /// this is default 0 0 if left null.
        /// </summary>
        /// <param name="shapePosition"></param>
        static public MonsterSpawn? Create(long Map_Id, long Map_X, long Map_Y, long Spawn_Distance, long Monster_Count, long Spawn_Timer, long Wander_Distance)
        {
            string insertNewSolid = $"INSERT INTO Monster_Animations ( Map_Id, Map_X, Map_Y, Spawn_Distance, Monster_Count, Spawn_Timer, Wander_Distance)" +
                $" VALUES($Map_Id, $Map_X, $Map_Y, $Spawn_Distance, $Monster_Count, $Spawn_Timer, $Wander_Distance,);";
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
    }
}
