using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using TestModule;

public class Detail : IComparable<Detail>
{
    public static int ID { get; private set; } = 0;
    public static string Name { get; private set; }
    public bool NeedRotate = false;
    public bool IsRotated = false;
    private bool IsRotedLoker = true;
    public double Pading { get; set; }

    public List<EntityObject> Entities { get; private set; }
    private Block Block { get; set; }
    public Insert Insert { get; private set; }
    public Vector3 Center { get; private set; }

    public double Width { get; private set; }
    public double Height { get; private set; }
    public double Area { get; private set; }
    public BoundingBox Bounds { get; set; }
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

        for (double i = 0; i <= fullAngle; i += fullAngle / step)
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

                case Ellipse ellipse:
                    ellipse.Center = RotatePointAroundOrigin(ellipse.Center, cos, sin);
                    ellipse.StartAngle = NormalizeAngle(ellipse.StartAngle + angle);
                    ellipse.EndAngle = NormalizeAngle(ellipse.EndAngle + angle);
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
        Center = new Vector3(Width / 2, Height / 2, 0);
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

                //case Point p:
                //    points.Add(p.Position);
                //    break;

                case Ellipse ellipse:
                    points.Add(ellipse.Center);
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