using netDxf;
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using System.Runtime.CompilerServices;
using TestModule;
using TextInfo = TestModule.TextInfo;

internal class Program
{
    private static DxfDocument DXF = new DxfDocument();
    private static List<TextInfo> Texts;
    private static List<TextInfo> TextsInfo;
    private static List<Detail> Details = new List<Detail>();
    private static string Path = @"C:\Users\mrbug\OneDrive\Рабочий стол\Developers\K-Kompas\Kwork.dxf";
    private static string PathCheck = @"C:\Users\mrbug\OneDrive\Рабочий стол\Developers\K-Kompas\ForCheck.dxf";

    private static void Main(string[] args)
    {
        StartNewTest();
    }

    private static void StartNewTest()
    {
        //SetReady.Setup(Path); // почистит от пробелов и лишних символов в Entities.Text
        //TextsInfo = new List<TextInfo>(TextProcessor.GetCounterText(Path)); // парсинг текста (с шт)
        //Texts = new List<TextInfo>(TextProcessor.GetText(Path)); // парсинг текста
        Details = new List<Detail>(DetailsProcessor.GetAllDetails(PathCheck)); // парсинг деталей
        //Details = Details.OrderByDescending(p => p.Area) // сортировка деталей по увелечению
        //    .ThenByDescending(p => p.Area)
        //    .ThenBy(p => p.Entities.Count)
        //    .ToList();

        // --- //

        // Очистка от лишних прямоугольников
        for (int i = 0; i < Details.Count; i++)
        {
            if (Details[i].Entities.Count <= 6)
            {
                Details.RemoveAt(i);
                i--;
            }
        }

        // Добавление первых 200 элементов
        for (int i = 0; i < 200; i++)
            DXF.Entities.Add(Details[i].Insert);

        DXF.Save(PathCheck.Replace("ForCheck", "ForCheck-SavedBlocks"));

        // --- //

        // заебло
        
        // --- // 


        FrameBuilder builder = new FrameBuilder();
        double width, height;
        width = height = 20000000;
        builder.CreateFrame(width, height);

        FrameEngine engine = new FrameEngine(builder.Bounds);

        List<int> forRemove = new List<int>();
        BoundingBox neededPlace = new BoundingBox(0,0,0,0);

        bool createNewFrame = true;
        while (Details.Count != 0)
        {
            foreach (var detail in Details)
            {
                if (engine.FreeArea < detail.Bounds.Area)
                {
                    builder.CreateFrame(width, height);
                    engine.UpdateFrame(builder.Bounds);
                    break;
                }

                if (engine.TryPlaceDetail(detail, out bool needRotate, out neededPlace))
                {
                    if (needRotate)
                    {
                        detail.RotateDetail(90);
                        detail.MoveDetail(neededPlace.MinX, neededPlace.MinY);
                    }
                    else
                    {
                        detail.MoveDetail(neededPlace.MinX, neededPlace.MinY);
                    }

                    forRemove.Add(Details.IndexOf(detail));
                }
            }

            for (int i = 0; i < forRemove.Count; i++)
            {
                // check (dont remove if forRemove.Count == 0)
                Details.RemoveAt(forRemove[i] - i);
            }
        }

        DXF = new DxfDocument();

        foreach (var item in Details)
        {
            DXF.Entities.Add(item.Insert);
        }

        DXF.Save(PathCheck.Replace("ForCheck", "Resul-WantToSee"));
    }
}