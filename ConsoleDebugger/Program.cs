using netDxf;
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using System.Runtime.CompilerServices;
using TestModule;

internal class Program
{
    private static DxfDocument DXF = new DxfDocument();
    private static List<Detail> Details = new List<Detail>();
    private static string Path = @"C:\Users\mrbug\OneDrive\Рабочий стол\Developers\K-Kompas\Kwork.dxf";
    private static string PathSimple = @"C:\Users\mrbug\OneDrive\Рабочий стол\Developers\K-Kompas\SimpleKwork.dxf";
    private static string PathCheck = @"C:\Users\mrbug\OneDrive\Рабочий стол\Developers\K-Kompas\ForCheck.dxf";

    private static void Main(string[] args)
    {
        StartNewTest();
    }

    private static void StartNewTest()
    {
        //SetReady.Setup(Path); // почистит от пробелов и лишних символов в Entities.Text
        Details = new List<Detail>(DetailsProcessor.GetAllDetails(PathSimple)); // парсинг деталей
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

        // Поворот широкой стороной
        foreach (var detail in Details)
            if (detail.Width < detail.Height)
                detail.RotateDetail(90);

        // Размещение в ряд
        int lastx = -2000;
        int step = 100;
        foreach (var detail in Details)
        {
            detail.MoveDetail(lastx + step, -300);
            lastx = (int)detail.Bounds.MaxX;
        }

        // Добавление первых 200 элементов
        for (int i = 0; i < Details.Count; i++)
        {
            DXF.Entities.Add(Details[i].Insert);
            for (int j = 0; j < Details[i].BoundsDXF.Count; j++)
            {
                DXF.Entities.Add(Details[i].BoundsDXF[j]);
            }
        }

        DXF.Save(PathSimple);


        // проверка работы размещения
        var controller = new FrameController(Details, 2000, 2000, 500);
        List<Insert> ins = controller.TakeFilled();

        DXF = new DxfDocument();
        foreach (var item in ins)
            DXF.Entities.Add((EntityObject)item.Clone());

        DXF.Save(PathCheck.Replace("ForCheck", "ForCheck-Replace"));
    }
}