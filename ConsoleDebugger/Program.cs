using netDxf;
using TestModule;
using TextInfo = TestModule.TextInfo;

internal class Program
{
    private static DxfDocument DXF;
    private static List<TextInfo> Texts;
    private static List<TextInfo> TextsInfo;
    private static List<Detail> Details;
    private static string Path = @"C:\Users\mrbug\OneDrive\Рабочий стол\Developers\K-Kompas\Kwork_Example.dxf";

    private static void Main(string[] args)
    {
        StartNewTest();
    }

    private static void StartNewTest()
    {
        //SetReady.Setup(Path); // почистит от пробелов и лишних символов в Entities.Text
        //TextsInfo = new List<TextInfo>(TextProcessor.GetCounterText(Path)); // парсинг текста (с шт)
        //Texts = new List<TextInfo>(TextProcessor.GetText(Path)); // парсинг текста
        //Details = new List<Detail>(DetailsProcessor.GetAllDetails(Path));

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

        // Создать рамку

        // Начать строить / рассчитывать
    }
}