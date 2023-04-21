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

namespace server
{
    internal class User {
        /// <summary>
        /// The users id in the database.
        /// </summary>
		public int UserId
        {
            get => (int)row["User_Id"];
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
            get => (string)row["Password"];
        }

        /// <summary>
        /// The users level.
        /// </summary>
		public int Level
		{
			get => (int)row["Level"];
			set
			{
				row["Level"] = value;
			}
		}

        /// <summary>
        /// the max helth the player can have.
        /// </summary>
		public int MaxHealth
		{
            get => (int)row["Max_Health"];
            set
            {
                row["Max_Health"] = value;
            }
        }

        /// <summary>
        /// the current helth of the player.
        /// </summary>
		public int Health
        {
            get => (int)row["Health"];
            set
            {
                row["Health"] = value;
            }
        }

        /// <summary>
        /// the players max stamana. used for running and hitting.
        /// </summary>
        public int MaxStamana
        {
            get => (int)row["Max_Stamana"];
            set
            {
                row["Max_Stamana"] = value;
            }
        }

        /// <summary>
        /// the player current stamana.
        /// </summary>
        public int Stamana
        {
            get => (int)row["Stamana"];
            set
            {
                row["Stamana"] = value;
            }
        }

        /// <summary>
        /// players strength.
        /// </summary>
        public int Strength
        {
            get => (int)row["Strength"];
            set
            {
                row["Strength"] = value;
            }
        }

        /// <summary>
        /// players speed. how fast they move and hit.
        /// </summary>
        public int Speed
        {
            get => (int)row["Speed"];
            set
            {
                row["Speed"] = value;
            }
        }

        /// <summary>
        /// what map the player is on. 
        /// </summary>
        public int Map_Id
        {
            get => (int)row["Map_Id"];
            set
            {
                row["Map_Id"] = value;
            }
        }

        /// <summary>
        /// the x cood on the current map. this is from the rop left of the map.
        /// </summary>
        public int X_Coord
        {
            get => (int)row["X_Coordinate"];
            set
            {
                row["X_Coordinate"] = value;
            }
        }

        /// <summary>
        /// the y cood on the current map. this is from the rop left of the map.
        /// </summary>
        public int Y_Coord
        {
            get => (int)row["Y_Coordinate"];
            set
            {
                row["Y_Coordinate"] = value;
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
        public User(int userId)
		{
            LoaderUser(userId);
        }


		private void LoaderUser(int userId)
		{
            adapter = new SQLiteDataAdapter();
            builder = new SQLiteCommandBuilder(adapter);
            data = new DataSet();
            string findUser = $"SELECT * FROM User WHERE User_Id=$id;";
            SQLiteCommand command  = new SQLiteCommand(findUser, DatabaseBuilder.Connection);
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
        static User? LogIn(string userName, string password)
		{
            SQLiteDataReader dataReader = CheckUserName(userName);
            if (!dataReader.HasRows)
            {
				return null;
			}
			dataReader.Read();
			int id = dataReader.GetInt32(0);
			User tempUser = new User(id);
            if (BCrypt.Net.BCrypt.Verify(password, tempUser.Password))
            {
                return tempUser;
            } else
            {
                throw new Exception("Invalid login.");
            }
        }

        static User? Create(string userName, string password)
        {
            SQLiteDataReader dataReader = CheckUserName(userName);
            if (dataReader.HasRows)
            {
                return null;
            }
            // insert new user
            string insertNewUser = $"INSERT User (UserName, PasswordHash) VALUES($name, $pass);";
            SQLiteCommand command = new SQLiteCommand(insertNewUser, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$name", userName);
            command.Parameters.AddWithValue("$pass", password);
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
        static private SQLiteDataReader CheckUserName (string userName)
        {
            SQLiteCommand cmd = DatabaseBuilder.Connection.CreateCommand();
            string findUser = $"SELECT User_Id FROM User WHERE UserName='{userName}'";
            cmd.CommandText = findUser;
            SQLiteDataReader dataReader = cmd.ExecuteReader();
            return dataReader;
        }

        static string hashPasswod(string password)
		{
			return password;
		}


		public int SaveUser()
		{
			builder.ConflictOption = ConflictOption.OverwriteChanges;
			builder.GetUpdateCommand();
			return adapter.Update(data);
        }

    }
}
