﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using server.mapObjects;

namespace server
{
    static class MapBuilder
    {
        public static void BuildOutsid()
        {
            Map.Create("Home", "img/maps/houseinside.png");

            Map? outside = Map.Create("outside", "img/maps/main_map.png");
            Portal.Create("Door out", 1, 60, 720, 2, 1666, 1960);
            Portal.Create("Door In", 2, 1666, 1910, 1, 60, 700);

            // build tree 1
            GameImage? tree = GameImage.CreateNewImage("tree1", "img/maps/objects/tree1.png");
            Shape treeshape = new Shape(20, 100);
            treeshape.Save("20x100");
            Solid? treesolid = Solid.Create(treeshape, shapePosition: new Point(88, 225), imageId: tree.ImageId, drawOrder: new Point(240, 125));

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
        }
    }
}
