using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json;
using System.Drawing;

namespace server.mapObjects
{
    internal class MapSolid : ISolid, IImage, IAnimation
    {
        private object dbDataLock = new object();

        private SQLiteDataAdapter? adapter;

        private SQLiteCommandBuilder? builder;

        private DataSet? data;

        private DataRow? row;

        private Solid? solid = null;

        private Shape? shape = null;

        public Point mapPosition = new Point(0, 0);

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

        public Int64 MapSolidId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Map_Solid_Id"];
                }
            }
        }


        public Int64 SolidId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Solid_Id"];
                }
            }
        }

        public Int64 ShapeId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Shape_Id"];
                }
            }
        }

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

        public MapSolid(long mapSolidId)
        {
            LoadMapSolid(mapSolidId);
        }

        private void LoadMapSolid(long mapSolidId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findShape = $"SELECT * FROM Map_Solids WHERE Map_Solid_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findShape, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", mapSolidId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                row = data.Tables[0].Rows[0];
                if (ShapeId > 0)
                {
                    shape = new Shape(ShapeId);
                }
                if (SolidId > 0)
                {
                    solid = new Solid(SolidId);
                }
                mapPosition = new Point((Int64)row["Map_X"], (Int64)row["Map_Y"]);
            }
        }

        static public MapSolid? Create(Map map, Solid solid, Point mapLocation, string description = "")
        {
            return Create(map.Id, 0, solid.SolidId, (long)mapLocation.X, (long)mapLocation.Y, description);
        }

        static public MapSolid? Create(Map map, Shape shape, Point mapLocation, string description = "")
        {
            return Create(map.Id, shape.ShapeId, 0, (long)mapLocation.X, (long)mapLocation.Y, description);
        }

        static private MapSolid? Create(long mapId, long shapeId, long solidId, long mapLocationX, long mapLocationY, string description = "")
        {
            string insertNewMapSolid = $"INSERT INTO Map_Solids (Description, Solid_Id, Shape_Id, Map_Id, Map_X, Map_Y)" +
                      $" VALUES($descript, $solidId, $shapeId, $mapId, $mapX, $mapY);";
            SQLiteCommand command = new SQLiteCommand(insertNewMapSolid, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$descript", description);
            command.Parameters.AddWithValue("$solidId", solidId);
            command.Parameters.AddWithValue("$shapeId", shapeId);
            command.Parameters.AddWithValue("$mapId", mapId);
            command.Parameters.AddWithValue("$mapX", mapLocationX);
            command.Parameters.AddWithValue("$mapY", mapLocationY);
            SQLiteTransaction transaction = null;
            try
            {
                transaction = DatabaseBuilder.Connection.BeginTransaction();
                if (command.ExecuteNonQuery() > 0)
                {
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    return new MapSolid(rowID);
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

        public Line[] Lines(Point? position = null)
        {
            if (position is null) { position = mapPosition; }
            if (solid is not null)
            {
                return solid.Lines(position);
            }
            else if (shape is not null)
            {
                return shape.Lines(position);
            }
            else
            {
                return new Line[0];
            }
        }

        public bool HasImage()
        {
            return solid != null && solid.HasImage();
        }

        public object? GetJsonImageObject(Point? position = null)
        {
            if (solid is null) return null;
            return solid.GetJsonImageObject(mapPosition);
        }

        public bool HasAnimation()
        {
            return solid != null && solid.HasAnimation();
        }

        public object? GetJsonAnimationObject(Point? position = null)
        {
            if (solid is null) return null;
            return solid.GetJsonAnimationObject(mapPosition);
        }
    }
}
