using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace server.mapObjects
{

    class Solid : ISolid
    {
        private object dbDataLock = new object();

        private SQLiteDataAdapter? adapter;

        private SQLiteCommandBuilder? builder;

        private DataSet? data;

        private DataRow? row;

        /// <summary>
        /// used to keep points and lines list safe.
        /// </summary>
        private object threadLock = new object();

        /// <summary>
        /// all points in the shape are relative to position.
        /// </summary>
        private Point shapePosition = new Point(0, 0);

        /// <summary>
        /// the draw order point relative to top left
        /// </summary>
        private Point drawPosition = new Point(0,0);


        private Shape shape;

        private GameImage? image = null;

        private GameAnimation? animation = null;

        /// <summary>
        /// The shape id in the database.
        /// </summary>
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

        public long ShapeId
        {
            get { return shape.ShapeId; }
        }

        public Solid(long solidId)
        {
            LoadFromId(solidId);
        }

        /// <summary>
        /// create a simple solid. 4 points.
        /// </summary>
        /// <param name="shapePosition"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        public Solid(Point shapePosition, double height, double width)
        {
            this.shapePosition = shapePosition;
            shape = new Shape(height, width);
            //shape.AddPoint(0, 0).AddPoint(width, 0).AddPoint(width, height).AddPoint(0, height);
        }

        private void LoadFromId(long solidId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findShape = $"SELECT * FROM Solids WHERE Solid_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findShape, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", solidId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                row = data.Tables[0].Rows[0];
                shape = new Shape((Int64)row["Shape_Id"]);
                shapePosition = new Point((Int64)row["Shape_Offset_X"], (Int64)row["Shape_Offset_Y"]);
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
            }
        }

        /// <summary>
        /// set the solids position on the map. 
        /// this is default 0 0 if left null.
        /// </summary>
        /// <param name="shapePosition"></param>
        static public Solid? Create(Shape shape, Point? shapePosition = null, long imageId = 0, long animationId = 0, Point? drawOrder = null, string description = "")
        {
            // set the default to 0, 0
            if (shapePosition == null)
            {
                shapePosition = new Point(0, 0);
            }
            if (drawOrder == null)
            {
                drawOrder = new Point(0, 0);
            }
            if (shape.ShapeId == 0)
            {
                shape.Save(description + " shape.");
            }
            string insertNewSolid = $"INSERT INTO Solids (Description, Image_Id, Animation_Id, Shape_Id, Shape_Offset_X, Shape_Offset_Y, Draw_Order_Y, Draw_Order_X)" +
                $" VALUES($descript, $imgId, $animId, $shapeId, $shapeX, $shapeY, $drawX, $drawY);";
            SQLiteCommand command = new SQLiteCommand(insertNewSolid, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$descript", description);
            command.Parameters.AddWithValue("$imgId", imageId);
            command.Parameters.AddWithValue("$animId", animationId);
            command.Parameters.AddWithValue("$shapeId", shape.ShapeId);
            command.Parameters.AddWithValue("$shapeX", shapePosition.X);
            command.Parameters.AddWithValue("$shapeY", shapePosition.Y);
            command.Parameters.AddWithValue("$drawX", drawOrder.X);
            command.Parameters.AddWithValue("$drawY", drawOrder.Y);
            SQLiteTransaction transaction = null;
            try
            {
                transaction = DatabaseBuilder.Connection.BeginTransaction();
                if (command.ExecuteNonQuery() > 0)
                {
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    return new Solid(rowID);
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

        /// <summary>
        /// returns a list of lines that make up the solid.
        /// </summary>
        /// <returns></returns>
        public Line[] Lines(Point? position = null)
        {
            if (position is null)
            {
                lock (threadLock)
                {
                    return shape.Lines(shapePosition);
                }
            }     
            lock (threadLock)
            {
                return shape.Lines(shapePosition + position);
            }
        }

        public bool HasImage()
        {
            return image != null;
        }

        public object? GetJsonImageObject(Point position)
        {
            if (image == null) return null;
            return image.GetJsonImageObject(position, (drawPosition + position).Y);
        }

        public bool HasAnimation()
        {
            return animation != null;
        }

        public object? GetJsonAnimationObject(Point position)
        {
            if (animation == null) return null;
            return animation.GetJsonAnimationObject(position, (drawPosition + position).Y);
        }
    }
}
