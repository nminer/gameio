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
	            'Max_Health'	INTEGER NOT NULL DEFAULT 100,
	            'Health'	INTEGER NOT NULL DEFAULT 100,
	            'Max_Stamina'	INTEGER NOT NULL DEFAULT 100,
	            'Stamina'	INTEGER NOT NULL DEFAULT 100,
	            'Max_Mana'	INTEGER NOT NULL DEFAULT 100,
	            'Mana'	INTEGER NOT NULL DEFAULT 100,
	            'Strength'	INTEGER NOT NULL DEFAULT 10,
	            'Speed'	INTEGER NOT NULL DEFAULT 10,
	            'Wisdom'	INTEGER NOT NULL DEFAULT 10,
	            'Map_id'	INTEGER NOT NULL DEFAULT 1,
	            'X_Coordinate'	REAL NOT NULL DEFAULT 60,
	            'Y_Coordinate'	REAL NOT NULL DEFAULT 60,
	            'Direction'	INTEGER NOT NULL DEFAULT 0,
	            'Deaths'	INTEGER NOT NULL DEFAULT 0,
	            'Spawn_Map_Id'	INTEGER NOT NULL DEFAULT 1,
	            'Spawn_X'	REAL NOT NULL DEFAULT 60,
	            'Spawn_Y'	REAL NOT NULL DEFAULT 60,
	            'Death_Points'	REAL NOT NULL DEFAULT 0,
	            PRIMARY KEY('User_Id' AUTOINCREMENT)
            );";
		Execute(createUserTable, Connection);

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
        Execute(createAvatarTable, Connection);

        // define maps table 
        string createMapsTable = @"CREATE TABLE 'Maps' (
	            'Map_Id'	INTEGER NOT NULL UNIQUE,
	            'MapName'	TEXT NOT NULL UNIQUE,
	            'ImagePath'	TEXT NOT NULL,
	            'Height'	INTEGER NOT NULL DEFAULT 0,
	            'Width'	INTEGER NOT NULL DEFAULT 0,
	            'Outside'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('Map_Id' AUTOINCREMENT)
            );";
        Execute(createMapsTable, Connection);

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
        Execute(createPortalsTable, Connection);

        // define portals table 
        string createImagessTable = @"CREATE TABLE 'Images' (
	            'Image_Id'	INTEGER NOT NULL UNIQUE,
	            'Image_Path'	TEXT NOT NULL,
	            'Name'	TEXT NOT NULL,
	            'Height'	INTEGER NOT NULL DEFAULT 0,
	            'Width'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('Image_Id' AUTOINCREMENT)
            );";
		Execute(createImagessTable, Connection);

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
				'Step_Horizontal'	INTEGER NOT NULL DEFAULT 1,
				'Start_Frame'	INTEGER NOT NULL DEFAULT 0,
				'Random_Start_Frame'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('Animation_Id' AUTOINCREMENT)
            );";
        Execute(createAnimationsTable, Connection);

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
        Execute(createVisualsTable, Connection);

		// define shapes
        string createShapesTable = @"CREATE TABLE 'Shapes' (
	            'Shape_Id'	INTEGER NOT NULL UNIQUE,
	            'Description'	TEXT NOT NULL,
				'Json_Points'	TEXT NOT NULL,
				'Is_Closed_Shape'	INTEGER NOT NULL DEFAULT 1,
				'Solid_Inside'      INTEGER NOT NULL DEFAULT 1,
	            PRIMARY KEY('Shape_Id' AUTOINCREMENT)
            );";
        Execute(createShapesTable, Connection);

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
        Execute(createSolidTable, Connection);

        // define map solids
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
        Execute(createMapSolidTable, Connection);

		// define sounds
        string createSoundsTable = @"CREATE TABLE 'Sounds' (
	            'sound_Id'	INTEGER NOT NULL UNIQUE,
				'Sound_Path'	TEXT NOT NULL,
	            'Name'	TEXT NOT NULL,
				'Repeat'	INTEGER NOT NULL DEFAULT 0,
				'Has_Delay'	INTEGER NOT NULL DEFAULT 0,
				'Delay_Min'	INTEGER NOT NULL DEFAULT 0,
				'Delay_Max'	INTEGER NOT NULL DEFAULT 0,
				'Full_Volume_Radius'	INTEGER NOT NULL DEFAULT 0,
				'Fade_Volume_Radius'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('sound_Id' AUTOINCREMENT)
            );";
        Execute(createSoundsTable, Connection);

        // define map sounds
        string createMapSoundsTable = @"CREATE TABLE 'Map_Sounds' (
	            'Map_Sound_Id'	INTEGER NOT NULL UNIQUE,
	            'Description'	TEXT NOT NULL,
				'Sound_Id'	INTEGER NOT NULL DEFAULT 0,
				'Map_Id'	INTEGER NOT NULL DEFAULT 0,
	            'Map_X'	INTEGER NOT NULL DEFAULT 0,
	            'Map_Y'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('Map_Sound_Id' AUTOINCREMENT)
            );";
        Execute(createMapSoundsTable, Connection);

        // define map light source -1 amount will be set as out side light and not always on.
		// main color and midcolor should be hex colors - '#ffffff'
        string createMapLightsTable = @"CREATE TABLE 'Map_Lights' (
	            'Map_Light_Id'	INTEGER NOT NULL UNIQUE,
	            'Description'	TEXT NOT NULL,
	            'Main_Color'	TEXT NOT NULL,
	            'Mid_Color'	TEXT NOT NULL,
				'Radius'	INTEGER NOT NULL DEFAULT 0,
				'Amount'	REAL NOT NULL DEFAULT -1,
				'Map_Id'	INTEGER NOT NULL DEFAULT 0,
	            'Map_X'	INTEGER NOT NULL DEFAULT 0,
	            'Map_Y'	INTEGER NOT NULL DEFAULT 0,
	            PRIMARY KEY('Map_Light_Id' AUTOINCREMENT)
            );";
        Execute(createMapLightsTable, Connection);

		// define monster types table 
		string createSoulStoneTable = @"CREATE TABLE 'Soul_Stones' (
					'Soul_Stone_Id'	INTEGER NOT NULL UNIQUE,
					'Map_Id'	INTEGER NOT NULL,
					'Map_X'	INTEGER NOT NULL DEFAULT 0,
					'Map_Y'	INTEGER NOT NULL DEFAULT 0,
					'Radius'	INTEGER NOT NULL DEFAULT 0,
					'Name'	TEXT NOT NULL UNIQUE,
					PRIMARY KEY('Soul_Stone_Id' AUTOINCREMENT)
				);";
		Execute(createSoulStoneTable, Connection);


        // define monster animations table 
        string createMonsterTypeTable = @"CREATE TABLE 'Monster_Types' (
					'Monster_Type_Id'	INTEGER NOT NULL UNIQUE,
					'Type'	TEXT NOT NULL,					
					'Description'	TEXT NOT NULL,
					PRIMARY KEY('Monster_Type_Id' AUTOINCREMENT)
				);";
        Execute(createMonsterTypeTable, Connection);

        // define monster animation table 
        string createMonsterAnimatinoTable = @"CREATE TABLE 'Monster_Animations' (
					'Monster_Animation_Id' INTEGER NOT NULL UNIQUE,
					'Monster_Type_Id' INTEGER NOT NULL,
					'Animation'	TEXT NOT NULL,
					'Image_Path' TEXT NOT NULL,
					'X' INTEGER NOT NULL DEFAULT 0,
					'Y' INTEGER NOT NULL DEFAULT 0,
					'Height' INTEGER NOT NULL DEFAULT 0,
					'Width'	INTEGER NOT NULL DEFAULT 0,
					'Draw_Height' INTEGER NOT NULL DEFAULT 0,
					'Draw_Width'	INTEGER NOT NULL DEFAULT 0,
					'Frames'	INTEGER NOT NULL DEFAULT 0,
					'Slowdown'	INTEGER NOT NULL DEFAULT 0,
					'After_Animation_Name'	TEXT NOT NULL DEFAULT '',
					'Star_Frame'	INTEGER NOT NULL DEFAULT 0,
					'Horizontal'	INTEGER NOT NULL DEFAULT 1,
					PRIMARY KEY('Monster_Animation_Id' AUTOINCREMENT)
				);";
        Execute(createMonsterAnimatinoTable, Connection);

        // define monster animation table 
        string createMonsterTable = @"CREATE TABLE 'Monsters' (
					'Monster_Id' INTEGER NOT NULL UNIQUE,
					'Monster_Type_Id' INTEGER NOT NULL,
					'Name'	TEXT NOT NULL,
					'Level'	INTEGER NOT NULL DEFAULT 1,
					'Health'	INTEGER NOT NULL DEFAULT 100,
					'Stamina'	INTEGER NOT NULL DEFAULT 100,
					'Mana'	INTEGER NOT NULL DEFAULT 100,
					'Strength'	INTEGER NOT NULL DEFAULT 10,
					'Speed'	INTEGER NOT NULL DEFAULT 10,
					'Wisdom'	INTEGER NOT NULL DEFAULT 10,
					'Aggressive_Distance' INTEGER NOT NULL DEFAULT 100,
					'Chase_Distance' INTEGER NOT NULL DEFAULT 100,
					'Min_Damage' INTEGER NOT NULL DEFAULT 1,
					'Max_Damage' INTEGER NOT NULL DEFAULT 10,
					PRIMARY KEY('Monster_Id' AUTOINCREMENT)
				);";
        Execute(createMonsterTable, Connection);

        // define monster spawn table 
        string createMonsterSpawnTable = @"CREATE TABLE 'Monster_Spawns' (
					'Monster_Spawn_Id' INTEGER NOT NULL UNIQUE,
					'Map_Id'	INTEGER NOT NULL DEFAULT 0,
					'Map_X'	INTEGER NOT NULL DEFAULT 0,
					'Map_Y'	INTEGER NOT NULL DEFAULT 0,
					'Spawn_Distance' INTEGER NOT NULL DEFAULT 0,
					'Monster_Count'	INTEGER NOT NULL DEFAULT 1,
					'Spawn_Timer' INTEGER NOT NULL DEFAULT 1000,
					'Wander_Distance' INTEGER NOT NULL DEFAULT 100,
					PRIMARY KEY('Monster_Spawn_Id' AUTOINCREMENT)
				);";
        Execute(createMonsterSpawnTable, Connection);

        // define monster spawn link table 
        string createMonsterSpawnLinkTable = @"CREATE TABLE 'Monster_Spawn_Links' (
					'Monster_Spawn_Link_Id'	INTEGER NOT NULL UNIQUE,
					'Monster_Spawn_Id'	INTEGER NOT NULL DEFAULT 0,				
					'Monster_Id'	INTEGER NOT NULL DEFAULT 0,
					'Monster_Spawn_Weight'	INTEGER NOT NULL DEFAULT 1,
					PRIMARY KEY('Monster_Spawn_Link_Id' AUTOINCREMENT)
				);";
        Execute(createMonsterSpawnLinkTable, Connection);
    }

    


    /// <summary>
    /// execute a non query, query string with the passed in connection.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="Connection"></param>
    private void Execute(string query, SQLiteConnection Connection)
	{
        SQLiteCommand cmd = Connection.CreateCommand();
        cmd.CommandText = query;
        cmd.ExecuteNonQuery();
    }
}

