using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{
    class SoulStone
    {
        private object dbDataLock = new object();

        private SQLiteDataAdapter? adapter;

        private SQLiteCommandBuilder? builder;

        private DataSet? data;

        private DataRow? row;

        public Point mapPosition = new Point(0, 0);


        /// <summary>
        /// The Map Sound Id in the database.
        /// </summary>
        public Int64 SoulStoneId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Soul_Stone_Id"];
                }
            }
        }

        public string Name
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return "";
                    }
                    return (string)row["Name"];
                }
            }
        }

        /// <summary>
        /// The sound id for this sound in the database.
        /// </summary>
        public Int64 Radius
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Radius"];
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

        public SoulStone(long soulStoneId)
        {
            LoadFromId(soulStoneId);
        }

        private void LoadFromId(long soulStoneId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findShape = $"SELECT * FROM Soul_Stones WHERE Soul_Stone_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findShape, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", soulStoneId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                row = data.Tables[0].Rows[0];
                mapPosition = new Point((Int64)row["Map_X"], (Int64)row["Map_Y"]);
            }
        }

        /// <summary>
        /// set the lights position on the map. 
        /// </summary>
        /// <param name="shapePosition"></param>
        static public SoulStone? Create(Map map, Point mapPosistion, long radius, string name = "Name")
        {
            string insertNewSoulStonr = $"INSERT INTO Soul_Stones (Map_Id, Map_X, Map_Y, Radius, Name)" +
                $" VALUES($MapId, $MapX, $MapY, $Radius, $Name);";
            SQLiteCommand command = new SQLiteCommand(insertNewSoulStonr, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$MapId", map.Id);
            command.Parameters.AddWithValue("$MapX", mapPosistion.X);
            command.Parameters.AddWithValue("$MapY", mapPosistion.Y);
            command.Parameters.AddWithValue("$Radius", radius);
            command.Parameters.AddWithValue("$Name", name);
            SQLiteTransaction transaction = null;
            try
            {
                transaction = DatabaseBuilder.Connection.BeginTransaction();
                if (command.ExecuteNonQuery() > 0)
                {
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    return new SoulStone(rowID);
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
