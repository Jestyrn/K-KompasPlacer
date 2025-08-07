using netDxf;
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using netDxf.Tables;
using Vector3 = netDxf.Vector3;

namespace TestModule
{
    public class DetailsProcessor
    {
        public static List<Detail> GetAllDetails(string path)
        {
            DxfDocument dxf = DxfDocument.Load(path);
            List<Block> blocks = (List<Block>)dxf.Blocks.ToList().Where(x => !x.Name.Contains("*")).ToList();
            List<EntityObject> ents = new List<EntityObject>();

            List<Detail> details = new List<Detail>();

            foreach (var block in blocks)
            {
                foreach (var entity in block.Entities.Where(x => x.Type == EntityType.Line | x.Type == EntityType.Arc | x.Type == EntityType.Circle))
                {
                    ents.Add(entity);
                }

                if (ents.Count != 0)
                {
                    details.Add(new Detail(ents));
                    ents.Clear();
                }
            }

            return details;
        }
    }

    public class Detail : IComparable<Detail>
    {
        public static int ID { get; private set; } = 0;
        public static string Name { get; private set; }
        public bool NeedRotate = false;
        public bool IsRotated = false;
        private bool IsRotedLoker = true;
        
        public List<EntityObject> Entities { get; private set; }
        private Block Block { get; set; }
        public Insert Insert { get; private set; }
        public Vector3 Center { get; private set; }

        public double Width { get; private set; }
        public double Height { get; private set; }
        public double Area { get; private set; }
        public BoundingBox Bounds { get; private set; }
        public List<EntityObject> BoundsDXF { get; private set; } = new List<EntityObject>();

        private List<Vector3> points = new List<Vector3>();

        private double minX;
        private double minY;
        private double maxX;
        private double maxY;

        public Detail(List<EntityObject> objects)
        {
            Entities = new List<EntityObject>(objects);
            CompletePrepairs();
            FindBestAngle();
            IsRotedLoker = false;
        }

        private void UpdateInsert()
        {
            Name = $"Detail{ID}";

            Block = new Block(Name);
            foreach (EntityObject obj in Entities)
                Block.Entities.Add((EntityObject)obj.Clone());

            Insert = new Insert(Block);
            ID++;
        }

        private void CompletePrepairs()
        {
            FindPoints();
            FindMinMax();
            FindGeometry();
            UpdateInsert();
        }

        public void MoveDetail(double x, double y)
        {
            Vector3 newPoint = new Vector3(x, y, 0);

            Vector3 offset = new Vector3(
                x - Bounds.MinX,
                y - Bounds.MinY,
                0);

            foreach (var entity in Entities)
            {
                switch (entity)
                {
                    case Line line:
                        line.StartPoint = ApplyOffset(line.StartPoint, offset);
                        line.EndPoint = ApplyOffset(line.EndPoint, offset);
                        break;

                    case Circle circle:
                        circle.Center = ApplyOffset(circle.Center, offset);
                        break;

                    case Arc arc:
                        arc.Center = ApplyOffset(arc.Center, offset);
                        break;

                    case Ellipse ellipse:
                        ellipse.Center = ApplyOffset(ellipse.Center, offset);
                        break;
                }
            }

            CompletePrepairs();
        }

        private Vector3 ApplyOffset(Vector3 point, Vector3 offset)
        {
            return new Vector3(
                point.X + offset.X,
                point.Y + offset.Y,
                point.Z);
        }

        private void FindBestAngle()
        {
            double fullAngle = 360;
            int step = 720;

            // список <угол, площадь>
            Dictionary<double, double> anglArea = new Dictionary<double, double>();

            for (double i = 0; i <= fullAngle; i += fullAngle/step)
            {
                RotateDetail(i);
                anglArea.Add(i, Area);
                RotateDetail(-i);
            }

            anglArea = anglArea.OrderByDescending(p => p.Value).ToDictionary();

            double bestAngle = 0;
            double lowerArea = double.MaxValue;

            foreach (var pare in anglArea)
            {
                if (lowerArea > pare.Value)
                {
                    bestAngle = pare.Key;
                    lowerArea = pare.Value;
                }
            }

            RotateDetail(bestAngle);
        }

        private Vector3 RotatePointAroundOrigin(Vector3 point, double cos, double sin)
        {
            double x = point.X - Center.X;
            double y = point.Y - Center.Y;

            return new Vector3(
                x * cos - y * sin + Center.X,
                x * sin + y * cos + Center.Y,
                0);
        }

        private double NormalizeAngle(double angle)
        {
            angle %= 360;
            return angle < 0 ? angle + 360 : angle;
        }

        public void RotateDetail(double angle)
        {
            if (Math.Abs(angle) == 0) return;
            else if (Math.Abs(angle) < 0.001) return;

            angle = NormalizeAngle(angle);

            double rad = angle * Math.PI / 180;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);

