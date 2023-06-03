﻿using System.Data.SQLite;
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

namespace server
{
    class User
    {
        private UserControls controls = new UserControls();

        /// <summary>
        /// The users id in the database.
        /// </summary>
		public Int64 UserId
        {
            get => (Int64)row["User_Id"];
        }

        /// <summary>
        /// the user name.
        /// </summary>
		public string UserName
        {
            get => (string)row["UserName"];
        }

        /// <summary>
        /// The users password. hashed?
        /// </summary>
		public string Password
        {
            get => (string)row["PasswordHash"];
        }

        /// <summary>
        /// The users level.
        /// </summary>
		public Int64 Level
        {
            get => (Int64)row["Level"];
            set
            {
                row["Level"] = value;
            }
        }

        /// <summary>
        /// the max helth the player can have.
        /// </summary>
		public Int64 MaxHealth
        {
            get => (Int64)row["Max_Health"];
            set
            {
                row["Max_Health"] = value;
            }
        }

        /// <summary>
        /// the current helth of the player.
        /// </summary>
		public Int64 Health
        {
            get => (Int64)row["Health"];
            set
            {
                row["Health"] = value;
            }
        }

        /// <summary>
        /// the players max stamana. used for running and hitting.
        /// </summary>
        public Int64 MaxStamana
        {
            get => (Int64)row["Max_Stamana"];
            set
            {
                row["Max_Stamana"] = value;
            }
        }

        /// <summary>
        /// the player current stamana.
        /// </summary>
        public Int64 Stamana
        {
            get => (Int64)row["Stamana"];
            set
            {
                row["Stamana"] = value;
            }
        }

        /// <summary>
        /// players strength.
        /// </summary>
        public Int64 Strength
        {
            get => (Int64)row["Strength"];
            set
            {
                row["Strength"] = value;
            }
        }

        /// <summary>
        /// players speed. how fast they move and hit.
        /// </summary>
        public Int64 Speed
        {
            get => (Int64)row["Speed"];
            set
            {
                row["Speed"] = value;
            }
        }

        /// <summary>
        /// what map the player is on. 
        /// </summary>
        public Int64 Map_Id
        {
            get => (Int64)row["Map_Id"];
            set
            {
                row["Map_Id"] = value;
            }
        }

        /// <summary>
        /// the x cood on the current map. this is from the rop left of the map.
        /// </summary>
        public Double X_Coord
        {
            get => (Double)row["X_Coordinate"];
            set
            {
                row["X_Coordinate"] = value;
            }
        }

        /// <summary>
        /// the y cood on the current map. this is from the rop left of the map.
        /// </summary>
        public Double Y_Coord
        {
            get => (Double)row["Y_Coordinate"];
            set
            {
                row["Y_Coordinate"] = value;
            }
        }

        public Int64 Direction
        {
            get => (Int64)row["Direction"];
            set
            {
                if (value >= 0 && value <= 8)
                {
                    row["Direction"] = value;
                }
                else
                {
                    row["Direction"] = 0;
                }

            }
        }

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

        static public User? Create(string userName, string password)
        {
            SQLiteDataReader dataReader = CheckUserName(userName);
            if (dataReader.HasRows)
            {
                return null;
            }
            // insert new user
            string insertNewUser = $"INSERT INTO User (UserName, PasswordHash) VALUES($name, $pass);";
            SQLiteCommand command = new SQLiteCommand(insertNewUser, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$name", userName);
            command.Parameters.AddWithValue("$pass", BCrypt.Net.BCrypt.HashPassword(password));
            try
            {
                if (command.ExecuteNonQuery() > 0)
                {
                    // user log in to return new user.
                    return LogIn(userName, password);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
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
            return JsonConvert.SerializeObject(new { username = UserName, x = X_Coord, y = Y_Coord, direction = Direction});
        }

        public void UpdateFromPlayer(JObject movement)
        {
            controls.Up = bool.Parse((string)movement["up"]);
            controls.Down = bool.Parse((string)movement["down"]);
            controls.Left = bool.Parse((string)movement["left"]);
            controls.Right = bool.Parse((string)movement["right"]);
            controls.Hit = bool.Parse((string)movement["hit"]);
            controls.Use = bool.Parse((string)movement["use"]);
        }

        /// <summary>
        /// update tick for user.
        /// </summary>
        public void TickGameUpdate()
        {
            double newX = X_Coord;
            double newY = Y_Coord;
            double newDirection = Direction;
            double moveKeys = controls.CountDirectionKeys();
            if (moveKeys > 0)
            {
                double speedMove = (Speed / 5) / moveKeys;
                if (controls.Up)
                {
                    newY -= speedMove;
                }
                if (controls.Down)
                {
                    newY += speedMove;
                }
                if (controls.Left)
                {
                    newX -= speedMove;
                }
                if (controls.Right)
                {
                    newX += speedMove;
                }
                // add in colletion detection with map here
                Y_Coord = newY;
                X_Coord = newX;
            }
        }

        internal class UserControls
        {
            public volatile bool Up = false;
            public volatile bool Down = false;
            public volatile bool Left = false;
            public volatile bool Right = false;
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
