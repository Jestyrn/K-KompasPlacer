using netDxf;
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using TestModule;
using TextInfo = TestModule.TextInfo;

internal class Program
{
    private static DxfDocument DXF = new DxfDocument();
    private static List<TextInfo> Texts;
    private static List<TextInfo> TextsInfo;
    private static List<Detail> Details;
    private static string Path = @"C:\Users\mrbug\OneDrive\Рабочий стол\Developers\K-Kompas\Kwork.dxf";

    private static void Main(string[] args)
    {
        StartNewTest();
    }

    private static void StartNewTest()
    {
        //SetReady.Setup(Path); // почистит от пробелов и лишних символов в Entities.Text
        //TextsInfo = new List<TextInfo>(TextProcessor.GetCounterText(Path)); // парсинг текста (с шт)
        //Texts = new List<TextInfo>(TextProcessor.GetText(Path)); // парсинг текста
        //Details = new List<Details>(DetailsProcessor.GetAllDetails(Path)); // парсинг деталей
        //Details = Details.OrderByDescending(p => p.Area) // сортировка деталей по увелечению
        //    .ThenByDescending(p => p.Area)
        //    .ThenBy(p => p.Entities.Count)
        //    .ToList();

        // --- //
        var packer = new MaxRectsPacker(100, 100);

        // 2. Добавляем детали (ширина, высота)
        var parts = new List<Part>
        {
            new Part { Width = 30, Height = 40, Id = 1 },
            new Part { Width = 20, Height = 60, Id = 2 },
            new Part { Width = 50, Height = 30, Id = 3 }
        };

        // 3. Пробуем упаковать
        foreach (var part in parts.OrderByDescending(p => Math.Max(p.Width, p.Height)))
        {
            if (!packer.TryPlacePart(part))
            {
                Console.WriteLine($"Деталь {part.Id} не поместилась!");
            }
        }

        // 4. Выводим результат
        foreach (var placed in packer.PlacedParts)
        {
            Console.WriteLine(
                $"Деталь {placed.Part.Id}: ({placed.X}, {placed.Y}), " +
                $"Размер: {placed.Part.Width}x{placed.Part.Height}");
        }
        // --- //



        FrameBuilder framer = new FrameBuilder(100);
        List<Block> blocks = new List<Block> 
        {
            framer.Construct(100, 100)
        };

        foreach (var block in blocks)
            foreach (var ent in block.Entities)
                DXF.Entities.Add((EntityObject)ent.Clone());

        DXF.Save(Path.Replace("Kwork", "TestLook")); 

        // 1. Найти/Построить рамку для размещения
        // 2. Сделать проверку размещения
        //      а) Коллизия с другими фигурами
        //      б) Коллизия с рамкой
        //      в) Деталь не вышла за рамку
        // 3.1. Все в порядке - размещаем
        // 3.2. Все в порядке - создаем новую рамку

        NewTestsGPT ss = new NewTestsGPT();
        ss.TryToDo(@"C:\Users\mrbug\OneDrive\Рабочий стол\Developers\K-Kompas\Edited.dxf");
    }
}