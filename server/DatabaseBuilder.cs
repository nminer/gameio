using System.CodeDom;
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
	            'X_Coordinate'	REAL NOT NULL DEFAULT 60,
	            'Y_Coordinate'	REAL NOT NULL DEFAULT 60,
	            'Direction'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('User_Id' AUTOINCREMENT)
            );";
		Excute(createUserTable, Connection);

        // define the User avatar table
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
        Excute(createAvatarTable, Connection);

        // define maps table 
        string createMapsTable = @"CREATE TABLE 'Maps' (
	            'Map_Id'	INTEGER NOT NULL UNIQUE,
	            'MapName'	TEXT NOT NULL UNIQUE,
	            'ImagePath'	TEXT NOT NULL,
	            'Height'	INTEGER NOT NULL DEFAULT 0,
	            'Width'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('Map_Id' AUTOINCREMENT)
            );";
        Excute(createMapsTable, Connection);

        // define portals table 
        string createPortalsTable = @"CREATE TABLE 'Portals' (
	            'Portal_Id'	INTEGER NOT NULL UNIQUE,
	            'Map_Id'	INTEGER NOT NULL,
	            'X_Coordinate'	REAL NOT NULL DEFAULT 0,
	            'Y_Coordinate'	REAL NOT NULL DEFAULT 0,
	            'Target_Map_Id'	INTEGER NOT NULL,
	            'Target_X'	REAL NOT NULL DEFAULT 0,
	            'Target_Y'	REAL NOT NULL DEFAULT 0,
	            'PortalName'	TEXT NOT NULL UNIQUE,
	            PRIMARY KEY('Portal_Id' AUTOINCREMENT)
            );";
        Excute(createPortalsTable, Connection);

        // define portals table 
        string createImagessTable = @"CREATE TABLE 'Images' (
	            'Image_Id'	INTEGER NOT NULL UNIQUE,
	            'ImagePath'	TEXT NOT NULL,
	            'Name'	TEXT NOT NULL,
	            'Height'	INTEGER NOT NULL DEFAULT 0,
	            'Width'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('Image_Id' AUTOINCREMENT)
            );";
		Excute(createImagessTable, Connection);

		// define animations table 
        string createAnimationsTable = @"CREATE TABLE 'Animations' (
	            'Animation_Id'	INTEGER NOT NULL UNIQUE,
	            'Name'	TEXT NOT NULL,
				'Image_Id'	INTEGER NOT NULL DEFAULT 0,
	            'Start_Frame_X'	INTEGER NOT NULL DEFAULT 0,
	            'Start_Frame_Y'	INTEGER NOT NULL DEFAULT 0,
	            'Frame_Height'	INTEGER NOT NULL DEFAULT 0,
	            'Frame_Width'	INTEGER NOT NULL DEFAULT 0,
	            'Frame_Count'	INTEGER NOT NULL DEFAULT 0,
	            'Frame_Slowdown'	INTEGER NOT NULL DEFAULT 5,
				'Start_Frame'	INTEGER NOT NULL DEFAULT 0,
				'Random_Start_Frame'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('Animation_Id' AUTOINCREMENT)
            );";
        Excute(createAnimationsTable, Connection);

        // define visuals table 
        string createVisualsTable = @"CREATE TABLE 'Map_Visuals' (
	            'Map_Visual_Id'	INTEGER NOT NULL UNIQUE,
	            'Name'	TEXT NOT NULL,
				'Image_Id'	INTEGER NOT NULL DEFAULT 0,
				'Animation_Id'	INTEGER NOT NULL DEFAULT 0,
				'Map_Id'	INTEGER NOT NULL DEFAULT 0,
	            'Map_X'	INTEGER NOT NULL DEFAULT 0,
	            'Map_Y'	INTEGER NOT NULL DEFAULT 0,
				'Draw_Order_Y'	INTEGER NOT NULL DEFAULT 0,
				'Draw_Order_X'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('Map_Visual_Id' AUTOINCREMENT)
            );";
        Excute(createVisualsTable, Connection);

		// define shapes
        string createShapesTable = @"CREATE TABLE 'Shapes' (
	            'Shape_Id'	INTEGER NOT NULL UNIQUE,
	            'Description'	TEXT NOT NULL,
				'Json_Points'	TEXT NOT NULL,
				'Is_Closed_Shape'	INTEGER NOT NULL DEFAULT 1,
	            PRIMARY KEY('Shape_Id' AUTOINCREMENT)
            );";
        Excute(createShapesTable, Connection);

        // define shapes
        string createSolidTable = @"CREATE TABLE 'Solids' (
	            'Solid_Id'	INTEGER NOT NULL UNIQUE,
	            'Description'	TEXT NOT NULL,
				'Image_Id'	INTEGER NOT NULL DEFAULT 0,
				'Animation_Id'	INTEGER NOT NULL DEFAULT 0,
				'Shape_Id'	INTEGER NOT NULL DEFAULT 0,
				'Shape_Offset_X'	INTEGER NOT NULL DEFAULT 0,
				'Shape_Offset_Y'	INTEGER NOT NULL DEFAULT 0,
				'Draw_Order_Y'	INTEGER NOT NULL DEFAULT 0,
				'Draw_Order_X'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('Solid_Id' AUTOINCREMENT)
            );";
        Excute(createSolidTable, Connection);

        // define shapes
        string createMapSolidTable = @"CREATE TABLE 'Map_Solids' (
	            'Map_Solid_Id'	INTEGER NOT NULL UNIQUE,
	            'Description'	TEXT NOT NULL,
				'Solid_Id'	INTEGER NOT NULL DEFAULT 0,
				'Shape_Id'	INTEGER NOT NULL DEFAULT 0,
				'Map_Id'	INTEGER NOT NULL DEFAULT 0,
	            'Map_X'	INTEGER NOT NULL DEFAULT 0,
	            'Map_Y'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('Map_Solid_Id' AUTOINCREMENT)
            );";
        Excute(createMapSolidTable, Connection);
    }

	/// <summary>
	/// execute a non query, query string with the passed in connection.
	/// </summary>
	/// <param name="query"></param>
	/// <param name="Connection"></param>
	private void Excute(string query, SQLiteConnection Connection)
	{
        SQLiteCommand cmd = Connection.CreateCommand();
        cmd.CommandText = query;
        cmd.ExecuteNonQuery();
    }
}

