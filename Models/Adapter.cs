using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TestModule
{
    public class Adapter
    {
        public TypeOfAdapter type;
        private string path;

        public Adapter(TypeOfAdapter type, string path = "")
        {
            this.type = type;
            this.path = path;

            switch (type)
            {
                case TypeOfAdapter.Console:
                    ConsoleExecute();
                    break;

                case TypeOfAdapter.WPF:
                    WpfExecute(path);
                    break;
            }
        }

        private void ConsoleExecute()
        {
            DxfDocument dxf = new DxfDocument();
            string reader = @"C:\Users\mrbug\OneDrive\Рабочий стол\Developers\K-Kompas\SimpleKwork.dxf";
            string writer = @"C:\Users\mrbug\OneDrive\Рабочий стол\Developers\K-Kompas\ForCheck.dxf";

            List<Detail> Details = new List<Detail>(DetailsProcessor.GetAllDetails(reader));

            for (int i = 0; i < Details.Count; i++)
            {
                if (Details[i].Entities.Count <= 6)
                {
                    Details.RemoveAt(i);
                    i--;
                }

                if (Details[i].Width < Details[i].Height)
                {
                    Details[i].RotateDetail(90);
                }
            }

            double width = 1550, height = 1000;
            Frame frame = new Frame(new Vector2(0, 0), width, height);
            var controller = new FrameController(Details, width, height, 1000);
            List<Insert> ins = controller.TakeFilled();

            dxf = new DxfDocument();
            foreach (var item in ins)
                dxf.Entities.Add((EntityObject)item.Clone());

            dxf.Save(writer.Replace("ForCheck", "ForCheck-Replace"));
            Console.WriteLine($"Complete work {type}");
        }

        private async Task WpfExecute(string path)
        {
            DataForWpf data = new DataForWpf(path);
            await data.Calculate();
        }

        public static async Task StartWpf(string reader, string writer, double pading, double listPading, double detailPading)
        {
            DxfDocument dxf = new DxfDocument();

            List<Detail> Details = new List<Detail>(DetailsProcessor.GetAllDetails(reader));

            for (int i = 0; i < Details.Count; i++)
            {
                if (Details[i].Entities.Count <= 6)
                {
                    Details.RemoveAt(i);
                    i--;
                }

                Details[i].AddPading(detailPading);

                if (Details[i].Width < Details[i].Height)
                {
                    Details[i].RotateDetail(90);
                }
            }

            Details = Details.OrderByDescending(x => x.Area).ThenBy(x => x.Width).ToList();

            double width = 1550, height = 1000;
            Frame frame = new Frame(new Vector2(0, 0), width, height);
            var controller = new FrameController(Details, width, height, (int)detailPading);
            List<Insert> ins = controller.TakeFilled();

            dxf = new DxfDocument();
            foreach (var item in ins)
                dxf.Entities.Add((EntityObject)item.Clone());

            dxf.Save(writer + "\\ReadyToUpload.dxf");
        }
    }

    public class DataForWpf
    {
        private static string Reader;
        public static bool Checker = false;

        public static string DetailsCount { get; private set; }
        public static string MinSize { get; private set; }
        public static string MinCount { get; private set; }

        public static List<Detail> Details { get; private set; }

        public DataForWpf(string reader)
        {
            Reader = reader;
        }

        public async Task Calculate()
        {
            DxfDocument dxf = new DxfDocument();

            List<Detail> Details = new List<Detail>(DetailsProcessor.GetAllDetails(Reader));
            double width = Details.Max(x => x.Width);
            double height = Details.Max(x => x.Height);
            double area = width * height;
            double sumArea = Details.Sum(x => x.Area);

            DetailsCount = Details.Count.ToString();
            MinSize = $"{(int)width + 1} : {(int)height + 1}";
            MinCount = $"{(int)(sumArea / area)}";
            Checker = true;
        }
    }

    public enum TypeOfAdapter
    {
        Console,
        WPF
    }
}