            foreach (var item in Entities)
            {
                switch (item)
                {
                    case Line line:
                        line.StartPoint = RotatePointAroundOrigin(line.StartPoint, cos, sin);
                        line.EndPoint = RotatePointAroundOrigin(line.EndPoint, cos, sin);
                        break;

                    case Circle circle:
                        circle.Center = RotatePointAroundOrigin(circle.Center, cos, sin);
                        break;

                    case Arc arc:
                        arc.Center = RotatePointAroundOrigin(arc.Center, cos, sin);
                        arc.StartAngle = NormalizeAngle(arc.StartAngle + angle);
                        arc.EndAngle = NormalizeAngle(arc.EndAngle + angle);
                        break;
                }
            }

            if (!IsRotedLoker)
                IsRotated = true;

            FindPoints();
            FindMinMax();
            FindGeometry();
        }

        private void FindGeometry()
        {
            Width = Math.Abs(maxX - minX);
            Height = Math.Abs(maxY - minY);
            Center = new Vector3(Width/2, Height/2, 0);
            Area = Width * Height;

            Bounds = new BoundingBox(minX, maxX, minY, maxY);
            BoundsDXF = new List<EntityObject>
            {
                new Line(new Vector2(minX, minY), new Vector2(minX, maxY)),
                new Line(new Vector2(minX, maxY), new Vector2(maxX, maxY)),
                new Line(new Vector2(maxX, maxY), new Vector2(maxX, minY)),
                new Line(new Vector2(maxX, minY), new Vector2(minX, minY)),
            };
        }

        private void FindMinMax()
        {
            minX = int.MaxValue;
            minY = int.MinValue;
            maxX = int.MinValue;
            maxY = int.MaxValue;

            foreach (var p in points)
            {
                minX = Math.Min(minX, p.X);
                minY = Math.Max(minY, p.Y);

                maxX = Math.Max(maxX, p.X);
                maxY = Math.Min(maxY, p.Y);
            }
        }

        private void FindPoints()
        {
            points.Clear();
            foreach (EntityObject obj in Entities)
            {
                switch (obj)
                {
                    case Arc arc:
                        points.Add(arc.Center);
                        break;

                    case Circle circ:
                        points.Add(circ.Center);
                        break;

                    case Line line:
                        points.Add(line.StartPoint);
                        points.Add(line.EndPoint);
                        break;

                    case Point p:
                        points.Add(p.Position);
                        break;
                }
            }
        }

        public int CompareTo(Detail other)
        {
            if (other == null) return 1;
            return Area.CompareTo(other.Area);
        }
    }

    public class SimpleDetail
    {
        public string DetailName;
        public Vector2 NewCoorinats;
    }

    public class BoundingBox
    {
        public double MinX { get; }
        public double MaxX { get; }
        public double MinY { get; }
        public double MaxY { get; }

        public double Width { get; }
        public double Height { get; }
        
        public double Area { get; }

        public BoundingBox(double minX, double maxX, double minY, double maxY)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;

            Width = Math.Abs(MaxX - MinX);
            Height = Math.Abs(MaxY - MinY);
        
            Area = Width * Height;
        }

        public BoundingBox(Vector2 leftX_upY, double width, double height)
        {
            MinX = leftX_upY.X;
            MaxX = leftX_upY.X + width;
            MinY = leftX_upY.Y;
            MaxY = leftX_upY.Y - height;

            Width = Math.Abs(MaxX - MinX);
            Height = Math.Abs(MaxY - MinY);

            Area = Width * Height;
        }

        public bool Intersects(BoundingBox other)
        {
            // Прямоугольники НЕ пересекаются?
            return MinX < other.MaxX &&
                   MaxX > other.MinX &&
                   MinY > other.MaxY &&
                   MaxY < other.MinY;
        }

        public bool Contains(BoundingBox other)
        {
            return  MinX <= other.MinX &
                    MaxX >= other.MaxX &
                    MinY >= other.MinY &
                    MaxY <= other.MaxY;
        }

        public bool CanInsert(BoundingBox whoInserted)
        {
            return  Width >= whoInserted.Width &
                    Height >= whoInserted.Height;
        }

        public List<BoundingBox> Split(BoundingBox taken)
        {
            var result = new List<BoundingBox>();

            // Верх
            if (taken.MaxY < this.MaxY)
                result.Add(new BoundingBox(
                    new Vector2(this.MinX, taken.MaxY),
                    this.Width, this.MaxY - taken.MaxY));

            // Низ
            if (taken.MinY > this.MinY)
                result.Add(new BoundingBox(
                    new Vector2(this.MinX, this.MinY),
                    this.Width, taken.MinY - this.MinY));

            // Лево
            if (taken.MinX > this.MinX)
                result.Add(new BoundingBox(
                    new Vector2(this.MinX, this.MinY),
                    taken.MinX - this.MinX, this.Height));

            // Право
            if (taken.MaxX < this.MaxX)
                result.Add(new BoundingBox(
                    new Vector2(taken.MaxX, this.MinY),
                    this.MaxX - taken.MaxX, this.Height));

            return result;
        }
    }
}
