using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using netDxf;
using netDxf.Blocks;
using netDxf.Entities;

/*
 Revised Detail class:
 - fixed min/max logic
 - correct center calculation (used as rotation pivot)
 - more accurate point collection for Lines/Circles/Arcs/Ellipses
 - padding is applied to geometry and movement
 - FindBestAngle uses clones (no side-effects) and searches for minimal bounding area
 - thread-safe id generation
 - clearer method names: MoveTo / MoveBy
*/

public class Detail : IComparable<Detail>
{
    private static int _idCounter = 0;
    public int Id { get; private set; }
    public string Name { get; private set; }

    public bool NeedRotate { get; set; } = false;
    public bool IsRotated { get; private set; } = false;
    private bool _rotationLocker = true; // когда true — не помечаем как 'IsRotated' (используется при инициализации)

    // Исправленное имя свойства: Padding (раньше было Pading)
    public double Padding { get; set; } = 0.0;

    public List<EntityObject> Entities { get; private set; } = new List<EntityObject>();
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

    public Detail(List<EntityObject> objects, double padding = 0.0)
    {
        // thread-safe id assignment
        Id = Interlocked.Increment(ref _idCounter);
        Name = $"Detail{Id}";

        Padding = padding;
        _rotationLocker = true; // блокируем пометку IsRotated на время подготовки

        // клонируем сразу — защищаем оригинальные объекты от внешних изменений
        Entities = new List<EntityObject>(objects.Select(o => (EntityObject)o.Clone()));

        CompletePrepairs();
        FindBestAngle();

        _rotationLocker = false; // дальнейшие повороты будут помечать IsRotated
    }

    private void UpdateInsert()
    {
        Block = new Block(Name);
        foreach (EntityObject obj in Entities)
            Block.Entities.Add((EntityObject)obj.Clone());

        Insert = new Insert(Block);
    }

    private void CompletePrepairs()
    {
        FindPoints();
        FindMinMax();
        FindGeometry();
        UpdateInsert();
    }

    // Перемещение так, чтобы padded Bounds.MinX/MinY стало (x,y)
    public void MoveTo(double x, double y)
    {
        var offset = new Vector3(x - Bounds.MinX, y - Bounds.MinY, 0);
        ApplyOffsetToEntities(offset);
        CompletePrepairs();
    }

    // Перемещение на дельту
    public void MoveBy(double dx, double dy)
    {
        var offset = new Vector3(dx, dy, 0);
        ApplyOffsetToEntities(offset);
        CompletePrepairs();
    }

