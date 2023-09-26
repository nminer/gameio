using server.mapObjects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace server.monsters
{
    class Monster
    {
        private MonsterType type;

        private string name;

        private long level;

        private long maxHealth;

        private long health;

        private long maxStamina;

        private long stamina;

        private long maxMana;

        private long mana;

        private long strength;

        private long speed;

        private long wisdom;

        private long aggressiveDistance;

        private long chaseDistance;

        private long minDamage;

        private long maxDamage;

        private GameSound? AttackSound;
        private GameSound? IdleSound;
        private GameSound? ChaseSound;

        public Point MapPosition = new Point(0, 0);

        private string currentAnimation = "standDown";


        private long databaseId;
        public long DatabaseId
        {
            get
            {
                return databaseId;
            }
        }
        
        private string id;
        /// <summary>
        /// The monster animation Id in the database.
        /// </summary>
        public string MonsterId
        {
            get
            {
                return id;
            }
        }

        /// <summary>
        /// calculate from animation and speed. TODO
        /// </summary>
        public Int64 Slowdown
        {
            get
            {
                 return 10;
            }
        }

        public Monster(long monsterId, Point? location = null)
        {
            LoadFromId(monsterId);
            if (location is null)
            {
                MapPosition = new Point(0, 0);
            } else
            {
                MapPosition = location;
            }
        }

        public void setLocation(Point location)
        {
            MapPosition = location;
        }

        private void LoadFromId(long monsterId)
        {
            databaseId = monsterId;
            SQLiteDataAdapter adapter = new SQLiteDataAdapter();
            SQLiteCommandBuilder builder = new SQLiteCommandBuilder(adapter);
            DataSet data = new DataSet();
            string findShape = $"SELECT * FROM Monsters WHERE Monster_Id=$id;";
            SQLiteCommand command = new SQLiteCommand(findShape, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$id", monsterId);
            adapter.SelectCommand = command;
            adapter.Fill(data);
            DataRow row = data.Tables[0].Rows[0];
            loadMonsterType((Int64)row["Monster_Type_Id"]);
            name = (string)row["name"];
            level = (Int64)row["Level"];
            maxHealth = (Int64)row["Health"];
            health = (Int64)row["Health"];
            maxStamina = (Int64)row["Stamina"];
            stamina = (Int64)row["Stamina"];
            maxMana = (Int64)row["Mana"];
            mana = (Int64)row["Mana"];
            strength = (Int64)row["Strength"];
            speed = (Int64)row["Speed"];
            wisdom = (Int64)row["Wisdom"];
            aggressiveDistance = (Int64)row["Aggressive_Distance"];
            chaseDistance = (Int64)row["Chase_Distance"];
            minDamage = (Int64)row["Min_Damage"];
            maxDamage = (Int64)row["Max_Damage"];
            loadSounds(row);
        }

        private void loadSounds(DataRow row)
        {
            if ((Int64)row["Attack_Sound_Id"] >  0)
            {
                AttackSound = new GameSound((Int64)row["Attack_Sound_Id"]);
            }
            if ((Int64)row["Idle_Sound_Id"] > 0)
            {
                IdleSound = new GameSound((Int64)row["Idle_Sound_Id"]);
            }
            if ((Int64)row["Chase_Sound_Id"] > 0)
            {
                ChaseSound = new GameSound((Int64)row["Chase_Sound_Id"]);
            }
            if ((Int64)row["Death_Sound_Id"] > 0)
            {
                ChaseSound = new GameSound((Int64)row["Death_Sound_Id"]);
            }
        }

        private void loadMonsterType(long typeId)
        {
            type = new MonsterType(typeId);
        }

        /// <summary>
        /// set the solids position on the map. 
        /// this is default 0 0 if left null.
        /// </summary>
        /// <param name="shapePosition"></param>
        static public Monster? Create(long Monster_Type_Id, string Name, long Level, long Health, long Stamina, long Mana, long Strength, long Speed, long Wisdom, long Aggressive_Distance, long Chase_Distance, long Min_Damage, long Max_Damage, long Attack_Sound_Id = 0, long Idle_Sound_Id = 0, long Chase_Sound_Id = 0, long Death_Sound_Id = 0)
        {
            string insertNewSolid = $"INSERT INTO Monster_Animations (Monster_Type_Id, Name, Level, Health, Stamina, Mana, Strength, Speed, Wisdom, Aggressive_Distance, Chase_Distance, Min_Damage, Max_Damage, Attack_Sound_Id, Idle_Sound_Id, Chase_Sound_Id, Death_Sound_Id)" +
                $" VALUES($Monster_Type_Id, $Name, $Level, $Health, $Stamina, $Mana, $Strength, $Speed, $Wisdom, $Aggressive_Distance, $Chase_Distance, $Min_Damage, $Max_Damage, $Attack_Sound_Id, $Idle_Sound_Id, $Chase_Sound_Id, $Death_Sound_Id);";
            SQLiteCommand command = new SQLiteCommand(insertNewSolid, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$Monster_Type_Id", Monster_Type_Id);
            command.Parameters.AddWithValue("$Name", Name);
            command.Parameters.AddWithValue("$Level", Level);
            command.Parameters.AddWithValue("$Health", Health);
            command.Parameters.AddWithValue("$Stamina", Stamina);
            command.Parameters.AddWithValue("$Mana", Mana);
            command.Parameters.AddWithValue("$Strength", Strength);
            command.Parameters.AddWithValue("$Speed", Speed);
            command.Parameters.AddWithValue("$Wisdom", Wisdom);
            command.Parameters.AddWithValue("$Aggressive_Distance", Aggressive_Distance);
            command.Parameters.AddWithValue("$Chase_Distance", Chase_Distance);
            command.Parameters.AddWithValue("$Min_Damage", Min_Damage);
            command.Parameters.AddWithValue("$Max_Damage", Max_Damage);
            command.Parameters.AddWithValue("$Attack_Sound_Id", Idle_Sound_Id);
            command.Parameters.AddWithValue("$Idle_Sound_Id", Idle_Sound_Id);
            command.Parameters.AddWithValue("$Chase_Sound_Id", Chase_Sound_Id);
            command.Parameters.AddWithValue("$Death_Sound_Id", Death_Sound_Id);
            SQLiteTransaction transaction = null;
            try
            {
                transaction = DatabaseBuilder.Connection.BeginTransaction();
                if (command.ExecuteNonQuery() > 0)
                {
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    return new Monster(rowID);
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


        public object? GetJsonMonsterOject()
        {
            return new
            {
                id = MonsterId,
                type = type.MonsterTypeId,
                name = name,
                slowdown = Slowdown,
                x = MapPosition.X,
                y = MapPosition.Y,
                animation = currentAnimation
            };
        }


        public enum AnimationNames
        {
            walkDown,
            walkUp,
            walkLeft,
            walkRight,
            standDown,
            standUp,
            standLeft,
            standRight,
            swingDown,
            swingUp,
            swingLeft,
            swingRight,
            castDown,
            castUp,
            castLeft,
            castRight,
            dieingDown
        }
        public static AnimationNames? AnimationNameToEnum(String animationString)
        {
            AnimationNames name;
            if (Enum.TryParse(animationString, out name))
            {
                return name;
            }
            return null;
        }

        public static string? AnimationEnumToName(AnimationNames nameEnum)
        {
            return Enum.GetName(typeof(AnimationNames), nameEnum);
        }

    }


}