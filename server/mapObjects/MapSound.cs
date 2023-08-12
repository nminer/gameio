using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{
    class MapSound
    {
        private object dbDataLock = new object();

        private SQLiteDataAdapter? adapter;

        private SQLiteCommandBuilder? builder;

        private DataSet? data;

        private DataRow? row;

        public Point mapPosition = new Point(0, 0);

        private GameSound? sound = null;

        /// <summary>
        /// The Map Sound Id in the database.
        /// </summary>
        public Int64 MapSoundId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Map_Sound_Id"];
                }
            }
        }

        public string Description
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return "";
                    }
                    return (string)row["Description"];
                }
            }
        }

        /// <summary>
        /// The sound id for this sound in the database.
        /// </summary>
        public Int64 SoundId
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
        /// The Map id in the database.
        /// </summary>
        public Int64 MapId
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

        public MapSound(long mapSoundId)
        {
            LoadFromId(mapSoundId);
        }

        private void LoadFromId(long mapSoundId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findShape = $"SELECT * FROM Map_Sounds WHERE Map_Sound_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findShape, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", mapSoundId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                row = data.Tables[0].Rows[0];
                long soundId = (Int64)row["Sound_Id"];
                if (soundId > 0)
                {
                    sound = new GameSound(soundId);
                }
                mapPosition = new Point((Int64)row["Map_X"], (Int64)row["Map_Y"]);
            }
        }

        /// <summary>
        /// set the solids position on the map. 
        /// this is default 0 0 if left null.
        /// </summary>
        /// <param name="shapePosition"></param>
        static public MapSound? Create(Map map, Point mapPosistion, long soundId, string description = "")
        {
            string insertNewSolid = $"INSERT INTO Map_Sounds (Description, Sound_Id, Map_Id, Map_X, Map_Y)" +
                $" VALUES($Description, $SoundId, $MapId, $MapX, $MapY);";
            SQLiteCommand command = new SQLiteCommand(insertNewSolid, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$Description", description);
            command.Parameters.AddWithValue("$SoundId", soundId);
            command.Parameters.AddWithValue("$MapId", map.Id);
            command.Parameters.AddWithValue("$MapX", mapPosistion.X);
            command.Parameters.AddWithValue("$MapY", mapPosistion.Y);
            SQLiteTransaction transaction = null;
            try
            {
                transaction = DatabaseBuilder.Connection.BeginTransaction();
                if (command.ExecuteNonQuery() > 0)
                {
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    return new MapSound(rowID);
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

        public object? GetJsonSoundObject(Point? position = null)
        {
            if (sound == null) return null;
            if (position is null) position = mapPosition;
            return sound.GetJsonSoundObject(position);
        }
    }
}
