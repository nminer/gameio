using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{
    public class GameSound
    {

        private object dbDataLock = new object();

        private SQLiteDataAdapter? adapter;

        private SQLiteCommandBuilder? builder;

        private DataSet? data;

        private DataRow? row;


        /// <summary>
        /// The sounds id in the database.
        /// </summary>
        public Int64 SoundId
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Sound_Id"];
                }
            }
        }
        
        public Int64 FullVolumeRadius
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Full_Volume_Radius"];
                }
            }
        }

        public Int64 FadeVolumeRadius
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Fade_Volume_Radius"];
                }
            }
        }

        public Int64 DelayMax
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Delay_Max"];
                }
            }
        }

        public Int64 DelayMin
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return 0;
                    }
                    return (Int64)row["Delay_Min"];
                }
            }
        }

        public bool Repeat
        {
            get
            {
               lock (dbDataLock)
                {
                    return (Int64)row["Repeat"] == 1;
                }
            }
        }

        public bool HasDelay
        {
            get
            {
                lock (dbDataLock)
                {
                    return (Int64)row["Has_Delay"] == 1;
                }
            }
        }

        /// <summary>
        /// Returns true if the min and max delay are the same.
        /// </summary>
        public bool RandomDelay
        {
            get
            { lock (dbDataLock) { return DelayMax == DelayMin; } }
        }

        public string SoundPath
        {
            get
            {
                lock (dbDataLock)
                {
                    if (data == null)
                    {
                        return "";
                    }
                    return (string)row["Sound_Path"];
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
        /// Load a sound from its sound id in the database.
        /// </summary>
        /// <param name="imageId"></param>
        public GameSound(Int64 soundId)
        {
            LoadSound(soundId);
        }

        private void LoadSound(Int64 soundId)
        {
            lock (dbDataLock)
            {
                adapter = new SQLiteDataAdapter();
                builder = new SQLiteCommandBuilder(adapter);
                data = new DataSet();
                string findUser = $"SELECT * FROM Sounds WHERE Sound_Id=$id;";
                SQLiteCommand command = new SQLiteCommand(findUser, DatabaseBuilder.Connection);
                command.Parameters.AddWithValue("$id", soundId);
                adapter.SelectCommand = command;
                adapter.Fill(data);
                row = data.Tables[0].Rows[0];
            }
        }

        static public GameSound? CreateNewSound(string soundName, string soundPath, bool repeat = true,
                                        Int64 minDelay = 0, Int64 maxDelay = 0, Int64 fadeRadius = 10, Int64 fullRadius = 10)
        {
            if (maxDelay < minDelay)
            {
                maxDelay = minDelay;
            }
            if (fadeRadius < fullRadius)
            {
                fadeRadius = fullRadius;
            }
            bool hasDelay = minDelay != 0 && maxDelay != 0;
            // insert new sound
            string insertNewSound = $"INSERT INTO Sounds (Sound_Path, Name, Repeat, Has_Delay, Delay_Min, Delay_Max, Full_Volume_Radius, Fade_Volume_Radius)" +
                $" VALUES($path, $name, $repeat, $hasDelay, $delayMin, $delayMax, $fullRadius, $fadeRadius);";
            SQLiteCommand command = new SQLiteCommand(insertNewSound, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$name", soundName);
            command.Parameters.AddWithValue("$path", soundPath);
            command.Parameters.AddWithValue("$repeat", repeat ? 1 : 0);
            command.Parameters.AddWithValue("$hasDelay", hasDelay ? 1 : 0);
            command.Parameters.AddWithValue("$delayMin", minDelay);
            command.Parameters.AddWithValue("$delayMax", maxDelay);
            command.Parameters.AddWithValue("$fullRadius", fullRadius);
            command.Parameters.AddWithValue("$fadeRadius", fadeRadius);
            SQLiteTransaction transaction = null;
            try
            {
                transaction = DatabaseBuilder.Connection.BeginTransaction();
                if (command.ExecuteNonQuery() > 0)
                {
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    return new GameSound(rowID);
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

        public object? GetJsonSoundObject(Point position)
        {
            return new {path = SoundPath, repeat = Repeat, x = position.X, y = position.Y, fullRadius = FullVolumeRadius, fadeRadius = FadeVolumeRadius};
        }
    }
}
