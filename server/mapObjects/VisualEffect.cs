using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{
    public class VisualEffect
    {
        public string ImagePath;

        public int FrameCount;

        public Point StartFramePosition;

        public Point Position;

        public bool Horizontal;

        public int AnimationHeight;

        public int AnimationWidth;

        public int SlowDown;

        public double DrawOrder;

        /// <summary>
        /// Load a sound from its sound id in the database.
        /// </summary>
        /// <param name="imageId"></param>
        public VisualEffect(string path, Point position, Point framePosition, int frameCont, int height, int width, int slowDown, double drawOrder = 0, bool horizontal = true )
        {
            this.ImagePath = path;
            this.Position = position;
            this.StartFramePosition = framePosition;
            this.FrameCount = frameCont;
            this.Horizontal = horizontal;
            this.AnimationHeight = height;
            this.AnimationWidth = width;
            this.SlowDown = slowDown;
            this.DrawOrder = drawOrder;
        }

        public object? GetJsonVisualObject()
        {
            return new { path = ImagePath,
                x = Position.X,
                y = Position.Y,
                frameX = StartFramePosition.X,
                frameY = StartFramePosition.Y,
                frameCount = FrameCount,
                horizontal = Horizontal,
                height = AnimationHeight,
                width = AnimationWidth,
                slowDown = SlowDown,
                drawOrder = DrawOrder,
            };
        }
    }
}
