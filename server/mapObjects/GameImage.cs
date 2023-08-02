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
    class GameImage
    {

        private object dbDataLock = new object();

        private SQLiteDataAdapter? adapter;

        private SQLiteCommandBuilder? builder;

        private DataSet? data;

        private DataRow? row;


        /// <summary>
        /// The image id in the database.
        /// </summary>
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
                    return (string)row["ImagePath"];
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

        /// <summary>
        /// Load a map image from its image id in the database.
        /// </summary>
        /// <param name="imageId"></param>
        public GameImage(Int64 imageId)
        {
            LoaderImage(imageId);
        }

        private void LoaderImage(Int64 imageId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findUser = $"SELECT * FROM Images WHERE Image_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findUser, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", imageId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                row = data.Tables[0].Rows[0];
            }
        }

        static public GameImage? CreateNewImage(string imageName, string imagePath)
        {
            System.Drawing.Image img;
            try
            {
                img = System.Drawing.Image.FromFile($"html/{imagePath}");
            }
            catch (Exception)
            {
                return null;
            }

            Int64 width = img.Width;
            Int64 height = img.Height;
            // insert new image
            string insertNewMap = $"INSERT INTO Images (ImagePath, Name, Height, Width) VALUES($path, $name, $height, $width);";
            SQLiteCommand command = new SQLiteCommand(insertNewMap, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$name", imageName);
            command.Parameters.AddWithValue("$path", imagePath);
            command.Parameters.AddWithValue("$height", height);
            command.Parameters.AddWithValue("$width", width);
            SQLiteTransaction transaction = null;
            try
            {
                transaction = DatabaseBuilder.Connection.BeginTransaction();
                if (command.ExecuteNonQuery() > 0)
                {
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    return new GameImage(rowID);
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

        public object? GetJsonImageObject(Point position, double drawOrder)
        {
            //string img = JsonConvert.SerializeObject(new { path = ImagePath, width = Width, height = Height, x = position.X, y = position.Y, drawOrder = drawOrder });
            return new { path = ImagePath, width = Width, height = Height, x = position.X, y = position.Y, drawOrder = drawOrder };
        }

    }
}
