using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestModule
{
    public class FrameBuilder
    {
        private static int staticPading = 10;
        private int pading = 10;
        private int step = 50;

        public double Area { get; private set; }

        public double lastX {  get; private set; }
        public double Width {  get; private set; }
        public double Height {  get; private set; }

        public BoundingBox BoundingBox { get; private set; }

        public FrameBuilder(int pading)
        {
            this.pading = pading;
            step = pading;
            staticPading = pading;
        }

        public Block Construct(int width, int height)
        {
            lastX = 0 + pading;
            Area = width * height;

            Vector3 leftDown = new Vector3(lastX, 0, 0);
            Vector3 rightDown = new Vector3(width + pading, 0, 0);
            Vector3 rightUp = new Vector3(width + pading, height, 0);
            Vector3 leftUp = new Vector3(lastX, height, 0);

            BoundingBox = new BoundingBox(leftDown.X, rightUp.X, leftDown.Y, rightUp.Y);

            Line line1 = new Line(leftDown, rightDown);
            Line line2 = new Line(rightDown, rightUp);
            Line line3 = new Line(rightUp, leftUp);
            Line line4 = new Line(leftUp, leftDown);

            List<EntityObject> objs = new List<EntityObject>
            {
                line1,
                line2,
                line3,
                line4
            };

            staticPading += step + step;
            pading = staticPading;
            Width = width;
            Height = height;

            return new Block($"Frame{pading / step}", objs);
        }
    }
}
