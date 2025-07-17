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

        // Начать склеивать или определять коорды / строить

        NewTestsGPT ss = new NewTestsGPT();
        ss.TryToDo(@"C:\Users\mrbug\OneDrive\Рабочий стол\Developers\K-Kompas\Edited.dxf");

        // Создать рамку

        // Начать строить / рассчитывать
    }
}