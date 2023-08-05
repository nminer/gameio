using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{
    internal class MapVisual : IImage, IAnimation
    {
        private object dbDataLock = new object();

        private SQLiteDataAdapter? adapter;

        private SQLiteCommandBuilder? builder;

        private DataSet? data;

        private DataRow? row;

        public Point mapPosition = new Point(0, 0);

        /// <summary>
        /// the draw order point relative to top left
        /// </summary>
        private Point drawPosition = new Point(0, 0);

        private GameImage? image = null;

        private GameAnimation? animation = null;

        /// <summary>
        /// The Map Visual Id in the database.
        /// </summary>
        public Int64 MapVisualId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Map_Visual_Id"];
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
        /// The image id for this shape in the database.
        /// </summary>
        public Int64 ImageId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Image_Id"];
                }
            }
        }

        /// <summary>
        /// The animation id for this shape in the database.
        /// </summary>
        public Int64 AnimationId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Animation_Id"];
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

        public MapVisual(long visualId)
        {
            LoadFromId(visualId);
        }

        private void LoadFromId(long visualId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findShape = $"SELECT * FROM Map_Visuals WHERE Map_Visual_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findShape, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", visualId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                row = data.Tables[0].Rows[0];
                drawPosition = new Point((Int64)row["Draw_Order_X"], (Int64)row["Draw_Order_Y"]);
                long imageId = (Int64)row["Image_Id"];
                if (imageId > 0)
                {
                    image = new GameImage(imageId);
                }
                long animationId = (Int64)row["Animation_Id"];
                if (animationId > 0)
                {
                    animation = new GameAnimation(animationId);
                }
                mapPosition = new Point((Int64)row["Map_X"], (Int64)row["Map_Y"]);
            }
        }

        /// <summary>
        /// set the solids position on the map. 
        /// this is default 0 0 if left null.
        /// </summary>
        /// <param name="shapePosition"></param>
        static public MapVisual? Create(Map map, Point mapPosistion, long imageId = 0, long animationId = 0, Point? drawOrder = null, string name = "")
        {
            // set the default to 0, 0
            if (drawOrder == null)
            {
                drawOrder = new Point(0, 0);
            }
            string insertNewSolid = $"INSERT INTO Map_Visuals (Name, Image_Id, Animation_Id, Map_Id, Map_X, Map_Y, Draw_Order_Y, Draw_Order_X)" +
                $" VALUES($Name, $ImageId, $AnimationId, $MapId, $MapX, $MapY, $DrawOrderY, $DrawOrderX);";
            SQLiteCommand command = new SQLiteCommand(insertNewSolid, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$Name", name);
            command.Parameters.AddWithValue("$ImageId", imageId);
            command.Parameters.AddWithValue("$AnimationId", animationId);
            command.Parameters.AddWithValue("$MapId", map.Id);
            command.Parameters.AddWithValue("$MapX", mapPosistion.X);
            command.Parameters.AddWithValue("$MapY", mapPosistion.Y);
            command.Parameters.AddWithValue("$DrawOrderY", drawOrder.X);
            command.Parameters.AddWithValue("$DrawOrderX", drawOrder.Y);
            SQLiteTransaction transaction = null;
            try
            {
                transaction = DatabaseBuilder.Connection.BeginTransaction();
                if (command.ExecuteNonQuery() > 0)
                {
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    return new MapVisual(rowID);
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

        public bool HasImage()
        {
            return image != null;
        }

        public object? GetJsonImageObject(Point? position = null)
        {
            if (image == null) return null;
            if (position is null) position = new Point(0, 0);
            return image.GetJsonImageObject(position, (drawPosition + position).Y);
        }

        public bool HasAnimation()
        {
            return animation != null;
        }

        public object? GetJsonAnimationObject(Point? position = null)
        {
            if (animation is null) return null;
            if (position is null) position = mapPosition;
            return animation.GetJsonAnimationObject(position, (drawPosition + position).Y);
        }
    }
}
