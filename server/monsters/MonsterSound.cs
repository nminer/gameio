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
    public class MonsterSound
    {
        private object dbDataLock = new object();

        private SQLiteDataAdapter? adapter;

        private SQLiteCommandBuilder? builder;

        private DataSet? data;

        private DataRow? row;

        private GameSound? gameSound;

        /// <summary>
        /// The monster sound Id in the database.
        /// </summary>
        public Int64 MonsterSoundId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Monster_Sound_Id"];
                }
            }
        }

        /// <summary>
        /// the id of the monster type this sound is a part of.
        /// </summary>
        public Int64 MonsterTypeId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Monster_Type_Id"];
                }
            }
        }

        /// <summary>
        /// the id of the monster type this sound is a part of.
        /// </summary>
        public Int64 Sound_Id
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Sound_Id"];
                }
            }
        }

        /// <summary>
        /// the name of the sound hit, spawn in ....
        /// </summary>
        public string SoundName
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return "";
                    }
                    return (string)row["Sound_Name"];
                }
            }
        }

        public MonsterSound(long monsterSoundId)
        {
            LoadFromId(monsterSoundId);
        }

        private void LoadFromId(long monsterSoundId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findShape = $"SELECT * FROM Monster_Sounds WHERE Monster_Sound_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findShape, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", monsterSoundId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                row = data.Tables[0].Rows[0];
                gameSound = new GameSound((Int64)row["Sound_Id"]);
            }
        }

        /// <summary>
        /// set the solids position on the map. 
        /// this is default 0 0 if left null.
        /// </summary>
        /// <param name="shapePosition"></param>
        static public MonsterSound? Create(long MonsterTypeId, long SoundId, string soundName)
        {
            string insertNewSolid = $"INSERT INTO Monster_Sounds (Monster_Type_Id, Sound_Id, Sound_Name)" +
                $" VALUES($Monster_Type_Id, $Sound_Id, $Sound_Name);";
            SQLiteCommand command = new SQLiteCommand(insertNewSolid, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$Monster_Type_Id", MonsterTypeId);
            command.Parameters.AddWithValue("$Sound_Id", SoundId);
            command.Parameters.AddWithValue("$Sound_Name", soundName);
            SQLiteTransaction transaction = null;
            try
            {
                transaction = DatabaseBuilder.Connection.BeginTransaction();
                if (command.ExecuteNonQuery() > 0)
                {
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    return new MonsterSound(rowID);
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
