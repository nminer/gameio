﻿using System.Data.SQLite;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using server.mapObjects;
using System.Numerics;
using server.monsters;
using System.Xml.Linq;

namespace server
{
    public class User : ICreature
    {
        private const double MIN_SPEED = 0.0;
        private const double MAX_SPEED = 100.0;

        private UserCounters counters;

        private UserControls controls = new UserControls();

        private object dbDataLock = new object();

        public double Height = 80;

        public double Width = 80;

        private Circle mySolid = new Circle(new Point(0, 0), 15);
        public Circle Solid
        {
            get { return mySolid; }
        }

        //private string[] Animations = {"walkDown", "walkUp", "walkLeft", "walkRight", "standDown", "standUp", "standLeft", "standRight", "swingDown", "swingUp", "swingLeft", "swingRight" };

        private string AnimationName = "standDown";

        private object coolDownLock = new object();
        private Double CoolDown = 0.0;
        private Double CoolDownCount = 0.0;
         
        public bool HasCoolDown
        {
            get
            {
                lock (coolDownLock)
                {
                    return CoolDown > 0 && CoolDownCount >= 0;
                }
            }
        }

        public void SetCoolDown(double cooldown)
        {
            lock (coolDownLock)
            {
                CoolDown = cooldown;
                CoolDownCount = cooldown;
            }
        }

        public void decCoolDown(double amount = 1.0) 
        {
            lock (coolDownLock)
            {
                CoolDownCount -= amount;
                if (CoolDownCount < 0)
                {
                    CoolDown = 0;
                    CoolDownCount = 0;
                }
            }
        }

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
        /// the max health the player can have.
        /// </summary>
		public Int64 MaxHealth
        {
            get
            {
                lock (dbDataLock)
                {
                    return (long)(((Int64)row["Max_Health"] + (Strength / 3)) * Death_Penalty);
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
        /// the current health of the player.
        /// </summary>
		public Int64 Health
        {
            get
            {
                lock (dbDataLock)
                {
                    if ((Int64)row["Health"] >= MaxHealth)
                    {
                        Health = MaxHealth;
                    }
                    return (Int64)row["Health"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    if (value >= MaxHealth)
                    {
                        row["Health"] = MaxHealth;
                    }
                    else if (value <= 0)
                    {
                        SetCoolDown(140);
                        AnimationName = "dieingDown";
                        row["Health"] = 0;
                    }
                    else
                    {
                        row["Health"] = value;
                    }
                }
            }
        }

        /// <summary>
        /// the players max Stamina. used for running and hitting.
        /// </summary>
        public Int64 MaxStamina
        {
            get
            {
                lock (dbDataLock)
                {
                    return (long)(((Int64)row["Max_Stamina"] + (Speed / 3)) * Death_Penalty);
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Max_Stamina"] = value;
                }
            }
        }

        /// <summary>
        /// the player current Stamina.
        /// </summary>
        public Int64 Stamina
        {
            get
            {
                lock (dbDataLock)
                {
                    if ((Int64) row["Stamina"] >= MaxStamina)
                    {
                        Stamina = MaxStamina;
                    }
                    return (Int64)row["Stamina"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    if (value >= MaxStamina)
                    {
                        row["Stamina"] = MaxStamina;
                    } else if (value < 0)
                    {
                        row["Stamina"] = 0;
                    }
                    else
                    {
                        row["Stamina"] = value;
                    } 
                }
            }
        }

        /// <summary>
        /// the players max mana. used formagic.
        /// </summary>
        public Int64 MaxMana
        {
            get
            {
                lock (dbDataLock)
                {
                    return (long)(((Int64)row["Max_Mana"] + (Wisdom / 3)) * Death_Penalty);
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Max_Mana"] = value;
                }
            }
        }

        /// <summary>
        /// the player current mana.
        /// </summary>
        public Int64 Mana
        {
            get
            {
                lock (dbDataLock)
                {
                    if ((Int64)row["Mana"] >= MaxMana)
                    {
                        Mana = MaxMana;
                    }
                    return (Int64)row["Mana"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    if (value >= MaxMana)
                    {
                        row["Mana"] = MaxMana;
                    }
                    else if (value < 0)
                    {
                        row["Mana"] = 0;
                    }
                    else
                    {
                        row["Mana"] = value;
                    }
                }
            }
        }

        /// <summary>
        /// players strength. up the health and the damage from melee weapons.
        /// </summary>
        public Int64 Strength
        {
            get
            {
                lock (dbDataLock)
                {
                    return (long)((Int64)row["Strength"] * Death_Penalty);
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
                    return (long)((Int64)row["Speed"] * Death_Penalty);
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

        public Int64 SpeedMoveMod
        {
            get
            {
                if (Stamina > 0)
                {
                    return Speed;
                }
                return Speed / 4;
            }
        }

        /// <summary>
        /// players Wisdom. this will up the mana and use of magic items.
        /// </summary>
        public Int64 Wisdom
        {
            get
            {
                lock (dbDataLock)
                {
                    return (long)((Int64)row["Wisdom"] * Death_Penalty);
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Wisdom"] = value;
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
                    mySolid.Center.X = value;
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
                    mySolid.Center.Y = value;
                }
            }
        }

        /// <summary>
        /// zero is down. clockwise to 359.
        /// </summary>
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

        /// <summary>
        /// amount of times the user has died.
        /// </summary>
        public Int64 Deaths
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Int64)row["Deaths"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                     row["Deaths"] = value;
                }
            }
        }

        public Int64 Spawn_Map_Id
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Int64)row["Spawn_Map_Id"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Spawn_Map_Id"] = value;
                }
            }
        }

        public Double Spawn_X
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Double)row["Spawn_X"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Spawn_X"] = value;
                }
            }
        }

