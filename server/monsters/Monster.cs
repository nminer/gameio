using server.mapObjects;
using System.Data.SQLite;
using System.Data;


namespace server.monsters
{
    public class Monster : ICreature
    {
        private const double MIN_SPEED = 0.0;
        private const double MAX_SPEED = 100.0;

        public MonsterSpawn? Spawn = null;

        private MonsterCounters counters;

        /// <summary>
        /// set to the user that the monster is targeting if any.
        /// </summary>
        private User? target;

        /// <summary>
        /// set to how long to target current target then recalculate a new target.
        /// the more aggressiveDistance and chaseDistance the longer the target time.
        /// </summary>
        private DateTime? targetTimer;

        private MonsterType type;
        public MonsterType MonsterType { get { return type; } }

        private object damageTakenLock = new object();
        /// <summary>
        /// user and amount of damage dealt to the monster
        /// </summary>
        private Dictionary<User, long> damageTaken = new Dictionary<User, long>();

        private Point nextMoveAmount = new Point();

        public Point Home = new Point(0,0);

        public Double Wander = 0;

        private double hitDistance = 10;

        private string name;
        public string Name
        {
            get
            {
                return name;
            }
        }

        private long level;

        private object healthLock = new object();
        private long maxHealth;

        private long health;

        /// <summary>
        /// the current health of the monster.
        /// </summary>
        public long Health
        {
            get
            {
                lock (healthLock)
                {
                    return health;
                }
            }
            set
            {
                lock (healthLock)
                {
                    if (value >= maxHealth)
                    {
                        health = maxHealth;
                    }
                    else if (value <= 0)
                    {
                        SetCoolDown(140);
                        currentAnimation = "dieingDown";
                        health = 0;
                    }
                    else
                    {
                        health = value;
                    }
                }
            }
        }

        private object staminaLock = new object();
        private long maxStamina;

        private long myStamina;

        /// <summary>
        /// used to set the current animation direction.
        /// </summary>
        private string directionString = "Down";

        /// <summary>
        /// used to set the current animation action.
        /// </summary>
        private string actionString = "stand";

        /// <summary>
        /// the current animation.
        /// </summary>
        private string currentAnimation = "spawn";

        /// <summary>
        /// the monster current Stamina.
        /// </summary>
        public long Stamina
        {
            get
            {
                lock (staminaLock)
                {
                    if (myStamina >= maxStamina)
                    {
                        myStamina = maxStamina;
                    }
                    return myStamina;
                }
            }
            set
            {
                lock (staminaLock)
                {
                    if (value >= maxStamina)
                    {
                        myStamina = maxStamina;
                    }
                    else if (value < 0)
                    {
                        myStamina = 0;
                    }
                    else
                    {
                        myStamina = value;
                    }
                }
            }
        }

        private object manaLock = new object();
        private long maxMana;

        private long mana;
        /// <summary>
        /// the current mana of the player.
        /// </summary>
        public long Mana
        {
            get
            {
                lock (manaLock)
                {
                    return mana;
                }
            }
            set
            {
                lock (manaLock)
                {
                    if (value >= maxMana)
                    {
                        mana = maxMana;
                    }
                    else if (value <= 0)
                    {
                        mana = 0;
                    }
                    else
                    {
                        mana = value;
                    }
                }
            }
        }

        private long strength;

        private long speed;

        public long Speed
        {
            get
            {
                return speed;
            }
        }

        private long wisdom;

        private long aggressiveDistance;

        private long chaseDistance;

        private long minDamage;

        private long maxDamage;

        public Point MapPosition = new Point(0, 0);

        public Circle mySolid = new Circle(new Point(0, 0), 15);

        public Circle Solid
        {
            get
            {
                return mySolid;
            }
        }        

        private object coolDownLock = new object();
        private Double CoolDown = 0.0;
        private Double CoolDownCount = 0.0;

        public bool HasCoolDown
        {
            get
            {
                lock (coolDownLock)
                {
                    return CoolDown > 0 && CoolDownCount >= 0;
                }
            }
        }

        public void SetCoolDown(double cooldown)
        {
            lock (coolDownLock)
            {
                CoolDown = cooldown;
                CoolDownCount = cooldown;
            }
        }

