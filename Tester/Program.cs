using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using System.Drawing;

public class Program
{
    private static List<Detail> details = new List<Detail>();
    // Деталь на полигоны
    // Полотно - проверить как было(гильотина), проверить "вычитание", добавить генетический алгоритм

    public static void Main(string[] args)
    {
        string readPath = @"C:\Users\mrbug\OneDrive\Рабочий стол\Developers\K-Kompas\SimpleKwork.dxf";
        details = DetailsProcessor.GetAllDetails(readPath);

        Console.WriteLine("Прочтено");

        DxfDocument dxf = new DxfDocument();

        Console.WriteLine("Деталь 1\n");
        foreach (var item in details[0].Contour)
            Console.WriteLine($"x:{item.X} y:{item.Y} ");
        foreach (var item in details[0].Entities)
            dxf.Entities.Add((EntityObject)item.Clone());

        Console.WriteLine("Деталь 2\n");
        foreach (var item in details[1].Contour)
            Console.WriteLine($"x:{item.X} y:{item.Y} ");
        foreach (var item in details[1].Entities)
            dxf.Entities.Add((EntityObject)item.Clone());

        Console.WriteLine("Деталь 3\n");
        foreach (var item in details[2].Contour)
            Console.WriteLine($"x:{item.X} y:{item.Y} ");
        foreach (var item in details[2].Entities)
            dxf.Entities.Add((EntityObject)item.Clone());

        dxf.Save(readPath.Replace("SimpleKwork", "result"));
    }
}

public class Detail
{
    public List<EntityObject> Entities { get; private set; }
    public List<Vector2> Contour { get; set; } = new List<Vector2>();

    public Detail(List<EntityObject> objects)
    {
        Entities = new List<EntityObject>(objects);
        Contour = ContourHelper.FindContour(Entities);
    }
}

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
            foreach (var entity in block.Entities.Where(x => x.Type == EntityType.Line | x.Type == EntityType.Arc | x.Type == EntityType.Circle | x.Type == EntityType.Ellipse))
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

public static class ContourHelper
{
    public static List<Vector2> FindContour(List<EntityObject> ents)
    {
        // Алгоритм:
        // 1. ents - Список всех элементов(линии, дуги, крги, овалы)
        // 2. Сдлеать элемент = список точек
        // 2.1 Линия - 2 точки
        // 2.2 Дуга - 3 точки
        // 3. Создать граф (элемент, все соседние элементы)
        // 4. Обойти граф в поисках контура
        // 5. Вернуть контур

        List<Vector2> result = new List<Vector2>();
        List<List<Vector2>> elements = FindElements(ents);

        // Если начало текущего элемента является концом другого элемента, то добавить в контур "другой элемент"
        // Иначе пропустить
        // Продолжать, пока не пройдем все элементы

        result = BuildContour(elements);

        return result;
    }

    public static List<Vector2> BuildContour(List<List<Vector2>> elements, double epsilon = 1e-6)
    {
        List<Vector2> result = new List<Vector2>();

        if (elements == null || elements.Count == 0)
            return result;

        // Берём первый элемент как стартовый
        List<Vector2> sorted = new List<Vector2>();

        foreach (var elem in elements)
        {
            sorted = elem.OrderByDescending(x => x.X).ToList();
        }

        List<Vector2> current = sorted;
        result.AddRange(current);
        elements.RemoveAt(0);

        bool progress = true;

        while (elements.Count > 0 && progress)
        {
            progress = false;
            Vector2 lastPoint = result[result.Count - 1];

            for (int i = 0; i < elements.Count; i++)
            {
                var elem = elements[i];
                Vector2 start = elem[0];
                Vector2 end = elem[elem.Count - 1];

                if (Near(lastPoint, start, epsilon))
                {
                    result.AddRange(elem.Skip(1));
                    elements.RemoveAt(i);
                    progress = true;
                    break;
                }
                else if (Near(lastPoint, end, epsilon))
                {
                    elem.Reverse();
                    result.AddRange(elem.Skip(1));
                    elements.RemoveAt(i);
                    progress = true;
                    break;
                }
            }
        }

        return result;
    }