        public Double Spawn_Y
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Double)row["Spawn_Y"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    row["Spawn_Y"] = value;
                }
            }
        }

        /// <summary>
        /// amount of death points the user has.
        /// this directly relates to the death penalty. 
        /// </summary>
        public Double Death_Points
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Double)row["Death_Points"];
                }
            }
            set
            {
                lock (dbDataLock)
                {
                    if (value < 0)
                    {
                        row["Death_Points"] = 0;
                    } else if (value > 0.4)
                    {
                        row["Death_Points"] = 0.4;
                    } else
                    {
                        row["Death_Points"] = value;
                    }                   
                }
            }
        }

        private double Death_Penalty
        {
            get
            {
                return 1 - Death_Points;
            }
        }

        /// <summary>
        /// reutrn the players x and y location as a Point.
        /// </summary>
        public Point Location
        {
            get
            {
                return new Point(X_Coord, Y_Coord);
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
            counters = new UserCounters(this);
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

        public double GetHitDistence()
        {
            return 10;
        }

        public long GitHitDamage()
        {
            Random rnd = new Random();
            return rnd.Next(1, 10);
        }

        public bool InHitDirection(double toHitDirection, double hitRange = 120)
        {
            hitRange = hitRange / 2;
            double anglediff = (Direction - toHitDirection + 180 + 360) % 360 - 180;
            return anglediff <= hitRange && anglediff >= -hitRange;
        }

        /// <summary>
        /// make user take damage
        /// returns true if damage was taken.
        /// </summary>
        /// <param name="damageAmount"></param>
        /// <returns></returns>
        public bool TakeDamage(long damageAmount, Map map)
        {
            lock (dbDataLock)
            {
                if (Health <= 0)
                {
                    return false;
                }
                counters.ResetRecharge();
                Health -= damageAmount;
            }
            return true;
        }

        public bool TakeDamage(long damageAmount, Monster monster, Map map)
        {
            lock (dbDataLock)
            {
                if (TakeDamage(damageAmount, map))
                {
                    if (Health > 0)
                    {
                        SocketServer.SendMessageToUser(this, damageAmount.ToString(), $"Damage from {monster.Name}");
                    } else
                    {
                        SocketServer.SendMessageToUser(this, damageAmount.ToString(), $"{monster.Name} killed you!");
                    }
                    return true;
                } 
            }
            return false;
        }

        public void Died()
        {
            counters.ResetRecharge();
            Death_Points += 0.05;
            Health = (long)(MaxHealth * .9);
            Stamina = (long)(MaxStamina * .9);
            Mana = (long)(MaxMana * .9);
            Deaths += 1;
        }

        public void SetSoulStone()
        {
            Spawn_Map_Id = Map_Id;
            Spawn_X = X_Coord;
            Spawn_Y = Y_Coord;
            SetCoolDown(60);
            if (Direction < 45 || Direction > 315)
            {
                AnimationName = "castDown";
            }
            else if (Direction >= 45 && Direction <= 135)
            {
                AnimationName = "castLeft";
            }
            else if (Direction > 135 && Direction <= 225)
            {
                AnimationName = "castUp";
            }
            else
            {
                AnimationName = "castRight";
            }
        }

        public SoundAffect GetTakeHitSound(bool critacalHit)
        {
            Random rnd = new Random();
            int i = rnd.Next(1, 10);
            string gender = "male";
            if (BodyStyle != "01")
            {
                gender = "female";
            }
            SoundAffect hit = new SoundAffect($"sounds/char/{gender}Hit{i}.wav", false, this.Location, 60, 200);
            return hit;
        } 

        /// <summary>
        /// returns json string for user.
        /// </summary>
        /// <returns></returns>
        public string GetJson()
        {
            if (this.HasCoolDown)
            {
                return JsonConvert.SerializeObject(
                    new
                    {
                        username = UserName,
                        avatar = new
                        {
                            body = BodyStyle,
                            bodyc = BodyColor,
                            nose = NoseStyle,
                            ears = EarStyle,
                            wrinkles = Wrinkles,
                            hair = HairStyle,
                            hairc = HairColor,
                            beard = BeardStyle,
                            beardc = BeardColor,
                            eyec = EyeColor
                        },
                        x = X_Coord,
                        y = Y_Coord,
                        direction = Direction,
                        speed = SpeedMoveMod,
                        animation = AnimationName,
                        running = controls.Run && Stamina > 0,
                        coolDown = CoolDown,
                        coolDownCount = CoolDownCount,
                        health = Health,
                        maxHealth = MaxHealth,
                        stamina = Stamina,
                        maxStamina = MaxStamina,
                        mana = Mana,
                        maxMana = MaxMana
                    }
                    );
            }
            return JsonConvert.SerializeObject(
                new { username = UserName,
                    avatar = new {body = BodyStyle, bodyc = BodyColor,
                                  nose = NoseStyle, ears = EarStyle, wrinkles = Wrinkles,
                                  hair = HairStyle, hairc = HairColor,
                                  beard = BeardStyle, beardc = BeardColor,
                                  eyec = EyeColor},
                    x = X_Coord, y = Y_Coord, direction = Direction,
                    speed = SpeedMoveMod,
                    animation = AnimationName,  running = controls.Run && Stamina > 0,
                    health = Health, maxHealth = MaxHealth,
                    stamina = Stamina, maxStamina = MaxStamina,
                    mana = Mana, maxMana = MaxMana
                }
                );
        }

        public void UpdateFromPlayer(JObject movement)
        {
            bool oldUse = controls.Use; 
            bool oldHit = controls.Hit;
            controls.Up = bool.Parse((string)movement["up"]);
            controls.Down = bool.Parse((string)movement["down"]);
            controls.Left = bool.Parse((string)movement["left"]);
            controls.Right = bool.Parse((string)movement["right"]);
            controls.Run = bool.Parse((string)movement["shift"]);
            controls.Use = bool.Parse((string)movement["use"]);                  
            controls.Hit = bool.Parse((string)movement["hit"]);
            if (HasCoolDown) return; 
            if (!oldUse && controls.Use)
            {
                GameServer.CheckForUserUse(this);
            }
        }

        /// <summary>
        /// update tick for user and animation.
        /// return next amount to move as relative point
        /// </summary>
        public Point GetNetMoveAmount()
        {
            if (Health <= 0)
            {
                if (HasCoolDown)
                {
                    decCoolDown();
                }
                return new Point(0, 0);
            }
            if (HasCoolDown)
            {
                decCoolDown();
                if (HasCoolDown)
                {
                    return new Point(0, 0);
                }
                int temp = 0;
            }
            if (controls.Hit)
            {
                if (Direction < 45 || Direction > 315)
                {
                    AnimationName = "swingDown";
                }
                else if (Direction >= 45 && Direction <= 135)
                {
                    AnimationName = "swingLeft";
                }
                else if (Direction > 135 && Direction <= 225)
                {
                    AnimationName = "swingUp";
                }
                else
                {
                    AnimationName = "swingRight";
                }
                // this is not the place i want to keep the sound for the user.
                SoundAffect punch = new SoundAffect("sounds/char/punch.wav", false, this.Location, 60, 200);
                GameServer.AddGameSoundAffect(this.Map_Id, punch);
                GameServer.GetMapById(this.Map_Id).PlayerHit(this);
                SetCoolDown(Stamina > 2 ? 40 : 60);
                Stamina = Stamina - 3; // hit takes 3 stam
                counters.ResetRecharge();
                return new Point(0, 0);
            }
            //Map map = GameServer.GetMapById(Map_Id);
            double moveX = 0;
            double moveY = 0;
            //double newDirection = Direction;
            double moveKeys = controls.CountDirectionKeys();
            //if (moveKeys == 0 || moveKeys == 4) { return new Point(0,0); }
            //double speedMove = (Speed / 5) / moveKeys;
            double modspeed = Mods.ConvertRange(MIN_SPEED, MAX_SPEED, 1, 10, SpeedMoveMod);
            if (controls.Run && Stamina > 0)
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
            if (controls.Run && (moveX != 0 || moveY != 0))
            {
                counters.DecCountStamina();
            }
            if (moveKeys == 0)
            {
                counters.Recharge();
            } else
            {
                counters.ResetRecharge();
            }
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

        /// <summary>
        /// class to keep up with health mana and stam decreasing or increasing over time.
        /// </summary>
        public class UserCounters
        {
            private int WaitTimeMax
            {
                get
                {
                    long v = 100 + (100 - user.Speed);
                    return (int)v;
                }
            }

            private int WaitTime = 0;

            private int HealthCountMax = 100;
            private int StaminaCountMax = 50;
            private int ManaCountMax = 80;

            private int HealthCountMin = -10;
            private int StaminaCountMin = -10;
            private int ManaCountMin = -10;

            private int HealthCount = 0;
            private int StaminaCount = 0;
            private int ManaCount = 0;

            private User user;

            public UserCounters(User userIn)
            {
                user = userIn;
            }

            public void CountRecharge(int count = 1)
            {
                WaitTime = WaitTime + count;
                if (WaitTime >= WaitTimeMax)
                {
                    IncCountStamina();
                    IncCountHealth();
                    IncCountMana();
                }
            }

            public void ResetRecharge()
            {
                WaitTime = 0;
            }

            public void Recharge()
            {
                CountRecharge(1);
            }

            public void CountStamina(int count = 1)
            {
                StaminaCount = StaminaCount + count;
                if (StaminaCount >= StaminaCountMax)
                {
                    user.Stamina = user.Stamina + (StaminaCount / StaminaCountMax);
                    StaminaCount = StaminaCount % StaminaCountMax;
                } else if (StaminaCount <= StaminaCountMin)
                {
                    user.Stamina = user.Stamina - (StaminaCount / StaminaCountMin);
                    StaminaCount = StaminaCount % StaminaCountMin;
                }
            }

            public void IncCountStamina()
            {
                CountStamina(1);
            }

            public void DecCountStamina()
            {
                CountStamina(-1);
            }

            public void CountHealth(int count = 1)
            {
                HealthCount = HealthCount + count;
                if (HealthCount >= HealthCountMax)
                {
                    user.Health = user.Health + (HealthCount / HealthCountMax);
                    HealthCount = HealthCount % HealthCountMax;
                }
                else if (HealthCount <= HealthCountMin)
                {
                    user.Health = user.Health - (HealthCount / HealthCountMin);
                    HealthCount = HealthCount % HealthCountMin;
                }
            }
            public void IncCountHealth()
            {
                CountHealth(1);
            }

            public void DecCountHealth()
            {
                CountHealth(-1);
            }

            public void CountMana(int count = 1)
            {
                ManaCount = ManaCount + count;
                if (ManaCount >= ManaCountMax)
                {
                    user.Mana = user.Mana + (ManaCount / ManaCountMax);
                    ManaCount = ManaCount % ManaCountMax;
                }
                else if (ManaCount <= ManaCountMin)
                {
                    user.Mana = user.Mana - (ManaCount / ManaCountMin);
                    ManaCount = ManaCount % ManaCountMin;
                }
            }
            public void IncCountMana()
            {
                CountMana(1);
            }

            public void DecCountMana()
            {
                CountMana(-1);
            }

        }

    }
}
