using Microsoft.Data.Sqlite;

internal class DatabaseBuilder
{
    private string databaseFile = "Game.db";

    private SqliteConnection connection;

    public DatabaseBuilder()
    {
        bool needToBuild = !File.Exists(databaseFile);
        connection = new SqliteConnection($"Data Source={databaseFile}");
            connection.Open();
        if (needToBuild)
        {
            BuildTables();
        }
    }
    
    private void BuildTables()
    {
        string q = @"CREATE TABLE 'User' (
	            'UserId'	INTEGER NOT NULL UNIQUE,
	            'UserName'	TEXT NOT NULL,
	            'Health'	INTEGER NOT NULL DEFAULT 10,
	            'Stamana'	INTEGER NOT NULL DEFAULT 10,
	            'Strength'	INTEGER NOT NULL DEFAULT 10,
	            'Speed'	INTEGER NOT NULL DEFAULT 10,
	            PRIMARY KEY('UserId' AUTOINCREMENT)
            );";
        SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = q;
        cmd.ExecuteNonQuery();
    }
}

