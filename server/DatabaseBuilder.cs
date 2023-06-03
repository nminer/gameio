using System.Data.SQLite;

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
	            'X_Coordinate'	REAL NOT NULL DEFAULT 0,
	            'Y_Coordinate'	REAL NOT NULL DEFAULT 0,
	            'Direction'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('User_Id' AUTOINCREMENT)
            );";
        SQLiteCommand cmd = Connection.CreateCommand();
        cmd.CommandText = createUserTable;
        cmd.ExecuteNonQuery();

    }
}

