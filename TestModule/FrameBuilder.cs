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
        public double Capacity { get; private set; } = 0;

        public BoundingBox Bounds { get; private set; } = new BoundingBox(0, 0, 0, 0);
        private List<EntityObject> _frame;
        public static double Pading {  get; private set; }

        private static double _lastX = 0;

        public FrameBuilder(double pading = 0)
        {
            Pading = pading;
            _frame = new List<EntityObject>();
        }

        public void CreateFrame(double width, double height)
        {
            Bounds = new BoundingBox(_lastX, _lastX + width, 0, height);

            _frame.Add(new Line(new Vector2(Bounds.MinX, Bounds.MinY), new Vector2(Bounds.MaxX, Bounds.MinY)));
            _frame.Add(new Line(new Vector2(Bounds.MaxX, Bounds.MinY), new Vector2(Bounds.MaxX, Bounds.MaxY)));
            _frame.Add(new Line(new Vector2(Bounds.MaxX, Bounds.MaxY), new Vector2(Bounds.MinX, Bounds.MaxY)));
            _frame.Add(new Line(new Vector2(Bounds.MaxX, Bounds.MaxY), new Vector2(Bounds.MinX, Bounds.MinY)));
            Capacity = width * height;

            _lastX += width;
        }

        public void InsertCapacity(double newCapacity) => Capacity = newCapacity;
    }
}
