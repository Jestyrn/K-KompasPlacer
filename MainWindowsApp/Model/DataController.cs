using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TestModule;

namespace MainWindowsApp.Model
{
    class DataController
    {
        public List<Detail> Details {  get; private set; }

        public string DetailsCount {  get; private set; }
        public string MinSize {  get; private set; }
        public string FramesCount {  get; private set; }
        public string FilePath {  get; set; }
        public string FolderPath {  get; set; }
        public string PadingInside {  get; set; }
        public string PadingFrame {  get; set; }
        public string PadingDetails {  get; set; }


        private double _insidePad;
        private double _framePad;
        private double _detailsPad;


        public DataController()
        {
            Details = new List<Detail>();
        }

        public bool CalculateDetails(string reader, string writer)
        {
            FilePath = reader;
            FolderPath = writer;

            Details = DetailsProcessor.GetAllDetails(reader);

            double width = Details.Max(x => x.Width);
            double height = Details.Max(x => x.Height);
            double area = width * height;
            double sumArea = Details.Sum(x => x.Area);

            DetailsCount = Details.Count.ToString();
            MinSize = $"{(int)width + 1} : {(int)height + 1}";
            FramesCount = $"{(int)(sumArea / area)}";

            return true;
        }

        public async Task<bool> ExecuteNesting(string size, string insidePading, string detailPading, string framePading)
        {
            if (double.TryParse(insidePading, out double insPadDouble))
            {
                PadingInside = insPadDouble.ToString();
            }
            else
            {
                AlertMessage(insidePading);
                insPadDouble = 0;
                PadingInside = "0";
            }
            _insidePad = insPadDouble;

            if (double.TryParse(detailPading, out double detPadDouble))
            {
                PadingDetails = detPadDouble.ToString();
            }
            else
            {
                AlertMessage(detailPading);
                detPadDouble = 0;
                PadingDetails = "0";
            }
            _detailsPad = detPadDouble;

            if (double.TryParse(framePading, out double framePadDouble))
            {
                PadingFrame = framePadDouble.ToString();
            }
            else
            {
                AlertMessage(framePading);
                framePadDouble = 0;
                PadingFrame = "0";
            }
            _framePad = framePadDouble;

            double.TryParse(size.Split(" : ")[0], out double width);
            double.TryParse(size.Split(" : ")[1], out double height);

            await MainExecute(width, height);
            return true;
        }

        private async Task MainExecute(double width, double height)
        {
            for (int i = 0; i < Details.Count; i++)
            {
                if (Details[i].Entities.Count <= 6)
                {
                    Details.RemoveAt(i);
                    i--;
                }

                Details[i].AddPading(_detailsPad);

                if (Details[i].Width < Details[i].Height)
                {
                    Details[i].RotateDetail(90);
                }
            }

            Details = Details.OrderByDescending(x => x.Area).ThenBy(x => x.Width).ToList();

            Frame frame = new Frame(new Vector2(0, 0), width, height);
            var controller = new FrameController(Details, width, height, (int)_framePad, (int)_insidePad);
            List<Insert> ins = controller.TakeFilled();

            if (controller.Ex.Length > 3)
            {
                MessageBox.Show(controller.Ex, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DxfDocument dxf = new DxfDocument();
            foreach (var item in ins)
                dxf.Entities.Add((EntityObject)item.Clone());

            var files = Directory.GetFiles(FolderPath);
            int cnt = 0;
            foreach (var file in files)
            {
                if (file.Contains("\\ReadyToUpload.dxf"))
                {
                    cnt++;
                }
            }
            string filename = cnt == 0 ? "\\ReadyToUpload.dxf" : $"\\ReadyToUpload{cnt}.dxf";

            dxf.Save(FolderPath + filename);
            MessageBox.Show("Работа завершена", "Завершено", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AlertMessage(string param)
        {
            MessageBox.Show("Не удалось конвертировать внутренний отступ" +
            "\nОжидаемый формат:\"0.00\"" +
            $"\nПришедший формат: {param}" +
            $"\nРабота продолжена с занчением: 0", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}