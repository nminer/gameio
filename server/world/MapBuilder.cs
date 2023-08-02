using System;
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





        }
    }
}
