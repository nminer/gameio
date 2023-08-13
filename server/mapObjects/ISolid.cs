namespace server.mapObjects
{
    interface ISolid
    {
        public Line[] Lines(Point? position = null);

        public double Distance(Circle circle, Point? position = null);
    }
}
