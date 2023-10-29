using server.mapObjects;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using static server.monsters.Monster;

namespace server.monsters
{
    public class MonsterAnimation
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
        /// height of the animation when drawn on the canvus
        /// </summary>
        public Int64 DrawHeight
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Draw_Height"];
                }
            }
        }

        /// <summary>
        /// width of the animation when drawn on the canvus
        /// </summary>
        public Int64 DrawWidth
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Draw_Width"];
                }
            }
        }

        /// <summary>
        /// where the solid of the monsters is from the draw top left
        /// </summary>
        public Int64 Solid_X
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Solid_X"];
                }
            }
        }

        /// <summary>
        /// where the solid of the monsters is from the draw top left
        /// </summary>
        public Int64 Solid_Y
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Solid_Y"];
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
        static public MonsterAnimation? Create(MonsterAnimationAttributes animationAttr)
        {
            string insertNewSolid = $"INSERT INTO Monster_Animations (Monster_Type_Id, Animation, Image_Path, X, Y, Height, Width, Draw_Height, Draw_Width, Solid_X, Solid_Y, Frames, Slowdown, After_Animation_Name, Star_Frame, Horizontal)" +
                $" VALUES($Monster_Type_Id, $Animation, $Image_Path, $X, $Y, $Height, $Width, $Draw_Height, $Draw_Width, $Solid_X, $Solid_Y, $Frames, $Slowdown, $After_Animation_Name, $Star_Frame, $Horizontal);";
            SQLiteCommand command = new SQLiteCommand(insertNewSolid, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$Monster_Type_Id", animationAttr.Monster_Type_Id);
            command.Parameters.AddWithValue("$Animation", Monster.AnimationEnumToName(animationAttr.Animation));
            command.Parameters.AddWithValue("$Image_Path", animationAttr.Image_Path);
            command.Parameters.AddWithValue("$X", animationAttr.X);
            command.Parameters.AddWithValue("$Y", animationAttr.Y);
            command.Parameters.AddWithValue("$Height", animationAttr.Height);
            command.Parameters.AddWithValue("$Width", animationAttr.Width);
            command.Parameters.AddWithValue("$Draw_Height", animationAttr.Draw_Height);
            command.Parameters.AddWithValue("$Draw_Width", animationAttr.Draw_Width);
            command.Parameters.AddWithValue("$Solid_X", animationAttr.Solid_X);
            command.Parameters.AddWithValue("$Solid_Y", animationAttr.Solid_Y);
            command.Parameters.AddWithValue("$Frames", animationAttr.Frames);
            command.Parameters.AddWithValue("$Slowdown", animationAttr.Slowdown);
            command.Parameters.AddWithValue("$After_Animation_Name", animationAttr.After_Animation_Name);
            command.Parameters.AddWithValue("$Star_Frame", animationAttr.Star_Frame);
            command.Parameters.AddWithValue("$Horizontal", animationAttr.Horizontal);
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
                height = Height,
                width = Width,
                drawHeight = DrawHeight,
                drawWidth = DrawWidth,
                solidX = Solid_X,
                solidY = Solid_Y,
                slowdown = Slowdown,
                after = AfterAnimationName,
                startFrame = StarFrame,
                horizontal = Horizontal,
            };
        }
    }

    public class MonsterAnimationAttributes
    {
        /// <summary>
        /// the monster type id that this animation is for.
        /// </summary>
        public long? Monster_Type_Id { get; set; }

        /// <summary>
        /// the animation name for this animation.
        /// each monster should have a full set of animations
        /// walkDown
        /// walkUp
        /// walkLeft
        /// walkRight
        /// standDown
        /// standUp
        /// standLeft
        /// standRight
        /// swingDown
        /// swingUp
        /// swingLeft
        /// swingRight
        /// castDown
        /// castUp
        /// castLeft
        /// castRight
        /// dieingDown
        /// </summary>
        public Monster.AnimationNames Animation { get; set; }

        /// <summary>
        /// string path to the image sheet for the animation.
        /// </summary>
        public string Image_Path { get; set; }

        /// <summary>
        /// x pixel position for the first frame of the animation.
        /// this is top left
        /// </summary>
        public long X { get; set; } = 0;

        /// <summary>
        /// the y pixel position for the first frame of the animation.
        /// this is top left.
        /// </summary>
        public long Y { get; set; } = 0;

        /// <summary>
        /// height of each frame in pixels.
        /// </summary>
        public long Height { get; set; } = 64;

        /// <summary>
        /// the width of each frame in pixels.
        /// </summary>
        public long Width { get; set; } = 64;

        /// <summary>
        /// the draw height for in game. 
        /// </summary>
        public long Draw_Height { get; set; } = 80;

        /// <summary>
        /// the draw width for in game.
        /// </summary>
        public long Draw_Width { get; set; } = 80;

        /// <summary>
        /// where the sold of the monster is from top left of draw
        /// </summary>
        public long Solid_X { get; set; } = 40;

        /// <summary>
        /// where the sold of the monster is from top left of draw
        /// </summary>
        public long Solid_Y { get; set; } = 65;

        /// <summary>
        /// number of frames for the animation.
        /// </summary>
        public long Frames { get; set; } = 1;

        /// <summary>
        /// this is the slow down for each frame.
        /// </summary>
        public long Slowdown { get; set; } = 10;

        /// <summary>
        /// name of animation to change to when the animation is finished.
        /// if this is set to "" the animation will repeat.
        /// </summary>
        public string After_Animation_Name { get; set; } = "";

        /// <summary>
        /// frame to start at default 0. -1 is random.
        /// </summary>
        public long Star_Frame { get; set; } = 0;

        /// <summary>
        /// set to true by default.
        /// step animation horizontally or vertical if false.
        /// </summary>
        public bool Horizontal { get; set; } = true;

        public MonsterAnimationAttributes()
        {

        }
    }
}