    private void ApplyOffsetToEntities(Vector3 offset)
    {
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

                default:
                    // Для прочих типов можно добавить обработку (Polyline, LwPolyline, Spline и т.д.)
                    break;
            }
        }
    }

    private Vector3 ApplyOffset(Vector3 point, Vector3 offset)
    {
        return new Vector3(point.X + offset.X, point.Y + offset.Y, point.Z + offset.Z);
    }

    // Ищем угол оптимизированно — не меняя оригинальные entities при переборе
    private void FindBestAngle()
    {
        _rotationLocker = true;

        var originalClone = Entities.Select(e => (EntityObject)e.Clone()).ToList();

        double bestAngle = 0;
        double bestArea = double.MaxValue;

        // Шагы можно настраивать: 360 шагов = 1 градус; для точнее увеличить шаги
        int steps = 360;

        for (int i = 0; i <= steps; i++)
        {
            double angle = i * (360.0 / steps);

            // делаем глубокий клон тестовой геометрии
            var testEntities = originalClone.Select(e => (EntityObject)e.Clone()).ToList();
            RotateEntities(testEntities, angle);

            var pts = CollectPointsFromEntities(testEntities);
            GetMinMaxFromPoints(pts, out double tMinX, out double tMaxX, out double tMinY, out double tMaxY);

            double w = Math.Abs(tMaxX - tMinX) + 2.0 * Padding;
            double h = Math.Abs(tMaxY - tMinY) + 2.0 * Padding;
            double area = w * h;

            if (area < bestArea)
            {
                bestArea = area;
                bestAngle = angle;
            }
        }

        // применяем лучший угол к реальным объектам
        RotateDetail(bestAngle);
        _rotationLocker = false;
    }

    // Вспомогательный метод: поворачивает коллекцию entity без изменения основного состояния Detail
    private void RotateEntities(List<EntityObject> entities, double angle)
    {
        if (Math.Abs(angle) < 1e-9) return;
        angle = NormalizeAngle(angle);
        double rad = angle * Math.PI / 180.0;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);

        // pivot — центр этих entities (чтобы вращать корректно)
        var pts = CollectPointsFromEntities(entities);
        GetMinMaxFromPoints(pts, out double tMinX, out double tMaxX, out double tMinY, out double tMaxY);
        var center = new Vector3((tMinX + tMaxX) / 2.0, (tMinY + tMaxY) / 2.0, 0);

        foreach (var item in entities)
        {
            switch (item)
            {
                case Line line:
                    line.StartPoint = RotatePointAroundOrigin(line.StartPoint, center, cos, sin);
                    line.EndPoint = RotatePointAroundOrigin(line.EndPoint, center, cos, sin);
                    break;

                case Circle circle:
                    circle.Center = RotatePointAroundOrigin(circle.Center, center, cos, sin);
                    break;

                case Arc arc:
                    arc.Center = RotatePointAroundOrigin(arc.Center, center, cos, sin);
                    arc.StartAngle = NormalizeAngle(arc.StartAngle + angle);
                    arc.EndAngle = NormalizeAngle(arc.EndAngle + angle);
                    break;

                case Ellipse ellipse:
                    ellipse.Center = RotatePointAroundOrigin(ellipse.Center, center, cos, sin);
                    ellipse.StartAngle = NormalizeAngle(ellipse.StartAngle + angle);
                    ellipse.EndAngle = NormalizeAngle(ellipse.EndAngle + angle);
                    break;
            }
        }
    }

    private Vector3 RotatePointAroundOrigin(Vector3 point, Vector3 center, double cos, double sin)
    {
        double x = point.X - center.X;
        double y = point.Y - center.Y;
        return new Vector3(x * cos - y * sin + center.X, x * sin + y * cos + center.Y, point.Z);
    }

    private double NormalizeAngle(double angle)
    {
        angle %= 360.0;
        if (angle < 0) angle += 360.0;
        return angle;
    }

    public void RotateDetail(double angle)
    {
        if (Math.Abs(angle) < 1e-9) return;

        angle = NormalizeAngle(angle);
        double rad = angle * Math.PI / 180.0;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);

        var pivot = Center; // теперь Center рассчитывается корректно в FindGeometry

        foreach (var item in Entities)
        {
            switch (item)
            {
                case Line line:
                    line.StartPoint = RotatePointAroundOrigin(line.StartPoint, pivot, cos, sin);
                    line.EndPoint = RotatePointAroundOrigin(line.EndPoint, pivot, cos, sin);
                    break;

                case Circle circle:
                    circle.Center = RotatePointAroundOrigin(circle.Center, pivot, cos, sin);
                    break;

                case Arc arc:
                    arc.Center = RotatePointAroundOrigin(arc.Center, pivot, cos, sin);
                    arc.StartAngle = NormalizeAngle(arc.StartAngle + angle);
                    arc.EndAngle = NormalizeAngle(arc.EndAngle + angle);
                    break;

                case Ellipse ellipse:
                    ellipse.Center = RotatePointAroundOrigin(ellipse.Center, pivot, cos, sin);
                    ellipse.StartAngle = NormalizeAngle(ellipse.StartAngle + angle);
                    ellipse.EndAngle = NormalizeAngle(ellipse.EndAngle + angle);
                    break;
            }
        }

        if (!_rotationLocker)
            IsRotated = true;

        CompletePrepairs();
    }

    private void FindGeometry()
    {
        Width = Math.Abs(maxX - minX) + 2.0 * Padding;
        Height = Math.Abs(maxY - minY) + 2.0 * Padding;

        // корректный центр в мировой системе координат
        Center = new Vector3((minX + maxX) / 2.0, (minY + maxY) / 2.0, 0);

        double pMinX = minX - Padding;
        double pMinY = minY - Padding;
        double pMaxX = maxX + Padding;
        double pMaxY = maxY + Padding;

        Area = Math.Abs(pMaxX - pMinX) * Math.Abs(pMaxY - pMinY);

        Bounds = new BoundingBox(pMinX, pMaxX, pMinY, pMaxY);

        BoundsDXF = new List<EntityObject>
        {
            new Line(new Vector2(pMinX, pMinY), new Vector2(pMinX, pMaxY)),
            new Line(new Vector2(pMinX, pMaxY), new Vector2(pMaxX, pMaxY)),
            new Line(new Vector2(pMaxX, pMaxY), new Vector2(pMaxX, pMinY)),
            new Line(new Vector2(pMaxX, pMinY), new Vector2(pMinX, pMinY)),
        };
    }

    private void FindMinMax()
    {
        GetMinMaxFromPoints(points, out minX, out maxX, out minY, out maxY);
    }

    private void GetMinMaxFromPoints(List<Vector3> pts, out double outMinX, out double outMaxX, out double outMinY, out double outMaxY)
    {
        if (pts == null || pts.Count == 0)
        {
            outMinX = outMinY = outMaxX = outMaxY = 0;
            return;
        }

        outMinX = double.MaxValue;
        outMinY = double.MaxValue;
        outMaxX = double.MinValue;
        outMaxY = double.MinValue;

        foreach (var p in pts)
        {
            if (double.IsNaN(p.X) || double.IsInfinity(p.X) || double.IsNaN(p.Y) || double.IsInfinity(p.Y))
                continue;

            outMinX = Math.Min(outMinX, p.X);
            outMinY = Math.Min(outMinY, p.Y);
            outMaxX = Math.Max(outMaxX, p.X);
            outMaxY = Math.Max(outMaxY, p.Y);
        }
    }

    private void FindPoints()
    {
        points = CollectPointsFromEntities(Entities);
    }

    private List<Vector3> CollectPointsFromEntities(IEnumerable<EntityObject> entities)
    {
        var pts = new List<Vector3>();

        foreach (var obj in entities)
        {
            switch (obj)
            {
                case Line line:
                    pts.Add(line.StartPoint);
                    pts.Add(line.EndPoint);
                    break;

                case Circle circ:
                    pts.Add(circ.Center);
                    double r = circ.Radius;
                    pts.Add(new Vector3(circ.Center.X + r, circ.Center.Y, circ.Center.Z));
                    pts.Add(new Vector3(circ.Center.X - r, circ.Center.Y, circ.Center.Z));
                    pts.Add(new Vector3(circ.Center.X, circ.Center.Y + r, circ.Center.Z));
                    pts.Add(new Vector3(circ.Center.X, circ.Center.Y - r, circ.Center.Z));
                    break;

                case Arc arc:
                    pts.AddRange(GetArcExtremes(arc));
                    break;

                case Ellipse ellipse:
                    pts.AddRange(GetEllipseSamplePoints(ellipse, 16));
                    break;

                default:
                    // TODO: добавить обработку Polyline, LwPolyline, Spline и т.п.
                    break;
            }
        }

        return pts;
    }

    private IEnumerable<Vector3> GetArcExtremes(Arc arc)
    {
        var list = new List<Vector3>();
        double r = arc.Radius;
        var c = arc.Center;

        double sa = NormalizeAngle(arc.StartAngle);
        double ea = NormalizeAngle(arc.EndAngle);

        // конечные точки
        list.Add(PointOnCircle(c, r, sa));
        list.Add(PointOnCircle(c, r, ea));

        // cardinal angles — если попадают в сектор, нужно учитывать
        double[] cardinals = { 0, 90, 180, 270 };
        foreach (var a in cardinals)
        {
            if (IsAngleBetween(a, sa, ea))
                list.Add(PointOnCircle(c, r, a));
        }

        return list;
    }

    private Vector3 PointOnCircle(Vector3 c, double r, double angleDeg)
    {
        double rad = angleDeg * Math.PI / 180.0;
        return new Vector3(c.X + r * Math.Cos(rad), c.Y + r * Math.Sin(rad), c.Z);
    }

    private bool IsAngleBetween(double angle, double start, double end)
    {
        angle = NormalizeAngle(angle);
        start = NormalizeAngle(start);
        end = NormalizeAngle(end);
        if (start <= end) return angle >= start && angle <= end;
        return angle >= start || angle <= end; // crossing 0
    }

    // Сэмплируем эллипс по параметру, используя MajorAxis + ratio
    private IEnumerable<Vector3> GetEllipseSamplePoints(Ellipse ellipse, int samples = 16)
    {
        var list = new List<Vector3>();
        var c = ellipse.Center;
        var major = ellipse.MajorAxis;
        double ratio = 1.0;

        // Попытка получить отношение осей (netDxf называет это по-разному в разных версиях)
        var prop = ellipse.GetType().GetProperty("MinorAxisRatio") ?? ellipse.GetType().GetProperty("Ratio");
        if (prop != null)
        {
            var val = prop.GetValue(ellipse);
            if (val is double d) ratio = d;
        }

        var minor = new Vector3(-major * ratio, major * ratio, 0);

        for (int i = 0; i < samples; i++)
        {
            double t = 2.0 * Math.PI * i / samples;
            double x = c.X + major * Math.Cos(t) + minor.X * Math.Sin(t);
            double y = c.Y + major * Math.Cos(t) + minor.Y * Math.Sin(t);
            list.Add(new Vector3(x, y, c.Z));
        }

        return list;
    }

    public int CompareTo(Detail other)
    {
        if (other == null) return 1;
        return Area.CompareTo(other.Area);
    }
}
