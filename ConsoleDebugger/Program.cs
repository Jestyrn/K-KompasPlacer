using netDxf;
using netDxf.Collections;
using netDxf.Entities;
using TestModule;
using TextInfo = TestModule.TextInfo;

internal class Program
{
    private static DxfDocument DXF = new DxfDocument();
    private static List<TextInfo> Texts;
    private static List<TextInfo> TextsInfo;
    private static List<Details> Details;
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
        Details = new List<Details>(DetailsProcessor.GetAllDetails(Path)); // парсинг деталей
        Details = Details.OrderByDescending(p => p.Area) // сортировка деталей по увелечению
            .ThenByDescending(p => p.Area)
            .ThenBy(p => p.Entities.Count)
            .ToList();

        int cnt = 0;
        foreach (var detail in Details)
        {
            Console.Write($"({cnt}: area {detail.Area}, ents {detail.Entities.Count}) - ");
            cnt++;
            if ( cnt % 5 == 0)
                Console.WriteLine();  
        }

        int lenght = 0;
        for (int i = 0; i < 10; i++)
        {
            lenght += (int)Details[i].Width + 20;
            Details[i].MoveDetail(lenght, 0);
            
            foreach (var item in Details[i].Entities)
            {
                DXF.Entities.Add((EntityObject)item.Clone());
            }

            foreach (var item in Details[i].BoundsDXF)
            {
                DXF.Entities.Add((EntityObject)item.Clone());
            }
        }

        DXF.Save(Path.Replace("Kwork", "TestLook")); 

        // 1. Найти/Построить рамку для размещения
        // 2. Сделать проверку размещения
        //      а) Коллизия с другими фигурами
        //      б) Коллизия с рамкой
        //      в) Деталь не вышла за рамку
        // 3.1. Все в порядке - размещаем
        // 3.2. Все в порядке - создаем новую рамку

        // Решено применение слующего паттерна расоположения
        // 1. располагать в стиле Гильятина(каттинг)
        // 2. рассмтривать оптимальность размещения при помощий "Поколений"
        // 3. зафиксировать лучший результат поколения (количество поколений подберется в ручную, чтобы найти оптимальную скорость)

        NewTestsGPT ss = new NewTestsGPT();
        ss.TryToDo(@"C:\Users\mrbug\OneDrive\Рабочий стол\Developers\K-Kompas\Edited.dxf");
    }
}