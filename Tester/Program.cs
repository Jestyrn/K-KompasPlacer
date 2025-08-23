using netDxf;
using netDxf.Entities;

public class Program
{
    private static DxfDocument dxf = new DxfDocument();
    // Деталь на полигоны
    // Полотно - проверить как было(гильотина), проверить "вычитание", добавить генетический алгоритм

    public static void Main(string[] args)
    {
        string readPath = @"C:\Users\mrbug\OneDrive\Рабочий стол\Developers\K-Kompas\SimpleKwork.dxf";
        dxf = DxfDocument.Load(readPath);

        Console.WriteLine("Прочтено");
    }
}