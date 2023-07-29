using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.Drawing;
using Newtonsoft.Json;

namespace server.mapObjects
{
    class Shape
    {

        /// <summary>
        /// holds the list of points that make up this shape.
        /// </summary>
        private List<Point> points = new List<Point>();

        private object dbDataLock = new object();

        private SQLiteDataAdapter? adapter;

        private SQLiteCommandBuilder? builder;

        private DataSet? data;

        private DataRow? row;

        /// <summary>
        /// gets set to true if lines have been built
        /// this is to keep us from building lines over and over again.
        /// </summary>
        private bool linesBuilt = false;

        /// <summary>
        /// the point that lines was last built from.
        /// </summary>
        private Point buildPoint = new Point(0,0);

        /// <summary>
        /// hold the list of lines for this shape .
        /// </summary>
        private List<Line> lines = new List<Line>();

        /// <summary>
        /// The users id in the database.
        /// </summary>
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

        /// <summary>
        // when set to true first and last point will be connected with a line.
        /// </summary>       
        public bool IsClosedShape
        {
            get
            {
                lock (dbDataLock)
                {
                    return isClosedShape;
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    isClosedShape = value;
                    linesBuilt = false;
                }
            }
        }
        private bool isClosedShape = true;


        public Shape(long  shapeId)
        {
            LoadFromId(shapeId);
        }

        public Shape()
        {

        }

        /// <summary>
        /// add a point to the solid.
        /// each pair of points in order create a line.
        /// </summary>
        /// <param name="point"></param>
        public Shape AddPoint(Point point)
        {
            lock (dbDataLock)
            {
                points.Add(point);
                linesBuilt = false;
            }
            return this;
        }

        /// <summary>
        /// add a point to this sold.
        /// each pair of points in order create a line.
        /// </summary>
        /// <param name="x">x coord</param>
        /// <param name="y">y coord</param>
        /// <returns></returns>
        public Shape AddPoint(double x, double y)
        {
            AddPoint(new Point(x, y));
            return this;
        }

        /// <summary>
        /// returns a list of lines that make up the solid.
        /// </summary>
        /// <returns></returns>
        public Line[] Lines(Point? relativePoint = null)
        {
            lock (dbDataLock)
            {
                if (relativePoint is null)
                {
                    relativePoint = new Point(0,0);
                }
                // build the lines once for the solid.
                if (!linesBuilt || relativePoint != buildPoint )
                {
                    linesBuilt = true;
                    lines.Clear();
                    if (points.Count <= 1)
                    {
                        return lines.ToArray();
                    }
                    // connect each pair of points to build lines
                    for (int i = 0; i < points.Count && i + 1 < points.Count; i++)
                    {
                        Point p1 = points[i] + relativePoint; // new Point(points[i].X + relativePoint.X, points[i].Y + relativePoint.Y);
                        Point p2 = points[i + 1] + relativePoint;  // new Point(points[i + 1].X + relativePoint.X, points[i + 1].Y + relativePoint.Y);
                        lines.Add(new Line(p1, p2));
                    }
                    // connect the last point to the first point if it is a closed shape
                    if (isClosedShape)
                    {
                        Point p1 = new Point(points.Last().X + relativePoint.X, points.Last().Y + relativePoint.Y);
                        Point p2 = new Point(points.First().X + relativePoint.X, points.First().Y + relativePoint.Y);
                        lines.Add(new Line(p1, p2));
                    }
                }
                return lines.ToArray();
            }
        }

        public string ToJson()
        {
            lock (dbDataLock)
            {
                string jsonString = System.Text.Json.JsonSerializer.Serialize(points);
                return jsonString;
            }
        }

        private void LoadFromId(long shapeId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findUser = $"SELECT * FROM Shapes WHERE Shape_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findUser, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", shapeId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                row = data.Tables[0].Rows[0];
                isClosedShape = (Int64)row["Is_Closed_Shape"] == 1;
                points = JsonConvert.DeserializeObject<List<Point>>((string)row["Json_Points"]);
                linesBuilt = false;
            }        
        }

        public void Save(string description = "")
        {
            lock (dbDataLock)
            {
                if (row == null)
                {
                    // insert new Shape
                    string insertNewShape = $"INSERT INTO Shapes (Description, Json_Points, Is_Closed_Shape) VALUES($description, $json, $isShape);";
                    SQLiteCommand command = new SQLiteCommand(insertNewShape, DatabaseBuilder.Connection);
                    SQLiteTransaction transaction = null;
                    transaction = DatabaseBuilder.Connection.BeginTransaction();
                    command.Parameters.AddWithValue("$description", description);
                    command.Parameters.AddWithValue("$json", ToJson());
                    command.Parameters.AddWithValue("$isShape", isClosedShape ? 1 : 0);
                    if (command.ExecuteNonQuery() != 1)
                    {
                        throw new Exception("Could Not create new Shape.");
                    }
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    LoadFromId(rowID);
                }
                else
                {
                    row["Json_Points"] = ToJson();
                    row["Is_Closed_Shape"] = isClosedShape ? 1 : 0;
                    builder.ConflictOption = ConflictOption.OverwriteChanges;
                    builder.GetUpdateCommand();
                    adapter.Update(data);
                }
            }
        }

    }
}
