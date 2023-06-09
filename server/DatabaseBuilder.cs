﻿using System.Data.SQLite;

internal class DatabaseBuilder
{
    public static readonly string DatabaseFile = "Game.db";

    public static readonly SQLiteConnection Connection = new($"Data Source={DatabaseFile}");

    public DatabaseBuilder()
    {
        bool needToBuild = !File.Exists(DatabaseFile);
            Connection.Open();
        if (needToBuild)
        {
            BuildTables();
        }
    }
    
    private void BuildTables()
    {
        // define the the User table
        string createUserTable = @"CREATE TABLE 'User' (
	            'User_Id'	INTEGER NOT NULL UNIQUE,
	            'UserName'	TEXT NOT NULL UNIQUE,
	            'PasswordHash'	TEXT NOT NULL,
	            'Experience'	INTEGER NOT NULL DEFAULT 0,
	            'Level'	INTEGER NOT NULL DEFAULT 1,
	            'Max_Health'	INTEGER NOT NULL DEFAULT 10,
	            'Health'	INTEGER NOT NULL DEFAULT 10,
	            'Max_Stamana'	INTEGER NOT NULL DEFAULT 10,
	            'Stamana'	INTEGER NOT NULL DEFAULT 10,
	            'Strength'	INTEGER NOT NULL DEFAULT 10,
	            'Speed'	INTEGER NOT NULL DEFAULT 10,
	            'Map_id'	INTEGER NOT NULL DEFAULT 1,
	            'X_Coordinate'	REAL NOT NULL DEFAULT 60,
	            'Y_Coordinate'	REAL NOT NULL DEFAULT 60,
	            'Direction'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('User_Id' AUTOINCREMENT)
            );";
        SQLiteCommand cmdUser = Connection.CreateCommand();
        cmdUser.CommandText = createUserTable;
        cmdUser.ExecuteNonQuery();

        // define the the User table
        string createAvatarTable = @"CREATE TABLE 'Avatar' (
	            'Avatar_Id'	INTEGER NOT NULL UNIQUE,
	            'User_Id'	INTEGER NOT NULL UNIQUE,
	            'Body_Style'	INTEGER NOT NULL DEFAULT 1,
	            'Body_Color'	INTEGER NOT NULL DEFAULT 1,
	            'Nose_Style'	INTEGER NOT NULL DEFAULT 0,
	            'Ear_Style'   	INTEGER NOT NULL DEFAULT 0,
				'Wrinkles'      INTEGER NOT NULL DEFAULT 0,
	            'Hair_Style'	INTEGER NOT NULL DEFAULT 0,
	            'Hair_Color'	INTEGER NOT NULL DEFAULT 0,
	            'Beard_style'	INTEGER NOT NULL DEFAULT 0,
	            'Beard_Color'	INTEGER NOT NULL DEFAULT 0,
	            'Eye_Color'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('Avatar_Id' AUTOINCREMENT)
            );";
        SQLiteCommand cmdAvatar = Connection.CreateCommand();
        cmdAvatar.CommandText = createAvatarTable;
        cmdAvatar.ExecuteNonQuery();

        // define maps table 
        string createMapsTable = @"CREATE TABLE 'Maps' (
	            'Map_Id'	INTEGER NOT NULL UNIQUE,
	            'MapName'	TEXT NOT NULL UNIQUE,
	            'ImagePath'	TEXT NOT NULL,
	            'Height'	INTEGER NOT NULL DEFAULT 0,
	            'Width'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('Map_Id' AUTOINCREMENT)
            );";
        SQLiteCommand cmdMap = Connection.CreateCommand();
        cmdMap.CommandText = createMapsTable;
        cmdMap.ExecuteNonQuery();
    }
}

