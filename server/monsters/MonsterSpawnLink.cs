using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.monsters
{
    internal class MonsterSpawnLink
    {
        private object dbDataLock = new object();

        private SQLiteDataAdapter? adapter;

        private SQLiteCommandBuilder? builder;

        private DataSet? data;

        private DataRow? row;



        /// <summary>
        /// The monster spawn link Id in the database.
        /// </summary>
        public Int64 MonsterSpawnLinkId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Monster_Spawn_Link_Id"];
                }
            }
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
        /// the weight for this monster to spawn with any of the other creatures in the spawn.
        /// </summary>
        public Int64 MonsterSpawnWeight
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Monster_Spawn_Weight"];
                }
            }
        }

        /// <summary>
        /// the monster id of the monster being linked to the spawn.
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


        public MonsterSpawnLink(long monsterSpawnLinkId)
        {
            LoadFromId(monsterSpawnLinkId);
        }

        private void LoadFromId(long monsterSpawnLinkId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findShape = $"SELECT * FROM Monster_Spawn_Links WHERE Monster_Spawn_Link_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findShape, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", monsterSpawnLinkId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                row = data.Tables[0].Rows[0];
            }
        }

        /// <summary>
        /// set the solids position on the map. 
        /// this is default 0 0 if left null.
        /// </summary>
        /// <param name="shapePosition"></param>
        static public MonsterSpawnLink? Create(long Monster_Spawn_Id, long Monster_Id, long Monster_Spawn_Weight)
        {
            string insertNewSolid = $"INSERT INTO Monster_Spawn_Links (Monster_Spawn_Id, Monster_Id, Monster_Spawn_Weight)" +
                $" VALUES($Monster_Spawn_Id, $Monster_Id, $Monster_Spawn_Weight);";
            SQLiteCommand command = new SQLiteCommand(insertNewSolid, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$Monster_Spawn_Id", Monster_Spawn_Id);
            command.Parameters.AddWithValue("$Monster_Id", Monster_Id);
            command.Parameters.AddWithValue("$Monster_Spawn_Weight", Monster_Spawn_Weight);
            SQLiteTransaction transaction = null;
            try
            {
                transaction = DatabaseBuilder.Connection.BeginTransaction();
                if (command.ExecuteNonQuery() > 0)
                {
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    return new MonsterSpawnLink(rowID);
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