    private static bool Near(Vector2 a, Vector2 b, double eps)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        // сравниваем квадрат расстояния, чтобы не вызывать дорогостоящий sqrt
        return (dx * dx + dy * dy) <= (eps * eps);
    }

    private static List<List<Vector2>> FindElements(List<EntityObject> ents)
    {
        List<List<Vector2>> elements = new List<List<Vector2>>();

        foreach (var item in ents)
        {
            switch (item)
            {
                case Line line:
                    elements.Add(new List<Vector2> { new Vector2(line.StartPoint.X, line.StartPoint.Y), new Vector2(line.EndPoint.X, line.EndPoint.Y) });
                    break;

                case Arc arc:
                    elements.Add(new List<Vector2> { FindAxle(arc.Center, arc.Radius, arc.StartAngle), FindAxle(arc.Center, arc.Radius, arc.EndAngle) });
                    break;

                case Circle circle:
                    Vector2 up = new Vector2(circle.Center.X, circle.Center.Y + circle.Radius);
                    Vector2 down = new Vector2(circle.Center.X, circle.Center.Y - circle.Radius);
                    Vector2 left = new Vector2(circle.Center.X - circle.Radius, circle.Center.Y);
                    Vector2 right = new Vector2(circle.Center.X + circle.Radius, circle.Center.Y);

                    elements.Add(new List<Vector2> { up, down, left, right });
                    break;
            }
        }
        return elements;
    }

    private static Vector2 FindAxle(Vector3 center, double radius, double angle)
    {
        Vector2 result = new Vector2();
        double rad = angle * Math.PI / 180;
        result.X = center.X + radius * Math.Cos(rad);
        result.Y = center.Y + radius * Math.Sin(rad);
        return result;
    }
}

public static class ContourUtils
{
    public static List<Vector2> BuildOuterContourFromElements(List<List<Vector2>> elements, double eps = 1e-5)
    {
        if (elements == null || elements.Count == 0)
            return new List<Vector2>();

        // 1) Собираем множество рёбер (неупорядоченные) + граф смежности
        var pointKey = new Func<Vector2, string>(p => $"{Math.Round(p.X / eps)},{Math.Round(p.Y / eps)}");

        var adj = new Dictionary<string, List<string>>();          // вершина -> соседи (по ключам)
        var key2point = new Dictionary<string, Vector2>();         // ключ -> исходная точка
        var edges = new HashSet<string>();                         // неориентированные рёбра (для учёта "неиспользованных")

        void AddVertex(Vector2 p)
        {
            var k = pointKey(p);
            if (!adj.ContainsKey(k))
            {
                adj[k] = new List<string>();
                key2point[k] = p;
            }
        }

        // добавление ребра (u-v)
        void AddEdge(Vector2 u, Vector2 v)
        {
            var ku = pointKey(u);
            var kv = pointKey(v);
            if (ku == kv) return;

            AddVertex(u);
            AddVertex(v);

            adj[ku].Add(kv);
            adj[kv].Add(ku);

            edges.Add(UndirectedEdgeKey(ku, kv));
        }

        foreach (var poly in elements)
        {
            if (poly == null || poly.Count < 2) continue;
            for (int i = 0; i < poly.Count - 1; i++)
                AddEdge(poly[i], poly[i + 1]);
        }

        if (edges.Count == 0)
            return new List<Vector2>();

        // 2) Вынимаем ВСЕ замкнутые кольца (и открытые пути — на всякий случай)
        var unused = new HashSet<string>(edges);
        var loops = new List<List<string>>();
        var openPaths = new List<List<string>>();

        while (unused.Count > 0)
        {
            // стартуем с произвольного ещё неиспользованного ребра
            var anyEdge = unused.First();
            (string a, string b) = SplitEdgeKey(anyEdge);

            // попробуем построить путь, идя из a к b, а затем продолжая дальше
            var pathKeys = TracePath(a, b, adj, unused);
            // Проверим замкнутость
            if (pathKeys.Count > 2 && pathKeys.First() == pathKeys.Last())
                loops.Add(pathKeys);
            else
                openPaths.Add(pathKeys);
        }

        // 3) Если есть петли — выберем самую «крупную» по площади
        if (loops.Count > 0)
        {
            double bestAbsArea = double.NegativeInfinity;
            List<string> bestLoop = null;

            foreach (var loop in loops)
            {
                var pts = KeysToPoints(loop, key2point, closeIfNeeded: false);
                double area = SignedArea(pts);
                double absArea = Math.Abs(area);
                if (absArea > bestAbsArea)
                {
                    bestAbsArea = absArea;
                    bestLoop = loop;
                }
            }

            var outer = KeysToPoints(bestLoop, key2point, closeIfNeeded: true);
            // Нормализуем ориентацию (CCW — положительная площадь)
            if (SignedArea(outer) < 0) outer.Reverse();
            return CompressCollinear(outer, eps);
        }

        // 4) Fallback: замкнутых нет — возьмём самый длинный открытый путь
        var longest = openPaths.OrderByDescending(p => p.Count).First();
        var polyline = KeysToPoints(longest, key2point, closeIfNeeded: false);
        return CompressCollinear(polyline, eps);
    }