        public void decCoolDown(double amount = 1.0)
        {
            lock (coolDownLock)
            {
                CoolDownCount -= amount;
                if (CoolDownCount < 0)
                {
                    CoolDown = 0;
                    CoolDownCount = 0;
                }
            }
        }

        /// <summary>
        /// the x coord on the current map. this is from the top left of the map.
        /// </summary>
        public Double X_Coord
        {
            get
            {
                return MapPosition.X;
            }
            set
            {
                MapPosition.X = value;
                mySolid.Center.X = value;
            }
        }

        /// <summary>
        /// the y coord on the current map. this is from the top left of the map.
        /// </summary>
        public Double Y_Coord
        {
            get
            {
                return MapPosition.Y;
            }
            set
            {
                MapPosition.Y = value;
                mySolid.Center.Y = value;
            }
        }


        private long databaseId;
        public long DatabaseId
        {
            get
            {
                return databaseId;
            }
        }
        
        private string id;
        /// <summary>
        /// The monster animation Id in the database.
        /// </summary>
        public string MonsterId
        {
            get
            {
                return id;
            }
        }

        /// <summary>
        /// calculate from animation and speed. TODO
        /// </summary>
        public Int64 Slowdown
        {
            get
            {
                 return 10;
            }
        }

        public Int64 SpeedMoveMod
        {
            get
            {
                if (myStamina > 0)
                {
                    return speed;
                }
                return speed / 4;
            }
        }

        public Monster(long monsterId, Point? location = null, double wander = 0)
        {
            LoadFromId(monsterId);
            if (location is null)
            {
                setLocation(new Point(0, 0));
                Home = new Point(0, 0);
            } else
            {
                setLocation(location);
                Home = new Point(location.X, location.Y);
            }
            id = Mods.RandomKey();
            Wander = wander;
            counters = new MonsterCounters(this);
        }

        public void setLocation(Point location)
        {
            MapPosition.X = location.X; MapPosition.Y = location.Y;
            //MapPosition = new Point(location.X, location.Y);
            mySolid = new Circle(MapPosition, type.Solid_Radius);
        }

