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

        dxf.Save(readPath.Replace("SimpleKwork", "result"));
    }
}

public class Detail
{
    public List<EntityObject> Entities { get; private set; }
    public List<Vector2> Contour { get; set; } = new List<Vector2>();

    private List<Vector3> points = new List<Vector3>();

    public Detail(List<EntityObject> objects)
    {
        Entities = new List<EntityObject>(objects);
        CompletePrepairs();
        Contour = ContourHelper.FindContour(Entities);
    }

    private void CompletePrepairs()
    {
        FindPoints();
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

                case Ellipse ellipse:
                    points.Add(ellipse.Center);
                    break;
            }
        }
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
    // Основной метод
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

        // Если начало первого элемента является концом другого элемента, то добавить в контур
        // Иначе пропустить
        // Продолжать, пока не пройдем все элементы
        // Если не удалось пройти все элементы, то вернуть пустой список
        // Если удалось, то вернуть контур

        return result;
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
                    elements.Add(new List<Vector2> { new Vector2(arc.Center.X, arc.Center.Y), FindAxle(arc.Center, arc.Radius, arc.StartAngle), FindAxle(arc.Center, arc.Radius, arc.EndAngle) });
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
