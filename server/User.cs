using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Data.Common;
using System.Data;
using System.Reflection.PortableExecutable;
using BCrypt.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using server.mapObjects;

namespace server
{
    class User
    {
        private const double MIN_SPEED = 0.0;
        private const double MAX_SPEED = 100.0;

        private UserControls controls = new UserControls();

        private object dbDataLock = new object();

        public double Height = 80;

        public double Width = 80;

        public Circle Solid = new Circle(new Point(0, 0), 15);

        //private string[] Animations = {"walkDown", "walkUp", "walkLeft", "walkRight", "standDown", "standUp", "standLeft", "standRight", "swingDown", "swingUp", "swingLeft", "swingRight" };

        private string AnimationName = "standDown";

        /// <summary>
        /// The users id in the database.
        /// </summary>
		public Int64 UserId
        {
            get 
            { 
                lock(dbDataLock)
                {
                    return (Int64)row["User_Id"];
                }
            }
        }

        /// <summary>
        /// the user name.
        /// </summary>
		public string UserName
        {
            get
            {
                lock (dbDataLock)
                {
                    return (string)row["UserName"];
                }
            }
        }

        /// <summary>
        /// The users password. hashed?
        /// </summary>
		public string Password
        {
            get
            {
                lock (dbDataLock)
                {
                    return (string)row["PasswordHash"];
                }
            }
        }

