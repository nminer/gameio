using server.mapObjects;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace server.monsters
{
    internal class MonsterAnimation
    {
        private object dbDataLock = new object();

        private SQLiteDataAdapter? adapter;

        private SQLiteCommandBuilder? builder;

        private DataSet? data;

        private DataRow? row;

        public Point AnimationPosition = new Point(0, 0);

        public bool Horizontal = true;

        /// <summary>
        /// The monster animation Id in the database.
        /// </summary>
        public Int64 MonsterAnimationId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Monster_Animation_Id"];
                }
            }
        }

        /// <summary>
        /// the id of the monster type this animation is a part of.
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
        /// the name of the animation walkup, walkdown ....
        /// </summary>
        public string Animation
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return "";
                    }
                    return (string)row["Animation"];
                }
            }
        }

        public string ImagePath
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return "";
                    }
                    return (string)row["Image_Path"];
                }
            }
        }


        /// <summary>
        /// height of the animation
        /// </summary>
        public Int64 Height
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Height"];
                }
            }
        }

        /// <summary>
        /// width of the animation
        /// </summary>
        public Int64 Width
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Width"];
                }
            }
        }

        /// <summary>
        /// number of frames in the animation
        /// </summary>
        public Int64 Frames
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Frames"];
                }
            }
        }

        /// <summary>
        /// the base slowdown for the animation
        /// </summary>
        public Int64 Slowdown
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Slowdown"];
                }
            }
        }

        public string AfterAnimationName
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return "";
                    }
                    return (string)row["After_Animation_Name"];
                }
            }
        }

        public Int64 StarFrame
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Star_Frame"];
                }
            }
        }
        public MonsterAnimation(long monsterAnimationId)
        {
            LoadFromId(monsterAnimationId);
        }

        private void LoadFromId(long monsterAnimationId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findShape = $"SELECT * FROM Monster_Animations WHERE Monster_Animation_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findShape, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", monsterAnimationId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                row = data.Tables[0].Rows[0];
                AnimationPosition = new Point((Int64)row["X"], (Int64)row["Y"]);
                Horizontal = (Int64)row["Horizontal"] == 1;
            }
        }

        /// <summary>
        /// set the solids position on the map. 
        /// this is default 0 0 if left null.
        /// </summary>
        /// <param name="shapePosition"></param>
        static public MonsterAnimation? Create(long Monster_Type_Id, Monster.AnimationNames Animation, string Image_Path, long X, long Y, long Height,long Width, long Frames, long Slowdown, string After_Animation_Name = "", long Star_Frame = 0, bool Horizontal = true)
        {
            string insertNewSolid = $"INSERT INTO Monster_Animations (Monster_Type_Id, Animation, Image_Path, X, Y, Height, Width, Frames, Slowdown, After_Animation_Name, Star_Frame, Horizontal)" +
                $" VALUES($Monster_Type_Id, $Animation, $Image_Path, $X, $Y, $Height, $Width, $Frames, $Slowdown, $After_Animation_Name, $Star_Frame, $Horizontal);";
            SQLiteCommand command = new SQLiteCommand(insertNewSolid, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$Monster_Type_Id", Monster_Type_Id);
            command.Parameters.AddWithValue("$Animation", Monster.AnimationEnumToName(Animation));
            command.Parameters.AddWithValue("$Image_Path", Image_Path);
            command.Parameters.AddWithValue("$X", X);
            command.Parameters.AddWithValue("$Y", Y);
            command.Parameters.AddWithValue("$Height", Height);
            command.Parameters.AddWithValue("$Width", Width);
            command.Parameters.AddWithValue("$Frames", Frames);
            command.Parameters.AddWithValue("$Slowdown", Slowdown);
            command.Parameters.AddWithValue("$After_Animation_Name", After_Animation_Name);
            command.Parameters.AddWithValue("$Star_Frame", Star_Frame);
            command.Parameters.AddWithValue("$Horizontal", Horizontal);
            SQLiteTransaction transaction = null;
            try
            {
                transaction = DatabaseBuilder.Connection.BeginTransaction();
                if (command.ExecuteNonQuery() > 0)
                {
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    return new MonsterAnimation(rowID);
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


        public object? GetJsonMonsterAnimationOject()
        {
            return new
            {
                name = Animation,
                image = ImagePath,
                frames = Frames,
                x = AnimationPosition.X,
                y = AnimationPosition.Y,
                width = Width,
                height = Height,
                slowdown = Slowdown,
                after = AfterAnimationName,
                startFrame = StarFrame,
                horizontal = Horizontal,
            };
        }
    }
}
