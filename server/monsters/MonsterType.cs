using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using server.mapObjects;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace server.monsters
{
    //TODO Add a dose monster have animation
    public class MonsterType
    {
        private object dbDataLock = new object();

        private SQLiteDataAdapter? adapter;

        private SQLiteCommandBuilder? builder;

        private DataSet? data;

        private DataRow? row;

        private List<MonsterAnimation> animations = new List<MonsterAnimation>();
        private Object animationsLock = new Object();

        /// <summary>
        /// The monster type Id in the database.
        /// </summary>
        public Int64 MonsterTypeId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Monster_Type_Id"];
                }
            }
        }

        /// <summary>
        /// the name of monster type
        /// </summary>
        public string Type
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return "";
                    }
                    return (string)row["Type"];
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

        public long Height
        {
            get
            {
                if (animations.Count == 0)
                {
                    return 0;
                }
                MonsterAnimation animation = animations[0];
                return animation.DrawHeight;
            }
        }
        public long Width
        {
            get
            {
                if (animations.Count == 0)
                {
                    return 0;
                }
                MonsterAnimation animation = animations[0];
                return animation.DrawWidth;
            }
        }

        public MonsterType(long monsterTypeId)
        {
            LoadFromId(monsterTypeId);
        }

        private void LoadFromId(long monsterTypeId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findType = $"SELECT * FROM Monster_Types WHERE Monster_Type_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findType, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", monsterTypeId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                row = data.Tables[0].Rows[0];
                LoadAnimations();
            }
        }

        public MonsterType AddNewAnimation(MonsterAnimationAttributes animationAttr)
        {
            animationAttr.Monster_Type_Id = MonsterTypeId;
            MonsterAnimation? ma = MonsterAnimation.Create(animationAttr); 
            if (ma != null )
            {
                lock (animationsLock)
                {
                    animations.Add(ma);
                }
            }
            return this;
        }

        public MonsterType AddAnimation(MonsterAnimation animation)
        {
            lock (animationsLock)
            {
                animations.Add(animation);
            }
            return this;
        } 

        private void LoadAnimations()
        {
            lock (animationsLock)
            {
                animations.Clear();
                SQLiteDataAdapter adapterAnimations = new SQLiteDataAdapter();

                SQLiteCommandBuilder builderAnimations = new SQLiteCommandBuilder(adapterAnimations);

                DataSet dataAnimations = new DataSet();
                string queryAnimations = $"SELECT * FROM Monster_Animations WHERE Monster_Type_Id=$id;";
                SQLiteCommand commandMapSolids = new SQLiteCommand(queryAnimations, DatabaseBuilder.Connection);
                commandMapSolids.Parameters.AddWithValue("$id", MonsterTypeId);
                adapterAnimations.SelectCommand = commandMapSolids;
                adapterAnimations.Fill(dataAnimations);
                if (dataAnimations.Tables.Count > 0 && dataAnimations.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow r in dataAnimations.Tables[0].Rows)
                    {
                        MonsterAnimation ms = new MonsterAnimation((Int64)r["Monster_Animation_Id"]);
                        animations.Add(ms);
                    }

                }
            }
        }

        /// <summary>
        /// new monster type this is used to hold/connect monster animations
        /// </summary>
        /// <param name="shapePosition"></param>
        static public MonsterType? Create(string Type, string Description = "")
        {
            string insertNewSolid = $"INSERT INTO Monster_Types (Type, Description)" +
                $" VALUES($Type, $Description);";
            SQLiteCommand command = new SQLiteCommand(insertNewSolid, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$Type", Type);
            command.Parameters.AddWithValue("$Description", Description);
            SQLiteTransaction transaction = null;
            try
            {
                transaction = DatabaseBuilder.Connection.BeginTransaction();
                if (command.ExecuteNonQuery() > 0)
                {
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    return new MonsterType(rowID);
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

        private object? GetJsonMonsterAnimationOjects()
        {
            List<Object> animationList = new List<object>();
            lock (animationsLock)
            {
                foreach (MonsterAnimation ma in animations)
                {
                    animationList.Add(ma.GetJsonMonsterAnimationOject());
                }
            }
            return new
            {
                monsterToLoad = new
                {
                    type = MonsterTypeId,
                    animations = animationList.ToArray()
                }
            };
        }

        public string GetJsonMonsterTypeString()
        {
            return JsonConvert.SerializeObject(GetJsonMonsterAnimationOjects());
        }

    }
}