        /// <summary>
        /// The users level.
        /// </summary>
		public Int64 Level
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Int64)row["Level"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Level"] = value;
                }
            }
        }

        /// <summary>
        /// the max helth the player can have.
        /// </summary>
		public Int64 MaxHealth
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Int64)row["Max_Health"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Max_Health"] = value;
                }
            }
        }

        /// <summary>
        /// the current helth of the player.
        /// </summary>
		public Int64 Health
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Int64)row["Health"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Health"] = value;
                }
            }
        }

        /// <summary>
        /// the players max stamana. used for running and hitting.
        /// </summary>
        public Int64 MaxStamana
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Int64)row["Max_Stamana"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Max_Stamana"] = value;
                }
            }
        }

        /// <summary>
        /// the player current stamana.
        /// </summary>
        public Int64 Stamana
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Int64)row["Stamana"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Stamana"] = value;
                }
            }
        }

        /// <summary>
        /// players strength.
        /// </summary>
        public Int64 Strength
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Int64)row["Strength"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Strength"] = value;
                }
            }
        }

        /// <summary>
        /// players speed. how fast they move and hit.
        /// </summary>
        public Int64 Speed
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Int64)row["Speed"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Speed"] = value;
                }
            }
        }

        /// <summary>
        /// what map the player is on. 
        /// </summary>
        public Int64 Map_Id
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
        /// the x cood on the current map. this is from the rop left of the map.
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
                    Solid.Center.X = value;
                }
            }
        }

        /// <summary>
        /// the y cood on the current map. this is from the rop left of the map.
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
                    Solid.Center.Y = value;
                }
            }
        }

        public Int64 Direction
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Int64)row["Direction"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    if (value >= 0 && value <= 359)
                    {
                        row["Direction"] = value;
                    }
                    else
                    {
                        row["Direction"] = 0;
                    }
                }
            }
        }

        private string BodyStyle = "01";
        public string BodyColor = "01";
        public string NoseStyle = "00";
        public string EarStyle = "00";
        public string Wrinkles = "00";
        private string HairStyle = "00";
        private string HairColor = "00";
        private string BeardStyle = "00";
        private string BeardColor = "00";
        private string EyeColor = "00";


        private SQLiteDataAdapter adapter;

        private SQLiteCommandBuilder builder;

        private DataSet data;

        private DataRow row;

        /// <summary>
        /// Load a user from its user id in the database.
        /// </summary>
        /// <param name="userId"></param>
        public User(Int64 userId)
        {
            LoaderUser(userId);
        }


        private void LoaderUser(Int64 userId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findUser = $"SELECT * FROM User WHERE User_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findUser, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", userId);
                adapter.SelectCommand = command;

                adapter.Fill(data);
                //if (!(data.Tables.Count > 0 && data.Tables[0].Rows.Count > 0))
                //{

                //}
                row = data.Tables[0].Rows[0];
            }
            // get the users avatar looks loaded.
            SQLiteDataAdapter adapterAvatar = new SQLiteDataAdapter();

            SQLiteCommandBuilder builderAvatar = new SQLiteCommandBuilder(adapterAvatar);

            DataSet dataAvatar = new DataSet();
            string queryLooks = "SELECT * FROM Avatar WHERE User_Id=$id;";
            SQLiteCommand commandAvatar = new SQLiteCommand(queryLooks, DatabaseBuilder.Connection);
            commandAvatar.Parameters.AddWithValue("$id", userId);
            adapterAvatar.SelectCommand = commandAvatar;
            adapterAvatar.Fill(dataAvatar);
            if (dataAvatar.Tables.Count > 0 && dataAvatar.Tables[0].Rows.Count > 0)
            {
                DataRow rowAvatar = dataAvatar.Tables[0].Rows[0];
                BodyStyle = ((Int64)rowAvatar["Body_Style"]).ToString("D2");
                BodyColor = ((Int64)rowAvatar["Body_Color"]).ToString("D2");
                NoseStyle = ((Int64)rowAvatar["Nose_Style"]).ToString("D2");
                EarStyle = ((Int64)rowAvatar["Ear_Style"]).ToString("D2");
                Wrinkles = ((Int64)rowAvatar["Wrinkles"]).ToString("D2");
                HairStyle = ((Int64)rowAvatar["Hair_Style"]).ToString("D2");
                HairColor = ((Int64)rowAvatar["Hair_Color"]).ToString("D2");
                BeardStyle = ((Int64)rowAvatar["Beard_Style"]).ToString("D2");
                BeardColor = ((Int64)rowAvatar["Beard_Color"]).ToString("D2");
                EyeColor = ((Int64)rowAvatar["Eye_Color"]).ToString("D2");
            }
        }

        /// <summary>
        /// try and log a User in and return the user object.
        /// returns the user if log in works.
        /// throws exception if login does not work.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        static public User LogIn(string userName, string password)
        {
            SQLiteDataReader dataReader = CheckUserName(userName);
            if (!dataReader.HasRows)
            {
                throw new Exception("Invalid login.");
            }
            dataReader.Read();
            int id = dataReader.GetInt32(0);
            User tempUser = new User(id);
            if (BCrypt.Net.BCrypt.Verify(password, tempUser.Password))
            {
                return tempUser;
            }
            else
            {
                throw new Exception("Invalid login.");
            }
        }

        /// <summary>
        /// anything that needs to be done when loging a user out can be done here.
        /// </summary>
        public void LogUserOut()
        {
            SaveUser();
        }

        static public void Create(string userName, string password)
        {
            SQLiteDataReader dataReader = CheckUserName(userName);
            if (dataReader.HasRows)
            {
                throw new Exception("User name already in use.");
            }
            // insert new user
            string insertNewUser = $"INSERT INTO User (UserName, PasswordHash) VALUES($name, $pass);";
            SQLiteCommand command = new SQLiteCommand(insertNewUser, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$name", userName);
            command.Parameters.AddWithValue("$pass", BCrypt.Net.BCrypt.HashPassword(password));
            if (command.ExecuteNonQuery() != 1)
            {
                throw new Exception("Could Not create user name and pass.");
            }
        }

        /// <summary>
        /// given the user name as a string returns the datareader with the row
        /// for the user. if any.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        static private SQLiteDataReader CheckUserName(string userName)
        {
            SQLiteCommand cmd = DatabaseBuilder.Connection.CreateCommand();
            string findUser = $"SELECT User_Id FROM User WHERE UserName='{userName}'";
            cmd.CommandText = findUser;
            SQLiteDataReader dataReader = cmd.ExecuteReader();
            return dataReader;
        }

        public void SetAvatar(Dictionary<string, string> avatar)
        {
            int bodys = avatar.ContainsKey("bodyStyle") ? Int32.Parse(avatar["bodyStyle"]) : 1;
            int bodyc = avatar.ContainsKey("bodyColor") ? Int32.Parse(avatar["bodyColor"]) : 1;
            int noses = avatar.ContainsKey("noseStyle") ? Int32.Parse(avatar["noseStyle"]) : 0;
            int ears = avatar.ContainsKey("earStyle") ? Int32.Parse(avatar["earStyle"]) : 0;
            int wrinkless = avatar.ContainsKey("wrinklesStyle") ? Int32.Parse(avatar["wrinklesStyle"]) : 0;
            int hairs = avatar.ContainsKey("hairStyle") ? Int32.Parse(avatar["hairStyle"]) : 0;
            int hairc = avatar.ContainsKey("hairColor") ? Int32.Parse(avatar["hairColor"]) : 0;
            int beards = avatar.ContainsKey("beardStyle") ? Int32.Parse(avatar["beardStyle"]) : 0;
            int beardc = avatar.ContainsKey("beardColor") ? Int32.Parse(avatar["beardColor"]) : 0;
            int eyec = avatar.ContainsKey("eyeColor") ? Int32.Parse(avatar["eyeColor"]) : 0;
            SetAvatar(bodys, bodyc, noses, ears, wrinkless, hairs, hairc, beards, eyec);
        }

        public void SetAvatar(int bodyStyle = 1, int bodyColor = 1,
                              int noseStyle = 0, int earStyle = 0,
                              int wrinkles = 0,
                              int hairStyle = 0, int hairColor = 0,
                              int beardStyle = 0, int beardColor = 0,
                              int eyeColor = 0)
        {
            SQLiteDataAdapter adapterAvatar = new SQLiteDataAdapter();

            SQLiteCommandBuilder builderAvatar = new SQLiteCommandBuilder(adapterAvatar);

            DataSet dataAvatar = new DataSet();
            string queryLooks = "SELECT * FROM Avatar WHERE User_Id=$id;";
            SQLiteCommand commandAvatar = new SQLiteCommand(queryLooks, DatabaseBuilder.Connection);
            commandAvatar.Parameters.AddWithValue("$id", UserId);
            adapterAvatar.SelectCommand = commandAvatar;
            adapterAvatar.Fill(dataAvatar);
            if (dataAvatar.Tables.Count > 0 && dataAvatar.Tables[0].Rows.Count > 0)
            {
                DataRow rowAvatar = dataAvatar.Tables[0].Rows[0];
                rowAvatar["Body_Style"] = bodyStyle;
                rowAvatar["Body_Color"] = bodyColor;
                rowAvatar["Nose_Style"] = noseStyle;
                rowAvatar["Ear_Style"] = earStyle;
                rowAvatar["Wrinkles"] = wrinkles;
                rowAvatar["Hair_Style"] = hairStyle;
                rowAvatar["Hair_Color"] = hairColor;
                rowAvatar["Beard_Style"] = beardStyle;
                rowAvatar["Beard_Color"] = beardColor;
                rowAvatar["Eye_Color"] = eyeColor;
                builderAvatar.ConflictOption = ConflictOption.OverwriteChanges;
                builderAvatar.GetUpdateCommand();
                adapterAvatar.Update(dataAvatar);
            }
            else
            {
                string insertAvatar = @$"INSERT INTO Avatar
                                        ('User_Id', 'Body_Style', 'Body_Color',
                                         'Nose_Style', 'Ear_Style', 'Wrinkles',
                                        'Hair_Style', 'Hair_Color',
                                        'Beard_style', 'Beard_Color', 'Eye_Color')
                                        Values
                                        ({UserId}, {bodyStyle}, {bodyColor},
                                        {noseStyle}, {earStyle}, {wrinkles},
                                        {hairStyle}, {hairColor}, 
                                        {beardStyle}, {beardColor}, {eyeColor});";
                SQLiteCommand command = new SQLiteCommand(insertAvatar, DatabaseBuilder.Connection);
                if (command.ExecuteNonQuery() != 1)
                {
                    throw new Exception("Could Not create user Avatar.");
                }
            }
        }

        public int SaveUser()
        {
            builder.ConflictOption = ConflictOption.OverwriteChanges;
            builder.GetUpdateCommand();
            return adapter.Update(data);
        }

        /// <summary>
        /// returns json string for user.
        /// </summary>
        /// <returns></returns>
        public string GetJson()
        {
            return JsonConvert.SerializeObject(
                new { username = UserName,
                    avatar = new {body = BodyStyle, bodyc = BodyColor,
                                  nose = NoseStyle, ears = EarStyle, wrinkles = Wrinkles,
                                  hair = HairStyle, hairc = HairColor,
                                  beard = BeardStyle, beardc = BeardColor,
                                  eyec = EyeColor},
                    x = X_Coord, y = Y_Coord, direction = Direction,
                    speed = Speed,
                    animation = AnimationName,  running = controls.Run}
                );
        }

        public void UpdateFromPlayer(JObject movement)
        {
            controls.Up = bool.Parse((string)movement["up"]);
            controls.Down = bool.Parse((string)movement["down"]);
            controls.Left = bool.Parse((string)movement["left"]);
            controls.Right = bool.Parse((string)movement["right"]);
            controls.Run = bool.Parse((string)movement["shift"]);
            controls.Hit = bool.Parse((string)movement["hit"]);
            controls.Use = bool.Parse((string)movement["use"]);

        }

        /// <summary>
        /// update tick for user and animation.
        /// return next amount to move as relitive point
        /// </summary>
        public Point GetNetMoveAmount()
        {
            //Map map = GameServer.GetMapById(Map_Id);
            double moveX = 0;
            double moveY = 0;
            //double newDirection = Direction;
            double moveKeys = controls.CountDirectionKeys();
            //if (moveKeys == 0 || moveKeys == 4) { return new Point(0,0); }
            //double speedMove = (Speed / 5) / moveKeys;
            double modspeed = mods.Mods.ConvertRange(MIN_SPEED, MAX_SPEED, 1, 10, Speed);
            if (controls.Run)
            {
                modspeed += 2;
            }
            double speedMove = 0;
            if (moveKeys != 0 && moveKeys != 4)
            {
                speedMove = modspeed / moveKeys;
            }
            if (controls.Up)
            {
                moveY -= speedMove;
            }
            if (controls.Down)
            {
                moveY += speedMove;
            }
            if (controls.Left)
            {
                moveX -= speedMove;
            }
            if (controls.Right)
            {
                moveX += speedMove;
            }
            // set next animation.
            if (moveX > 0)
            {
                AnimationName = "walkRight";
                Direction = 270;
            } else if (moveX < 0)
            {
                AnimationName = "walkLeft";
                Direction = 90;
            } else if (moveY > 0)
            {
                AnimationName = "walkDown";
                Direction = 0;
            } else if (moveY < 0)
            {
                AnimationName = "walkUp";
                Direction = 180;
            } else
            {
                if (Direction < 45 || Direction > 315)
                {
                    AnimationName = "standDown";
                } else if (Direction >= 45 && Direction <= 135)
                {
                    AnimationName = "standLeft";
                } else if (Direction > 135 && Direction <= 225)
                {
                    AnimationName = "standUp";
                } else
                {
                    AnimationName = "standRight";
                }
            }
            //SaveUser();
            // return the x and the y
            return new Point(moveX, moveY);          
        }

        internal class UserControls
        {
            public volatile bool Up = false;
            public volatile bool Down = false;
            public volatile bool Left = false;
            public volatile bool Right = false;
            public volatile bool Run = false;
            public volatile bool Use = false;
            public volatile bool Hit = false;

            public int CountDirectionKeys()
            {
                int count = 0;
                if (Up) { count++; }
                if (Down) { count++; }
                if (Left) { count++; }
                if (Right) { count++; }
                return count;
            }

        }
    }
}
