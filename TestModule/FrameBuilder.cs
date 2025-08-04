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
        private static int _id;
        public double Capacity { get; private set; } = 0;

        public BoundingBox Bounds { get; private set; } = new BoundingBox(0, 0, 0, 0);
        private List<EntityObject> _frame;

        private List<Detail> DetailsInFrame = new List<Detail>();
        public Insert FullFrame { get; private set; }

        public static double Pading {  get; private set; }

        private static double _lastX = 0;

        public FrameBuilder(double pading = 0)
        {
            _id = 0;
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

        public void AddNewDetail(Detail detail, double x, double y)
        {
            detail.MoveDetail(x, y);
            DetailsInFrame.Add(detail);
            Capacity -= detail.Area;
        }

        public void CompleteFrame()
        {
            Block full = new Block($"FullFrame{_id}");
            foreach (var lines in _frame)
                full.Entities.Add((EntityObject)lines.Clone());

            foreach (var elements in DetailsInFrame)
                for (int i = 0; i < elements.Entities.Count; i++)
                    full.Entities.Add((EntityObject)elements.Entities[i].Clone());

            FullFrame = new Insert(full);

            _id++;
        }

        public void InsertCapacity(double newCapacity) => Capacity = newCapacity;
    }
}
