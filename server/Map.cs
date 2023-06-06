using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using server.mapObjects;

namespace server
{
    internal class Map
    {
        private object databaseLock = new object();
        private object listLock = new object();

        private SQLiteDataAdapter adapter;

        private SQLiteCommandBuilder builder;

        private DataSet data;

        private DataRow row;

        private List<User> users = new List<User>();

        /// <summary>
        /// The map id
        /// </summary>
        public Int64 Id
        {
            get
            {
                lock (databaseLock)
                {
                    return (Int64)row["Map_Id"];
                }
            }
        }

        /// <summary>
        /// The map Name
        /// </summary>
        public string Name
        {
            get
            {
                lock (databaseLock)
                {
                    return (string)row["MapName"];
                }
            }
        }

        /// <summary>
        /// The map height
        /// </summary>
        public Int64 Height
        {
            get
            {
                lock (databaseLock)
                {
                    return (Int64)row["Height"];
                }
            }
        }

        /// <summary>
        /// The map width
        /// </summary>
        public Int64 Width
        {
            get
            {
                lock (databaseLock)
                {
                    return (Int64)row["Width"];
                }
            }
        }

        /// <summary>
        /// The mape image path
        /// </summary>
        public string ImagePath
        {
            get
            {
                lock (databaseLock)
                {
                    return (string)row["ImagePath"];
                }
            }
        }

        /// <summary>
        /// keep the solids thread friendly.
        /// </summary>
        private object solidsLock = new object();
        /// <summary>
        /// a list of solid objects on the map.
        /// </summary>
        private List<ISolid> solids = new List<ISolid>();

        /// <summary>
        /// Load a user from its user id in the database.
        /// </summary>
        /// <param name="userId"></param>
        public Map(Int64 mapId)
        {
            LoadMap(mapId);
        }


        private void LoadMap(Int64 mapId)
        {
            lock (databaseLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findUser = $"SELECT * FROM Maps WHERE Map_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findUser, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", mapId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                row = data.Tables[0].Rows[0];
                // add the solid outline.
                
            }
            lock(solidsLock)
            {
                solids.Add(new Solid(new Point(0, 0), Height, Width));
            }
        }

        static public Map? Create(string mapName, string imagePath)
        {
            System.Drawing.Image img;
            try
            {
                img = System.Drawing.Image.FromFile($"html/{imagePath}");
            }
            catch (Exception)
            {
                return null;
            }

            Int64 height = img.Width;
            Int64 width = img.Height;
            // insert new user
            string insertNewMap = $"INSERT INTO Maps (MapName, ImagePath, Height,Width) VALUES($name, $path, $height, $width);";
            SQLiteCommand command = new SQLiteCommand(insertNewMap, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$name", mapName);
            command.Parameters.AddWithValue("$path", imagePath);
            command.Parameters.AddWithValue("$height", height);
            command.Parameters.AddWithValue("$width", width);
            try
            {
                if (command.ExecuteNonQuery() > 0)
                {
                    return null;//Map();
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        /// <summary>
        /// add a user to this mape 
        /// this will update the users map location.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddUser(User user, double x=0, double y=0)
        {
            user.Map_Id = Id;
            user.X_Coord = x;
            user.Y_Coord = y;
            lock(listLock)
            {
                users.Add(user);
            }
        }

        public void RemoveUser(User user)
        {
            lock(listLock)
            {
                users.Remove(user);
            }
        }

        /// <summary>
        /// return the list of users for this map.
        /// </summary>
        /// <returns></returns>
        public List<User> GetUsers()
        {
            lock(listLock)
            {
                return new List<User>(users);
            }      
        }

        /// <summary>
        /// returns json string for Map.
        /// </summary>
        /// <returns></returns>
        public string GetJson()
        {
            return JsonConvert.SerializeObject(new { mapName = Name, width = Width, height = Height, image = ImagePath });
        }

        public void TickGameUpdate()
        {
            lock (listLock)
            {
                // get each user and check for movement.
                foreach (var user in users)
                {
                    Point nextMoveStep = user.GetNetMoveAmount();
                    // if we are note moveing skip this user.
                    if (nextMoveStep.X == 0 && nextMoveStep.Y == 0) { continue; }
                    bool canMove = true;
                    Point nextMove = new Point(user.X_Coord + nextMoveStep.X, user.Y_Coord + nextMoveStep.Y);
                    Circle tempUser = new Circle(nextMove, user.Solid.Radius);
                        canMove = !CheckCollisionWithSolids(tempUser);
                    // if full move failed(canMove = false) check for just x move 
                    if (!canMove)
                    {
                        tempUser.Center.Y = user.Y_Coord;
                        canMove = !CheckCollisionWithSolids(tempUser);
                    }
                    // if full move and x move fail check for just y move
                    if (!canMove)
                    {
                        tempUser.Center.X = user.X_Coord;
                        tempUser.Center.Y = user.Y_Coord + nextMoveStep.Y;
                        canMove = !CheckCollisionWithSolids(tempUser);
                    }
                    if (canMove)
                    {
                        // set the user new position
                        user.X_Coord = tempUser.Center.X;
                        user.Y_Coord = tempUser.Center.Y;
                    }
                }

            }
        }

        /// <summary>
        /// returne true if we have a collision
        /// </summary>
        /// <param name="circle"></param>
        /// <returns></returns>
        private bool CheckCollisionWithSolids(Circle circle)
        {
            lock (solidsLock)
            {
                foreach (Solid solid in solids)
                {
                    foreach (Line line in solid.Lines())
                    {
                        if (ModCollision.DoesLineInterceptCircle(line, circle))
                        {
                            // we hit a solid and can not move.
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }


}
