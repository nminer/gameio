﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    static class UserSystem
    {
        private static Dictionary<string, User> SessionIdToUser = new Dictionary<string, User>();
        private static Dictionary<string, string> UserNameToSession = new Dictionary<string, string>();
        private static object SessionUserLock = new object();

        /// <summary>
        /// All logged in users.
        /// </summary>
        private static Dictionary<string, User> LoggedInUsers = new Dictionary<string, User>();

        /// <summary>
        /// keep all the user grouped by location.
        /// key map id
        /// value list of users in map.
        /// </summary>
        private static Dictionary<Int64, List<User>> UsersLocated = new Dictionary<Int64, List<User>>();

        /// <summary>
        /// keep the dictionaries all in line.
        /// </summary>
        private static object UserLock = new object();

        /// <summary>
        /// log a user in reutrns the user or null if it can not be logged in.
        /// throws exception if login does not work.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static User? Login(string username, string password)
        {
            lock (UserLock)
            {
                if (LoggedInUsers.ContainsKey(username))
                {
                    return null;
                }
                else
                {
                    User loggedInUser = User.LogIn(username, password);
                    LoggedInUsers.Add(username, loggedInUser);
                    //AddUserToLocation(loggedInUser);
                    return loggedInUser;
                }
            }
        }

        public static bool FinalizeLogin(User user)
        {
            //LoggedInUsers.Add(user.UserName, user);
            AddUserToLocation(user);
            return true;
        }
        /// <summary>
        /// add passed in user to the location dictionary 
        /// </summary>
        /// <param name="userToAdd"></param>
        private static void AddUserToLocation(User userToAdd)
        {
            lock (UserLock)
            {
                if (!UsersLocated.ContainsKey(userToAdd.Map_Id))
                {
                    UsersLocated.Add(userToAdd.Map_Id, new List<User>());
                }
                UsersLocated[userToAdd.Map_Id].Add(userToAdd);
            }
        }

        /// <summary>
        /// log a user out 
        /// returns true if the user was logged out
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static bool Logout(string username)
        {
            lock (UserLock)
            {
                if (!LoggedInUsers.ContainsKey(username)) { return false; }
                User user = LoggedInUsers[username];
                RemoveUserFromLocation(user);
                RemoveUserFromSession(user);
                return LoggedInUsers.Remove(username);
            }
        }

        /// <summary>
        /// remove the user from the location dictionary 
        /// </summary>
        /// <param name="userToRemove"></param>
        private static void RemoveUserFromLocation(User userToRemove)
        {
            lock (UserLock)
            {
                if (UsersLocated.ContainsKey(userToRemove.Map_Id))
                {
                    UsersLocated[userToRemove.Map_Id].Remove(userToRemove);
                    if (UsersLocated[userToRemove.Map_Id].Count == 0)
                    {
                        UsersLocated.Remove(userToRemove.Map_Id);
                    }
                }
            }
        }

        private static void RemoveUserFromSession(User userToRemove)
        {
            lock (SessionUserLock)
            {
                if (UserNameToSession.ContainsKey(userToRemove.UserName))
                {
                    string keyToRemove = UserNameToSession[userToRemove.UserName];
                    UserNameToSession.Remove(userToRemove.UserName);
                    SessionIdToUser.Remove(keyToRemove);
                }
            }
        }
        /// <summary>
        /// set the user to a new location map and coords
        /// </summary>
        /// <param name="userToChange"></param>
        /// <param name="newMapId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void ChangeUsersLocation(User userToChange, int newMapId, Int64 x, Int64 y)
        {
            if (userToChange.Map_Id != newMapId)
            {
                RemoveUserFromLocation(userToChange);
                userToChange.Map_Id = newMapId;
                userToChange.X_Coord = x;
                userToChange.Y_Coord = y;
                AddUserToLocation(userToChange);
            }
            else
            {
                userToChange.X_Coord = x;
                userToChange.Y_Coord = y;
            }
        }

        /// <summary>
        /// reurn the logged in user with name.
        /// returns null if no user for name is loged in.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static User? GetLoggedInUser(string username)
        {
            lock (UserLock)
            {
                if (!LoggedInUsers.ContainsKey(username))
                {
                    return LoggedInUsers[username];
                }
            }
            return null;
        }

        /// <summary>
        /// create a new user and log them in.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static User? CreateNewUser(string userName, string password)
        {
            User.Create(userName, password);
            return Login(userName, password);
        }


        public static string AssignSessionId(User user)
        {
            string id = "";
            lock (SessionUserLock)
            {
                do
                {
                    StringBuilder builder = new StringBuilder();
                    Enumerable
                       .Range(65, 26)
                        .Select(e => ((char)e).ToString())
                        .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
                        .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
                        .OrderBy(e => Guid.NewGuid())
                        .Take(11)
                        .ToList().ForEach(e => builder.Append(e));
                    id = builder.ToString();
                } while (SessionIdToUser.ContainsKey(id));
                SessionIdToUser.Add(id, user);  
                UserNameToSession.Add(user.UserName, id);
            }
            return id;
        }

    }
}