        private void LoadFromId(long monsterId)
        {
            databaseId = monsterId;
            SQLiteDataAdapter adapter = new SQLiteDataAdapter();
            SQLiteCommandBuilder builder = new SQLiteCommandBuilder(adapter);
            DataSet data = new DataSet();
            string findShape = $"SELECT * FROM Monsters WHERE Monster_Id=$id;";
            SQLiteCommand command = new SQLiteCommand(findShape, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$id", monsterId);
            adapter.SelectCommand = command;
            adapter.Fill(data);
            DataRow row = data.Tables[0].Rows[0];
            loadMonsterType((Int64)row["Monster_Type_Id"]);
            name = (string)row["name"];
            level = (Int64)row["Level"];
            maxHealth = (Int64)row["Health"];
            health = (Int64)row["Health"];
            maxStamina = (Int64)row["Stamina"];
            myStamina = (Int64)row["Stamina"];
            maxMana = (Int64)row["Mana"];
            mana = (Int64)row["Mana"];
            strength = (Int64)row["Strength"];
            speed = (Int64)row["Speed"];
            wisdom = (Int64)row["Wisdom"];
            aggressiveDistance = (Int64)row["Aggressive_Distance"];
            chaseDistance = (Int64)row["Chase_Distance"];
            minDamage = (Int64)row["Min_Damage"];
            maxDamage = (Int64)row["Max_Damage"];
        }

        private void loadMonsterType(long typeId)
        {
            type = new MonsterType(typeId);
        }

        /// <summary>
        /// set the solids position on the map. 
        /// this is default 0 0 if left null.
        /// </summary>
        /// <param name="shapePosition"></param>
        static public Monster? Create(MonsterAttributes attr)
        {
            string insertNewSolid = $"INSERT INTO Monsters (Monster_Type_Id, Name, Level, Health, Stamina, Mana, Strength, Speed, Wisdom, Aggressive_Distance, Chase_Distance, Min_Damage, Max_Damage)" +
                $" VALUES($Monster_Type_Id, $Name, $Level, $Health, $Stamina, $Mana, $Strength, $Speed, $Wisdom, $Aggressive_Distance, $Chase_Distance, $Min_Damage, $Max_Damage);";
            SQLiteCommand command = new SQLiteCommand(insertNewSolid, DatabaseBuilder.Connection);
            command.Parameters.AddWithValue("$Monster_Type_Id", attr.Monster_Type_Id);
            command.Parameters.AddWithValue("$Name", attr.Name);
            command.Parameters.AddWithValue("$Level", attr.Level);
            command.Parameters.AddWithValue("$Health", attr.Health);
            command.Parameters.AddWithValue("$Stamina", attr.Stamina);
            command.Parameters.AddWithValue("$Mana", attr.Mana);
            command.Parameters.AddWithValue("$Strength", attr.Strength);
            command.Parameters.AddWithValue("$Speed", attr.Speed);
            command.Parameters.AddWithValue("$Wisdom", attr.Wisdom);
            command.Parameters.AddWithValue("$Aggressive_Distance", attr.Aggressive_Distance);
            command.Parameters.AddWithValue("$Chase_Distance", attr.Chase_Distance);
            command.Parameters.AddWithValue("$Min_Damage", attr.Min_Damage);
            command.Parameters.AddWithValue("$Max_Damage", attr.Max_Damage);
            SQLiteTransaction transaction = null;
            try
            {
                transaction = DatabaseBuilder.Connection.BeginTransaction();
                if (command.ExecuteNonQuery() > 0)
                {
                    long rowID = DatabaseBuilder.Connection.LastInsertRowId;
                    transaction.Commit();
                    return new Monster(rowID);
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


        public object? GetJsonMonsterOject()
        {
            return new
            {
                id = MonsterId,
                type = type.MonsterTypeId,
                name = name,
                slowdown = Slowdown,
                x = MapPosition.X,
                y = MapPosition.Y,
                animation = currentAnimation,
                health = Health,
                maxHealth = maxHealth
            };
        }

        /// <summary>
        /// calculate target
        /// hit
        /// and movement.
        /// </summary>
        /// <param name="map"></param>
        public void calculateNextMove(Map map)
        {
            Point moveAmount = new Point();
            
            // if we have a target move to hit it.
            // if no target and out of wonder range. go home. if closer to home then move then set to home.
            // no target and in wonder range random wonder
            nextMoveAmount.X = 0;
            nextMoveAmount.Y = 0;
            if (HasCoolDown)
            {
                decCoolDown();
                counters.ResetRecharge();
                if (!HasCoolDown && currentAnimation == "spawn")
                {
                    currentAnimation = "standDown";
                    SetCoolDown(100);
                } else if (!HasCoolDown && Health == 0)
                {
                    // monster dead and need to be removed.
                    if (Spawn != null)
                    {
                        Spawn.RemoveDeadMonster(this);
                    }
                } else if (!HasCoolDown)
                {
                    actionString = "stand";
                    currentAnimation = actionString + directionString;
                }
                return;
            }
            setTarget(map);
            if (target != null)
            {
                Point targetPosition = new Point(target.X_Coord, target.Y_Coord);
                double dist = target.Solid.Distance(Solid);
                if (dist <= GetHitDistance())
                {
                    // hit
                    long damage = GitHitDamage();
                    if (target.TakeDamage(damage, this, map))
                    {
                        map.AddDamage(new Damage(target.Solid.Center, damage, 211, 0, 0));
                        map.AddSoundAffect(target.GetTakeHitSound(false));
                        setSoundAffect(map, SoundNames.attack);
                    }
                    SetCoolDown(Stamina > 2 ? 100 : 200);
                    actionString = "swing";
                    currentAnimation = actionString + directionString;
                    counters.ResetRecharge();
                    return;
                } else {
                    double modspeed = Mods.ConvertRange(MIN_SPEED, MAX_SPEED, 1, 10, SpeedMoveMod);
                    Point totalMove = targetPosition - MapPosition;
                    moveAmount.X = modspeed * (totalMove.X / (Math.Abs(totalMove.X) + Math.Abs(totalMove.Y)));
                    moveAmount.Y = modspeed * (totalMove.Y / (Math.Abs(totalMove.X) + Math.Abs(totalMove.Y)));
                    nextMoveAmount = moveAmount;
                }
                actionString = "walk";
            } else
            {
                double dist = MapPosition.Distance(Home);
                double modspeed = Mods.ConvertRange(MIN_SPEED, MAX_SPEED, 1, 10, SpeedMoveMod);
                if (dist <= modspeed)
                {
                    moveAmount.X = MapPosition.X - Home.X;
                    moveAmount.Y = MapPosition.Y - Home.Y;
                } else
                {
                    Point totalMove = Home - MapPosition;
                    moveAmount.X = modspeed * (totalMove.X / (Math.Abs(totalMove.X) + Math.Abs(totalMove.Y)));
                    moveAmount.Y = modspeed * (totalMove.Y / (Math.Abs(totalMove.X) + Math.Abs(totalMove.Y)));
                    nextMoveAmount = moveAmount; 
                }
            }
            if (nextMoveAmount.X != 0 || nextMoveAmount.Y != 0)
            {
                actionString = "walk";
                counters.ResetRecharge();
            } else
            {
                actionString = "stand";
                counters.Recharge();
            }
            setDirectionFromNextMove();
            currentAnimation = actionString + directionString;
        }

        private void setDirectionFromNextMove()
        {
            if (nextMoveAmount.X != 0 || nextMoveAmount.Y != 0)
            {
                Point p = new Point();
                double newDirection = p.Direction(nextMoveAmount);
                if (newDirection < 45 || newDirection > 315)
                {
                    directionString = "Down";
                }
                else if (newDirection >= 45 && newDirection <= 135)
                {
                    directionString = "Left";
                }
                else if (newDirection > 135 && newDirection <= 225)
                {
                    directionString = "Up";
                }
                else
                {
                    directionString = "Right";
                }
                
            }
        }

        public SoundAffect GetTakeHitSound(bool critacalHit)
        {
            Random rnd = new Random();
            int i = rnd.Next(1, 10);
            string gender = "male";
            SoundAffect hit = new SoundAffect($"sounds/char/{gender}Hit{i}.wav", false, this.MapPosition, 60, 200);
            return hit;
        }

        public double GetHitDistance()
        {
            return hitDistance;
        }

        public long GitHitDamage()
        {
            long damage = Mods.IntBetween((int)minDamage, (int)maxDamage);
            Stamina = Stamina - 3;
            return damage;
        }

        public bool TakeDamage(long damageAmount, Map map)
        {
            return TakeDamage(damageAmount, null, map);
        }

        public bool TakeDamage(long damageAmount, User? user, Map map)
        {
            if (Health <= 0 || currentAnimation == "spawn")
            {
                return false;
            }
            if (user != null)
            {
                lock (damageTakenLock)
                {
                    if (damageTaken.ContainsKey(user))
                    {
                        damageTaken[user] += damageAmount;
                    } else
                    {
                        damageTaken.Add(user, damageAmount);
                    }
                }
            }
            counters.ResetRecharge();
            Health -= damageAmount;
            if (user != null)
            {
                if (Health == 0)
                {
                    setSoundAffect(map, SoundNames.die);
                    SocketServer.SendMessageToUser(user, damageAmount.ToString(), $"You Killed {Name}");
                } else
                {
                    setSoundAffect(map, SoundNames.getHit);
                    SocketServer.SendMessageToUser(user, damageAmount.ToString(), $"You hit {Name}");
                }

            }
            return true;
        }

        /// <summary>
        /// set the time that a target will get chased for
        /// </summary>
        /// <returns></returns>
        private DateTime setTargetTime()
        {
            DateTime now = DateTime.Now;
            now.AddMilliseconds(100*(aggressiveDistance+chaseDistance));
            return now;
        }

        private void setTarget(Map map)
        {
            List<User> users = map.GetUsers();
            DateTime now = DateTime.Now;            
            if (target != null)
            {
                Point targetPoint = new Point(target.X_Coord, target.Y_Coord);
                if (targetPoint.Distance(MapPosition) > chaseDistance)
                {
                    target = null;
                    targetTimer = null;
                }
                counters.ResetRecharge();
            }
            if (target == null || (target != null && (!targetTimer.HasValue || (targetTimer.Value - now).TotalMilliseconds < 0)))
            {
                // reset target to null
                target = null;
                List<User> closeUsers = new List<User>();
                List<User> closestUsers = new List<User>();
                double clostestDist = 0;
                foreach (User user in users)
                {
                    double curDist = user.Location.Distance(MapPosition);
                    if (curDist <= aggressiveDistance)
                    {
                        closeUsers.Add(user);
                        if (closestUsers.Count == 0)
                        {
                            closestUsers.Add(user);
                            clostestDist = curDist;
                        }
                        else
                        {
                            if (curDist == clostestDist)
                            {
                                closestUsers.Add(user);
                            }
                            else if (curDist < clostestDist)
                            {
                                closestUsers.Clear();
                                closestUsers.Add(user);
                                clostestDist = curDist;
                            }
                        }
                    }
                }
                User mostDamage = getUserWhoDidMostDamage();
                if (mostDamage != null && closeUsers.Contains(mostDamage))
                {
                    target = mostDamage;
                    targetTimer = setTargetTime();
                }
                else if (closestUsers.Count > 0)
                {
                    target = closestUsers[Mods.IntBetween(0, closestUsers.Count - 1)];
                    targetTimer = setTargetTime();
                } else
                {
                    target = null;
                    targetTimer = null;
                }
            }
        }

        private User? getUserWhoDidMostDamage()
        {
            User? user = null;
            long damage = 0;
            lock (damageTakenLock)
            {
                foreach (KeyValuePair<User, long> kvp in damageTaken)
                {
                    if (user == null)
                    {
                        user = kvp.Key;
                        damage = kvp.Value;
                    }
                    else if (kvp.Value > damage)
                    {
                        user = kvp.Key;
                        damage = kvp.Value;
                    }

                }
            }
            return user;
        }

        public Point GetNetMoveAmount()
        { 
            return nextMoveAmount;
        }

        public void setSoundAffect(Map map, SoundNames soundName)
        {
            MonsterSound? sound =  MonsterType.GetSound(soundName);
            if (sound != null && sound.Sound != null)
            {
                map.AddSoundAffect(new SoundAffect(sound.Sound.SoundPath, false, this.MapPosition, sound.Sound.FullVolumeRadius, sound.Sound.FadeVolumeRadius));
            }
        }

        public enum AnimationNames
        {
            walkDown,
            walkUp,
            walkLeft,
            walkRight,
            standDown,
            standUp,
            standLeft,
            standRight,
            swingDown,
            swingUp,
            swingLeft,
            swingRight,
            castDown,
            castUp,
            castLeft,
            castRight,
            dieingDown,
            deadDown,
        }
        public static AnimationNames? AnimationNameToEnum(String animationString)
        {
            AnimationNames name;
            if (Enum.TryParse(animationString, out name))
            {
                return name;
            }
            return null;
        }

        public static string? AnimationEnumToName(AnimationNames nameEnum)
        {
            return Enum.GetName(typeof(AnimationNames), nameEnum);
        }

        public enum SoundNames
        {
            attack,
            die,
            getHit,
        }
        public static SoundNames? SoundNamesToEnum(String soundNameString)
        {
            SoundNames name;
            if (Enum.TryParse(soundNameString, out name))
            {
                return name;
            }
            return null;
        }

        public static string? SoundNamesEnumToName(SoundNames nameEnum)
        {
            return Enum.GetName(typeof(SoundNames), nameEnum);
        }

    }

    public class MonsterAttributes
    {
        /// <summary>
        /// the monster type id. this is the id from the database.
        /// </summary>
        public long Monster_Type_Id { get; set; } = 0;

        /// <summary>
        /// the name of the Monster. is is what will appear in the game.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// the level of the monster. used to help calculate the xp for the monster.
        /// </summary>
        public long Level { get; set; } = 1;

        /// <summary>
        /// how much max health the monster will have.
        /// </summary>
        public long Health { get; set; } = 100;

        /// <summary>
        /// how much max stamina the monster will have.
        /// </summary>
        public long Stamina { get; set; } = 100;

        /// <summary>
        /// how much max mana the monster will have.
        /// </summary>
        public long Mana { get; set; } = 100;

        /// <summary>
        /// how strong the monster is. used for how well the monster will hit.
        /// </summary>
        public long Strength { get; set; } = 10;

        /// <summary>
        /// how fast the monster moves.
        /// </summary>
        public long Speed { get; set; } = 10;

        /// <summary>
        /// used for spell casting.
        /// </summary>
        public long Wisdom { get; set; } = 10;

        /// <summary>
        /// how far away someone will be before the monster will start to try and chase.
        /// </summary>
        public long Aggressive_Distance { get; set; } = 300;

        /// <summary>
        /// the monster will continue to chase until target gets outside the chase dist.
        /// </summary>
        public long Chase_Distance { get; set; } = 320;

        /// <summary>
        /// the min damage the monster will hit for.
        /// </summary>
        public long Min_Damage { get; set; } = 1;

        /// <summary>
        /// the max damage the monster will hit for.
        /// </summary>
        public long Max_Damage { get; set; } = 10;

        public MonsterAttributes() { 
        }

    }

    /// <summary>
    /// class to keep up with health mana and stam decreasing or increasing over time.
    /// </summary>
    public class MonsterCounters
    {
        private int WaitTimeMax
        {
            get
            {
                long v = 100 + (100 - monster.Speed);
                return (int)v;
            }
        }

        private int WaitTime = 0;

        private int HealthCountMax = 100;
        private int StaminaCountMax = 50;
        private int ManaCountMax = 80;

        private int HealthCountMin = -10;
        private int StaminaCountMin = -10;
        private int ManaCountMin = -10;

        private int HealthCount = 0;
        private int StaminaCount = 0;
        private int ManaCount = 0;

        private Monster monster;

        public MonsterCounters(Monster monsterIn)
        {
            monster = monsterIn;
        }

        public void CountRecharge(int count = 1)
        {
            WaitTime = WaitTime + count;
            if (WaitTime >= WaitTimeMax)
            {
                IncCountStamina();
                IncCountHealth();
                IncCountMana();
            }
        }

        public void ResetRecharge()
        {
            WaitTime = 0;
        }

        public void Recharge()
        {
            CountRecharge(1);
        }

        public void CountStamina(int count = 1)
        {
            StaminaCount = StaminaCount + count;
            if (StaminaCount >= StaminaCountMax)
            {
                monster.Stamina = monster.Stamina + (StaminaCount / StaminaCountMax);
                StaminaCount = StaminaCount % StaminaCountMax;
            }
            else if (StaminaCount <= StaminaCountMin)
            {
                monster.Stamina = monster.Stamina - (StaminaCount / StaminaCountMin);
                StaminaCount = StaminaCount % StaminaCountMin;
            }
        }

        public void IncCountStamina()
        {
            CountStamina(1);
        }

        public void DecCountStamina()
        {
            CountStamina(-1);
        }

        public void CountHealth(int count = 1)
        {
            HealthCount = HealthCount + count;
            if (HealthCount >= HealthCountMax)
            {
                monster.Health = monster.Health + (HealthCount / HealthCountMax);
                HealthCount = HealthCount % HealthCountMax;
            }
            else if (HealthCount <= HealthCountMin)
            {
                monster.Health = monster.Health - (HealthCount / HealthCountMin);
                HealthCount = HealthCount % HealthCountMin;
            }
        }
        public void IncCountHealth()
        {
            CountHealth(1);
        }

        public void DecCountHealth()
        {
            CountHealth(-1);
        }

        public void CountMana(int count = 1)
        {
            ManaCount = ManaCount + count;
            if (ManaCount >= ManaCountMax)
            {
                monster.Mana = monster.Mana + (ManaCount / ManaCountMax);
                ManaCount = ManaCount % ManaCountMax;
            }
            else if (ManaCount <= ManaCountMin)
            {
                monster.Mana = monster.Mana - (ManaCount / ManaCountMin);
                ManaCount = ManaCount % ManaCountMin;
            }
        }
        public void IncCountMana()
        {
            CountMana(1);
        }

        public void DecCountMana()
        {
            CountMana(-1);
        }

    }
}