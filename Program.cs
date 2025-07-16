using netDxf;
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using System.Drawing;
using System.Reflection.Metadata;
using TesterModule;

internal class Program
{
    private static DxfDocument DXF;
    private static List<TextInfo> Texts;
    private static List<TextInfo> TextsInfo;
    private static string Path = @"C:\Users\mrbug\OneDrive\Рабочий стол\Developers\K-Kompas\Kwork_Example.dxf";

    private static void Main(string[] args)
    {
        StartNewTest();
    }

    private static void StartNewTest()
    {
        SetReady.Setup(Path); // почистит от пробелов и лишних символов в Entities.Text
        TextsInfo = new List<TextInfo>(TextProcessor.GetCounterText(Path)); // парсинг текста (с шт)
        Texts = new List<TextInfo>(TextProcessor.GetText(Path)); // парсинг текста

        //NewTestsGPT.TryToDo(); // парсинг фигур

        // Отсеивание фигур (+ упрощение к набору прямоугольников)
        // Создать рамку

        // Начать строить / рассчитывать
    }
}