    // -------- helpers --------

    private static string UndirectedEdgeKey(string k1, string k2)
        => string.CompareOrdinal(k1, k2) <= 0 ? $"{k1}|{k2}" : $"{k2}|{k1}";

    private static (string a, string b) SplitEdgeKey(string key)
    {
        var i = key.IndexOf('|');
        return (key[..i], key[(i + 1)..]);
    }

    // Проход по рёбрам: начиная с (u-v), идём, пока есть неиспользованные рёбра; откусываем встречные рёбра из множества unused
    private static List<string> TracePath(string u, string v, Dictionary<string, List<string>> adj, HashSet<string> unused)
    {
        var path = new List<string> { u, v };
        string prev = u;
        string cur = v;

        // пометить стартовое ребро использованным
        unused.Remove(UndirectedEdgeKey(u, v));

        // идём вперёд
        while (true)
        {
            // ищем среди соседей cur того, кто образует ещё неиспользованное ребро
            string next = null;
            foreach (var nb in adj[cur])
            {
                if (nb == prev) continue;
                var ekey = UndirectedEdgeKey(cur, nb);
                if (unused.Contains(ekey))
                {
                    next = nb;
                    unused.Remove(ekey);
                    break;
                }
            }

            if (next == null)
            {
                // попробуем «замкнуться» назад в начало пути
                var backKey = UndirectedEdgeKey(cur, path[0]);
                if (unused.Contains(backKey))
                {
                    unused.Remove(backKey);
                    path.Add(path[0]); // замкнули
                }
                break;
            }

            path.Add(next);
            prev = cur;
            cur = next;

            // если замкнули — останавливаемся
            if (cur == path[0])
                break;
        }

        return path;
    }

    private static List<Vector2> KeysToPoints(List<string> keys, Dictionary<string, Vector2> dict, bool closeIfNeeded)
    {
        var pts = new List<Vector2>(keys.Count);
        foreach (var k in keys)
            pts.Add(dict[k]);

        if (closeIfNeeded && (pts.Count < 2 || !Near(pts[0], pts[^1], 1e-12)))
            pts.Add(pts[0]);

        return pts;
    }

    private static double SignedArea(List<Vector2> poly)
    {
        if (poly.Count < 3) return 0;
        double s = 0;
        int n = poly.Count;
        for (int i = 0, j = n - 1; i < n; j = i++)
            s += (poly[j].X * poly[i].Y - poly[i].X * poly[j].Y);
        return 0.5 * s;
    }

    private static bool Near(Vector2 a, Vector2 b, double eps)
    {
        double dx = a.X - b.X, dy = a.Y - b.Y;
        return (dx * dx + dy * dy) <= (eps * eps);
    }

    private static List<Vector2> CompressCollinear(List<Vector2> pts, double eps)
    {
        if (pts.Count <= 2) return pts;

        bool closed = Near(pts[0], pts[^1], eps);
        int end = closed ? pts.Count - 1 : pts.Count;

        var result = new List<Vector2>();
        result.Add(pts[0]);

        for (int i = 1; i < end - 1; i++)
        {
            var a = result[^1];
            var b = pts[i];
            var c = pts[i + 1];

            if (!IsCollinear(a, b, c, eps))
                result.Add(b);
        }
        result.Add(pts[end - 1]);

        if (closed)
        {
            // закрыть итог, сохранив отсутствие дублей на стыке
            if (!Near(result[0], result[^1], eps))
                result.Add(result[0]);
        }

        return result;

        static bool IsCollinear(Vector2 a, Vector2 b, Vector2 c, double eps)
        {
            double area2 = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            return Math.Abs(area2) <= eps;
        }
    }
}
