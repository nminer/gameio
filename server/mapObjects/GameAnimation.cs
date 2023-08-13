using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Transactions;
using System.Data.Common;
using System.Drawing;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace server.mapObjects
{
    class GameAnimation
    {

        private object dbDataLock = new object();

        private SQLiteDataAdapter? adapter;

        private SQLiteCommandBuilder? builder;

        private DataSet? data;

        private DataRow? row;


        /// <summary>
        /// The Animation id in the database.
        /// </summary>
        public Int64 AnimationId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Animation_Id"];
                }
            }
        }

        public Int64 FrameHeight
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Frame_Height"];
                }
            }
        }

        public Int64 FrameWidth
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Frame_Width"];
                }
            }
        }

        public Int64 ImageId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Image_Id"];
                }
            }
        }

        public string Name
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return "";
                    }
                    return (string)row["Name"];
                }
            }
        }

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
                    return (Int64)row["Frame_Slowdown"];
                }
            }
        }

        public bool RandomStartFrame
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return false;
                    }
                    return (Int64)row["Random_Start_Frame"] == 1;
                }
            }
        }

        public bool StepHorizontal
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return false;
                    }
                    return (Int64)row["Step_Horizontal"] == 1;
                }
            }
        }
        
        public Int64 StartFrame
        {
            get
            {
                lock (dbDataLock)
                {
                    if (RandomStartFrame)
                    {
                        Random rnd = new Random();
                        return rnd.Next((int)(FrameCount - 1));
                    }
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Start_Frame"];
                }
            }
        }

        public Int64 FrameCount
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Frame_Count"];
                }
            }
        }

        private GameImage image;

        private Point firstFramePosition;

        /// <summary>
        /// Load a map image from its image id in the database.
        /// </summary>
        /// <param name="imageId"></param>
        public GameAnimation(Int64 animationId)
        {
            LoadAnimation(animationId);
        }

        private void LoadAnimation(Int64 animationId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findAnimation = $"SELECT * FROM Animations WHERE Animation_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findAnimation, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", animationId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                row = data.Tables[0].Rows[0];
                image = new GameImage(ImageId);
                firstFramePosition = new Point((Int64)row["Start_Frame_X"], (Int64)row["Start_Frame_Y"]);
            }
        }

        static public GameAnimation? CreateNewAnimation(string name, GameImage gameImage,
            long frameWidth, long frameHeight,
            long numberOfFrames, long slowdown, Point firstFrameTopLeft, bool stepHorizontal, long startFrame,
            bool randomStartFrame)
        {
            string insertNewMap = $"INSERT INTO Animations (Name, Image_Id, Start_Frame_X, " +
                $"Start_Frame_Y, Frame_Height, Frame_Width, Frame_Count, Frame_Slowdown, Step_Horizontal, Start_Frame, Random_Start_Frame)" +
                $" VALUES($name, $imgId, $frameX, $frameY, $frameHeight, $frameWidth, $frameCount, $frameSlowdown, $StepHorizontal, $StartFrame, $random);";
            SQLiteCommand command = new SQLiteCommand(insertNewMap, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$imgId", gameImage.ImageId);
            command.Parameters.AddWithValue("$frameX", firstFrameTopLeft.X);
            command.Parameters.AddWithValue("$frameY", firstFrameTopLeft.Y);
            command.Parameters.AddWithValue("$frameHeight", frameHeight);
            command.Parameters.AddWithValue("$frameWidth", frameWidth);
            command.Parameters.AddWithValue("$frameCount", numberOfFrames);
            command.Parameters.AddWithValue("$frameSlowdown", slowdown);
            command.Parameters.AddWithValue("$StepHorizontal", stepHorizontal ? 1 : 0);
            command.Parameters.AddWithValue("$startFrame", startFrame);
            command.Parameters.AddWithValue("$random", randomStartFrame ? 1 : 0);
            SQLiteTransaction transaction = null;
            try
            {
                transaction = DatabaseBuilder.Connection.BeginTransaction();
                if (command.ExecuteNonQuery() > 0)
                {
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    return new GameAnimation(rowID);
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

        public object? GetJsonAnimationObject(Point position, double drawOrder)
        {
            return new { path = image.ImagePath, width = FrameWidth, height = FrameHeight, frameX = firstFramePosition.X, frameY = firstFramePosition.Y, frameCount = FrameCount, horizontal = StepHorizontal, firstFrame = StartFrame, x = position.X, y = position.Y, slowDown= Slowdown, drawOrder = drawOrder };
        }

    }
}
