using server.mapObjects;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    class Portal
    {
        private object dbDataLock = new object();

        private SQLiteDataAdapter adapter;

        private SQLiteCommandBuilder builder;

        private DataSet data;

        private DataRow row;

        /// <summary>
        /// The portal id in the database.
        /// </summary>
        public Int64 PortalId
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Int64)row["Portal_Id"];
                }
            }
        }

        /// <summary>
        /// The map id where the portal is located.
        /// </summary>
        public Int64 MapId
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Int64)row["Map_Id"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Map_Id"] = value;
                }
            }
        }

        /// <summary>
        /// the x cood on the current map. this is from the top left of the map.
        /// </summary>
        public Double X_Coord
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Double)row["X_Coordinate"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["X_Coordinate"] = value;
                }
            }
        }

        /// <summary>
        /// the y cood on the current map. this is from the top left of the map.
        /// </summary>
        public Double Y_Coord
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Double)row["Y_Coordinate"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Y_Coordinate"] = value;
                }
            }
        }

        /// <summary>
        /// The map id where the portal is sending the player.
        /// </summary>
        public Int64 TargetMapId
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Int64)row["Target_Map_Id"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Target_Map_Id"] = value;
                }
            }
        }

        /// <summary>
        /// the y cood on the target map. this is from the top left of the map.
        /// </summary>
        public Double Target_X_Coord
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Double)row["Target_X"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Target_X"] = value;
                }
            }
        }

        /// <summary>
        /// the y cood on the target map. this is from the top left of the map.
        /// </summary>
        public Double Target_Y_Coord
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Double)row["Target_Y"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Target_Y"] = value;
                }
            }
        }

        /// <summary>
        /// The portal Name
        /// </summary>
        public string Name
        {
            get
            {
                lock (dbDataLock)
                {
                    return (string)row["PortalName"];
                }
            }
        }

        /// <summary>
        /// reutnr the portals location as a point.
        /// </summary>
        public Point Location
        {
            get
            {
                return new Point(X_Coord, Y_Coord);
            }
        }


        public Portal(Int64 portalId)
        {
            LoadPortalFromDatabase(portalId);
        }

        private void LoadPortalFromDatabase(Int64 portalId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findUser = $"SELECT * FROM Portals WHERE Portal_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findUser, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", portalId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                if (data.Tables.Count == 0 || data.Tables[0].Rows.Count == 0)
                {
                    throw new Exception($"No Portal with ID {portalId}.");
                }
                row = data.Tables[0].Rows[0];
            }
        }

        static public void Create(string portalName, Int64 mapId, double x, double y, Int64 targetMapId, Int64 tartgetX, Int64 targetY)
        {
            // insert new user
            string insertNewUser = $"INSERT INTO Portals (Map_Id, X_Coordinate, Y_Coordinate, Target_Map_Id, Target_X, Target_Y, PortalName) VALUES($Map_Id, $X_Coordinate, $Y_Coordinate, $Target_Map_Id, $Target_X, $Target_Y, $PortalName);";
            SQLiteCommand command = new SQLiteCommand(insertNewUser, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$Map_Id", mapId);
            command.Parameters.AddWithValue("$X_Coordinate", x);
            command.Parameters.AddWithValue("$Y_Coordinate", y);
            command.Parameters.AddWithValue("$Target_Map_Id", targetMapId);
            command.Parameters.AddWithValue("$Target_X", tartgetX);
            command.Parameters.AddWithValue("$Target_Y", targetY);
            command.Parameters.AddWithValue("$PortalName", portalName);       
            if (command.ExecuteNonQuery() != 1)
            {
                throw new Exception("Could Not create portal.");
            }
        }
    }
}
