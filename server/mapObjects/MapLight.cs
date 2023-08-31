using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{
    class MapLight
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
        public Int64 MapLightId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Map_Light_Id"];
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
        /// The main color hex code.
        /// this is the color at the center of the light.
        /// </summary>
        public String MainColor
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return "";
                    }
                    return (string)row["Main_Color"];
                }
            }
        }

        /// <summary>
        /// The mid color hex code.
        /// this is the color in the middle of the fade for the light.
        /// </summary>
        public String MidColor
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return "";
                    }
                    return (string)row["Mid_Color"];
                }
            }
        }

        /// <summary>
        /// amount of light 0-1. 0.5 is a good amount.
        /// </summary>
        public double Amount
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (double)row["Amount"];
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

        public MapLight(long mapLightId)
        {
            LoadFromId(mapLightId);
        }

        private void LoadFromId(long mapLightId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findShape = $"SELECT * FROM Map_Lights WHERE Map_Light_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findShape, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", mapLightId);
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
        static public MapLight? Create(Map map, Point mapPosistion, long radius, string mainColor = "#616100", string midColor = "#110", double amount = -1, string description = "")
        {
            string insertNewSolid = $"INSERT INTO Map_Lights (Description, Radius, Main_Color, Mid_Color, Amount, Map_Id, Map_X, Map_Y)" +
                $" VALUES($Description, $Radius, $mainColor, $MidColor, $Amount, $MapId, $MapX, $MapY);";
            SQLiteCommand command = new SQLiteCommand(insertNewSolid, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$Description", description);
            command.Parameters.AddWithValue("$Radius", radius);
            command.Parameters.AddWithValue("$MainColor", mainColor);
            command.Parameters.AddWithValue("$MidColor", midColor);
            command.Parameters.AddWithValue("$Amount", amount);
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
                    return new MapLight(rowID);
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

        public object? GetJsonLightObject(Point? position = null)
        {
            if (position is null) position = mapPosition;
            return new {x = mapPosition.X, y = mapPosition.Y, radius = Radius, mainColor = MainColor, midColor=MidColor, amount = Amount};
        }
    }
}
