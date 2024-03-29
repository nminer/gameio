﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using server.mapObjects;
using server.monsters;

namespace server
{
    static class MapBuilder
    {
        public static void BuildOutside()
        {


            Map? house = Map.Create("Home", "img/maps/houseinside.png", false);
            if (house == null)
            {
                // no need to build now.
                return;
            }

            Map? outside = Map.Create("outside", "img/maps/main_map.png");
            Portal.Create("Door out", 1, 60, 720, 2, 1666, 1960);
            Portal.Create("Door In", 2, 1666, 1910, 1, 60, 700);

            //soul stone
            GameImage? stoneImage = GameImage.CreateNewImage("soulStone", "img/maps/objects/soulStone1.png");
            Shape stoneShape = new Shape().AddPoint(0, 107).AddPoint(9, 119).AddPoint(29, 124).AddPoint(72, 115).AddPoint(72,107);
            stoneShape.Save("soulStone1 Shape");
            Solid? stoneSolid = Solid.Create(stoneShape, shapePosition: new Point(0, 0), imageId: stoneImage.ImageId, drawOrder: new Point(32, 107));
            MapSolid? stoneMapSolid = MapSolid.Create(outside, stoneSolid, new Point(2487, 2152));
            //MapVisual? stoneVisual = MapVisual.Create(outside, new Point(2487, 2152), imageId: stoneImage.ImageId, drawOrder: new Point(32, 50));
            SoulStone.Create(outside, new Point(2519, 2278), 100);
            MapLight? stonelight = MapLight.Create(outside, new Point(2530, 2248), 100, mainColor:"#00fcff", midColor: "#84d7d8", amount:0.07);
            GameSound? stonesound = GameSound.CreateNewSound("camp fire", "sounds/outside/soulStone1.mp3", fadeRadius: 300, fullRadius: 50);
            MapSound? sSound = MapSound.Create(outside, new Point(2530, 2248), stonesound.SoundId);

            // build tree 1
            GameImage? tree = GameImage.CreateNewImage("tree1", "img/maps/objects/tree1.png");
            Shape treeshape = new Shape(20, 100);
            treeshape.Save("20x100");
            Solid? treesolid = Solid.Create(treeshape, shapePosition: new Point(88, 225), imageId: tree.ImageId, drawOrder: new Point(125, 240));

            // build house 1
            GameImage? house1 = GameImage.CreateNewImage("house1", "img/maps/objects/house1.png");
            Shape houseShape = new Shape().AddPoint(13, 210).AddPoint(60, 285).AddPoint(421,285).AddPoint(434,271).AddPoint(427, 210);
            houseShape.Save("house1 Shape");
            Solid? houseSolid = Solid.Create(houseShape, shapePosition: new Point(0, 0), imageId: house1.ImageId, drawOrder: new Point(210, 210));

            // place tree outside.
            MapSolid ? mapSolid = MapSolid.Create(outside, treesolid, new Point(1034, 1729));
            MapSolid? mapSolid2 = MapSolid.Create(outside, treesolid, new Point(1362, 2122));

            MapSolid? maphouse1 = MapSolid.Create(outside, houseSolid, new Point(1450, 1660));


            GameImage? campfire = GameImage.CreateNewImage("campfire", "img/maps/objects/animations/campfire.png");
            GameAnimation? gaFire = GameAnimation.CreateNewAnimation("Camp Fire", campfire, 64, 64, 5, 5, new Point(0, 0), true, 0, true);
            MapVisual? mFire = MapVisual.Create(outside, new Point(1834, 2200), animationId: gaFire.AnimationId, drawOrder: new Point(32, 50));
            MapLight? mFireLight = MapLight.Create(outside, new Point(1867, 2242), 250);

            GameSound? fireSound = GameSound.CreateNewSound("camp fire", "sounds/login/campfire.mp3", fadeRadius: 200, fullRadius: 30);
            MapSound? mSound = MapSound.Create(outside, new Point(1864, 2240), fireSound.SoundId);

            GameSound? bebachsound = GameSound.CreateNewSound("camp fire", "sounds/outside/beach.mp3", fadeRadius: 500, fullRadius: 100);
            MapSound? bSound = MapSound.Create(outside, new Point(1827, 3000), bebachsound.SoundId);
            MapSound? bSound2 = MapSound.Create(outside, new Point(1084, 3111), bebachsound.SoundId);
            MapSound? bSound3 = MapSound.Create(outside, new Point(462, 3235), bebachsound.SoundId);
            MapSound? bSound4 = MapSound.Create(outside, new Point(2594, 2998), bebachsound.SoundId);
            MapSound? bSound5 = MapSound.Create(outside, new Point(3217, 2763), bebachsound.SoundId);
            MapSound? bSound6 = MapSound.Create(outside, new Point(3654, 2994), bebachsound.SoundId);
            MapSound? bSound7 = MapSound.Create(outside, new Point(4165, 3163), bebachsound.SoundId);
            MapSound? bSound8 = MapSound.Create(outside, new Point(4656, 3199), bebachsound.SoundId);

            Shape beachShape = new Shape().AddPoint(184, 3133).AddPoint(370, 3140).AddPoint(412, 3161).AddPoint(696, 3037).AddPoint(794, 2905).AddPoint(865, 2905)
                .AddPoint(957, 2941).AddPoint(1584, 2962).AddPoint(1565, 2927).AddPoint(1738, 2885).AddPoint(1980, 2904).AddPoint(2070, 2924).AddPoint(2685, 2827)
                .AddPoint(2579, 2756).AddPoint(2769, 2691).AddPoint(3277, 2668).AddPoint(3584, 2707).AddPoint(3731, 2782).AddPoint(3767, 2957).AddPoint(3925, 2957)
                .AddPoint(4143, 2956).AddPoint(4339, 3060).AddPoint(4454, 3067).AddPoint(4803, 3010).AddPoint(4942, 3033).AddPoint(5200, 3082).AddPoint(5314, 3095);
            beachShape.IsClosedShape = false;
            beachShape.Save("outside beach line");
            Solid beachSold = Solid.Create(beachShape);
            MapSolid? beachMapSolid = MapSolid.Create(outside, beachSold);

            GameImage? somkeimg = GameImage.CreateNewImage("smoke", "img/maps/objects/animations/smoke.png");
            GameAnimation? gaSmoke = GameAnimation.CreateNewAnimation("smoke", somkeimg, 93, 149, 16, 10, new Point(0, 0), true, 0, true);
            MapVisual? msmoke = MapVisual.Create(outside, new Point(1820, 2110), animationId: gaSmoke.AnimationId, drawOrder: new Point(40, 139));
            MapVisual? msmoke2 = MapVisual.Create(outside, new Point(1820, 2110), animationId: gaSmoke.AnimationId, drawOrder: new Point(40, 139));
            MapVisual? msmokehose = MapVisual.Create(outside, new Point(1790, 1518), animationId: gaSmoke.AnimationId, drawOrder: new Point(40, 500));
            MapVisual? msmokehose2 = MapVisual.Create(outside, new Point(1790, 1518), animationId: gaSmoke.AnimationId, drawOrder: new Point(40, 500));


            // first go at monster
            MonsterType pigman = MonsterType.Create("Pig Man", 15,"fist monster is a pig man");
            //let standDownAnimation = new CharAnimation(images, 1, 0, 640, 64, 64, this, this);
            MonsterAnimationAttributes maa = new MonsterAnimationAttributes()
            {
                Animation = Monster.AnimationNames.standDown,
                Image_Path = "img/monsters/pigman.png",
                X = 0,
                Y = 640,
                Height = 64,
                Width = 64,
                Draw_Height = 80,
                Draw_Width = 80,
                Solid_X = 40,
                Solid_Y = 65,
                Frames = 1,
                Slowdown = 10
            };
            pigman.AddNewAnimation(maa);
            maa.Animation = Monster.AnimationNames.standUp;
            maa.Y = 512;
            pigman.AddNewAnimation(maa);
            maa.Animation = Monster.AnimationNames.standLeft;
            maa.Y = 576;
            pigman.AddNewAnimation(maa);
            maa.Animation = Monster.AnimationNames.standRight;
            maa.Y = 704;
            pigman.AddNewAnimation(maa);
            maa.Animation = Monster.AnimationNames.walkRight;
            maa.X = 64;
            maa.Y = 704;
            maa.Frames = 8;
            maa.Slowdown = 7;
            pigman.AddNewAnimation(maa);
            maa.Animation = Monster.AnimationNames.walkDown;
            maa.Y = 640;
            pigman.AddNewAnimation(maa);
            maa.Animation = Monster.AnimationNames.walkLeft;
            maa.Y = 576;
            pigman.AddNewAnimation(maa);
            maa.Animation = Monster.AnimationNames.walkUp;
            maa.Y = 512;
            pigman.AddNewAnimation(maa);
            maa.Animation = Monster.AnimationNames.deadDown;
            maa.Y = 1280;
            maa.X = 320;
            maa.Frames = 1;
            maa.Slowdown = 10;
            pigman.AddNewAnimation(maa);
            maa.Animation = Monster.AnimationNames.dieingDown;
            maa.X = 64;
            maa.Y = 1280;
            maa.Frames = 5;
            maa.After_Animation_Name = "deadDown";
            pigman.AddNewAnimation(maa);
            maa.Animation = Monster.AnimationNames.swingRight;
            maa.Y = 960;
            maa.Frames = 5;
            maa.After_Animation_Name = "standRight";
            pigman.AddNewAnimation(maa);
            maa.Animation = Monster.AnimationNames.swingDown;
            maa.After_Animation_Name = "standDown";
            maa.Y = 896;
            pigman.AddNewAnimation(maa);
            maa.Animation = Monster.AnimationNames.swingLeft;
            maa.After_Animation_Name = "standLeft";
            maa.Y = 832;
            pigman.AddNewAnimation(maa);
            maa.Animation = Monster.AnimationNames.swingUp;
            maa.After_Animation_Name = "standUp";
            maa.Y = 768;
            pigman.AddNewAnimation(maa);
            GameSound? pigManHit1 = GameSound.CreateNewSound("pig man hit 1", "sounds/monsters/pigman/hit-1.wav", fadeRadius: 500, fullRadius: 100);
            GameSound? pigManHit2 = GameSound.CreateNewSound("pig man hit 2", "sounds/monsters/pigman/hit-2.wav", fadeRadius: 500, fullRadius: 100);
            GameSound? pigManHit3 = GameSound.CreateNewSound("pig man hit 3", "sounds/monsters/pigman/hit-3.wav", fadeRadius: 500, fullRadius: 100);
            GameSound? pigManGetHit1 = GameSound.CreateNewSound("pig man get hit 1", "sounds/monsters/pigman/die-1.wav", fadeRadius: 500, fullRadius: 100);
            GameSound? pigManGetHit2 = GameSound.CreateNewSound("pig man get hit 2", "sounds/monsters/pigman/die-2.wav", fadeRadius: 500, fullRadius: 100);
            GameSound? pigManDie = GameSound.CreateNewSound("pig man die", "sounds/monsters/pigman/belch.wav", fadeRadius: 500, fullRadius: 100);
            pigman.AddSound(pigManHit1, Monster.SoundNames.attack);
            pigman.AddSound(pigManHit2, Monster.SoundNames.attack);
            pigman.AddSound(pigManHit3, Monster.SoundNames.attack);
            pigman.AddSound(pigManGetHit1, Monster.SoundNames.getHit);
            pigman.AddSound(pigManGetHit2, Monster.SoundNames.getHit);
            pigman.AddSound(pigManDie, Monster.SoundNames.die);

            //Monster.AnimationNames.standDown, "img/monsters/pigman.png", 0, 640, 64, 64, 80, 80, 1, 10);
            Monster pig = Monster.Create(
                new MonsterAttributes()
                {
                    Monster_Type_Id = pigman.MonsterTypeId,
                    Name = "Hambo",
                    Level = 1,
                    Health = 100,
                    Stamina = 100,
                    Mana = 10,
                    Speed = 10,
                    Strength = 10,
                    Wisdom = 1,
                    Aggressive_Distance = 400,
                    Chase_Distance = 450,
                    Min_Damage = 1,
                    Max_Damage = 2,
                });// pigman.MonsterTypeId, "Pig", 1, 100, 100, 100, 10, 10, 10, 300, 300, 1, 10);
            MonsterSpawn spawn1 = MonsterSpawn.Create(outside.Id, 1840, 2222, 0, 1, 10000, 0);
            spawn1.AddMonster(pig);
            MonsterSpawn spawn2 = MonsterSpawn.Create(outside.Id, 1890, 2242, 0, 1, 10000, 0);
            spawn1.AddMonster(pig);
            spawn2.AddMonster(pig);


            // spider 
            MonsterType spiderType = MonsterType.Create("spider", 20, "spider monster");
            MonsterAnimationAttributes spiderAnimation = new MonsterAnimationAttributes()
            {
                Animation = Monster.AnimationNames.standLeft,
                Image_Path = "img/monsters/spider.png",
                X = 0,
                Y = 30,
                Height = 44,
                Width = 128,
                Draw_Height = 40,
                Draw_Width = 128,
                Solid_X = 64,
                Solid_Y = 24,
                Frames = 4,
                Slowdown = 20
            };
            spiderType.AddNewAnimation(spiderAnimation);
            spiderAnimation.Animation = Monster.AnimationNames.standUp;
            spiderAnimation.Y = 110;
            spiderType.AddNewAnimation(spiderAnimation);
            spiderAnimation.Animation = Monster.AnimationNames.standRight;
            spiderAnimation.Y = 184;
            spiderType.AddNewAnimation(spiderAnimation);
            spiderAnimation.Animation = Monster.AnimationNames.standDown;
            spiderAnimation.Y = 240;
            spiderType.AddNewAnimation(spiderAnimation);
            spiderAnimation.Animation = Monster.AnimationNames.walkUp;
            spiderAnimation.Y = 110;
            spiderAnimation.X = 512;
            spiderAnimation.Frames = 7;
            spiderAnimation.Slowdown = 0;
            spiderType.AddNewAnimation(spiderAnimation);
            spiderAnimation.Animation = Monster.AnimationNames.walkLeft;
            spiderAnimation.Y = 30;       
            spiderType.AddNewAnimation(spiderAnimation);
            spiderAnimation.Animation = Monster.AnimationNames.walkRight;
            spiderAnimation.Y = 184;
            spiderType.AddNewAnimation(spiderAnimation);
            spiderAnimation.Animation = Monster.AnimationNames.walkDown;
            spiderAnimation.Y = 240;
            spiderType.AddNewAnimation(spiderAnimation);
            spiderAnimation.Animation = Monster.AnimationNames.deadDown;
            spiderAnimation.Y = 240;
            spiderAnimation.X = 2944;
            spiderAnimation.Frames = 1;
            spiderAnimation.Slowdown = 10;
            spiderType.AddNewAnimation(spiderAnimation);
            spiderAnimation.Animation = Monster.AnimationNames.dieingDown;
            spiderAnimation.X = 2432;
            spiderAnimation.Y = 240;
            spiderAnimation.Frames = 5;
            spiderAnimation.After_Animation_Name = "deadDown";
            spiderType.AddNewAnimation(spiderAnimation);
            spiderAnimation.Animation = Monster.AnimationNames.swingUp;
            spiderAnimation.Y = 104;
            spiderAnimation.X = 1664;
            spiderAnimation.Frames = 4;
            spiderAnimation.Slowdown = 10;
            spiderAnimation.After_Animation_Name = "standUp";
            spiderType.AddNewAnimation(spiderAnimation);
            spiderAnimation.Animation = Monster.AnimationNames.swingLeft;
            spiderAnimation.Y = 30;
            spiderAnimation.After_Animation_Name = "standLeft";
            spiderType.AddNewAnimation(spiderAnimation);
            spiderAnimation.Animation = Monster.AnimationNames.swingRight;
            spiderAnimation.Y = 184;
            spiderAnimation.After_Animation_Name = "standRight";
            spiderType.AddNewAnimation(spiderAnimation);
            spiderAnimation.Animation = Monster.AnimationNames.swingDown;
            spiderAnimation.Y = 240;
            spiderAnimation.After_Animation_Name = "standDown";
            spiderType.AddNewAnimation(spiderAnimation);

            GameSound? spiderSwing1 = GameSound.CreateNewSound("spider swing 1", "sounds/monsters/spider/spider_swing.mp3", fadeRadius: 500, fullRadius: 100);
            GameSound? spiderSwing2 = GameSound.CreateNewSound("spider swing 2", "sounds/monsters/spider/spider_swing2.mp3", fadeRadius: 500, fullRadius: 100);
            GameSound? spiderHit1 = GameSound.CreateNewSound("spider hit 1", "sounds/monsters/spider/spider_hit.mp3", fadeRadius: 500, fullRadius: 100);
            GameSound? spiderHit2 = GameSound.CreateNewSound("spider hit 2", "sounds/monsters/spider/spider_hit2.mp3", fadeRadius: 500, fullRadius: 100);
            GameSound? spiderDie = GameSound.CreateNewSound("spider die", "sounds/monsters/spider/spider_die.mp3", fadeRadius: 500, fullRadius: 100);
            spiderType.AddSound(spiderSwing1, Monster.SoundNames.attack);
            spiderType.AddSound(spiderSwing2, Monster.SoundNames.attack);
            spiderType.AddSound(spiderHit1, Monster.SoundNames.getHit);
            spiderType.AddSound(spiderHit2, Monster.SoundNames.getHit);
            spiderType.AddSound(spiderDie, Monster.SoundNames.die);

            Monster spider = Monster.Create(
                new MonsterAttributes()
                {
                    Monster_Type_Id = spiderType.MonsterTypeId,
                    Name = "spider",
                    Level = 1,
                    Health = 100,
                    Stamina = 100,
                    Mana = 10,
                    Speed = 20,
                    Strength = 10,
                    Wisdom = 1,
                    Aggressive_Distance = 400,
                    Chase_Distance = 450,
                    Min_Damage = 1,
                    Max_Damage = 2,
                });
            MonsterSpawn spawn3 = MonsterSpawn.Create(outside.Id, 1890, 2362, 0, 1, 10000, 0);
            spawn3.AddMonster(spider);
        }
    }
}
