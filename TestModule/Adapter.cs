using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestModule
{
    public class Adapter
    {
        public TypeOfAdapter type;

        public Adapter(TypeOfAdapter type)
        {
            this.type = type;

            switch (type)
            {
                case TypeOfAdapter.Console:
                    ConsoleExecute();
                    break;

                case TypeOfAdapter.WPF:
                    WpfExecute();
                    break;

                case TypeOfAdapter.Forms:
                    FormsExecute();
                    break;
            }
        }

        private void FormsExecute()
        {
            throw new NotImplementedException();
        }

        private void WpfExecute()
        {
            throw new NotImplementedException();
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
        }
    }

    public enum TypeOfAdapter
    {
        Console,
        WPF,
        Forms
    }
}
