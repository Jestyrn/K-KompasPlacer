using Microsoft.Win32;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TestModule;

namespace MainWindowsApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string ReaderPath;
        private string WriterPath;

        private bool ReaderChosen = false;
        private bool WriterChosen = false;

        private bool ReadyToNesting = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ChangeReaderPath(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Файлы чертежей (*.dxf)|*.dxf";
            fileDialog.Title = "Выбирите файл формата DXF";

            var result = fileDialog.ShowDialog();
            if (result == true)
                ReaderPath = fileDialog.FileName;

            FileName.Text = System.IO.Path.GetFileName(ReaderPath);
            ReaderChosen = true;

            CheckState();
        }

        private void ChangeWriterPath(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog();
            folderDialog.Title = "Укажите путь для сохранения";

            var result = folderDialog.ShowDialog();
            if (result == true)
                WriterPath = folderDialog.FolderName;

            SaveFolder.Text = System.IO.Path.GetFileName(WriterPath);
            WriterChosen = true;

            CheckState();
        }

        private void RealSize_TextChanged(object sender, RoutedEventArgs e)
        {
            CurrentSize.Text = RealSize.Text.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // BorderPading ListPading DetailsPading
            if (ReaderChosen)
            {
                double minimumWidth = double.TryParse(MinSize.Text.Split(" : ")[0], out double a) ? a : 10000;
                double minimumHeight = double.TryParse(MinSize.Text.Split(" : ")[1], out a) ? a : 10000;

                double currentWidth, currentHeight;
                if (RealSize.Text.Contains(" : "))
                {
                    currentWidth = double.TryParse(RealSize.Text.Split(" : ")[0], out a) ? a : minimumWidth;
                    currentHeight = double.TryParse(RealSize.Text.Split(" : ")[1], out a) ? a : minimumHeight;
                }
                else
                {
                    throw new Exception("Размеры не валидны\n" +
                        "Отсутствует сеппаратор между шириной и высотой (\" : \")");
                }

                if (((currentWidth >= minimumWidth) && (currentHeight >= minimumHeight)) || ((currentWidth >= minimumHeight) && (currentHeight >= currentWidth)))
                {
                    double pading = double.TryParse(BorderPading.Text, out a) ? a : 0;
                    double listPading = double.TryParse(ListPading.Text, out a) ? a : 0;
                    double detailPading = double.TryParse(DetailsPading.Text, out a) ? a : 0;

                    Adapter.StartWpf(ReaderPath, WriterPath, pading, listPading, detailPading);
                }
                else
                {
                    MessageBox.Show("Размеры оказаль некоректными");
                }

            }
        }

        private async void CheckState()
        {
            if (ReaderChosen && WriterChosen)
            {
                MessageBox.Show("Готовятся данные для отображения, пожалуйста подождите");
                var adapter = await Task.Run(() => new Adapter(TypeOfAdapter.WPF, ReaderPath));

                await CheckStateAsync();
                UpdateUIWithData();
                ReadyToNesting = true;
            }
        }

        private async Task CheckStateAsync()
        {
            while (!DataForWpf.Checker)
            {
                await Task.Delay(2500);
            }
        }

        private void UpdateUIWithData()
        {
            DetailsCount.Text = DataForWpf.DetailsCount.ToString();
            MinSize.Text = DataForWpf.MinSize.ToString();
            CurrentSize.Text = MinSize.Text.Clone().ToString();
            MinCount.Text = DataForWpf.MinCount.ToString();
            RealSize.Text = MinSize.Text;
        }

        private void RealSize_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }

    //public class MyViewModel : INotifyPropertyChanged
    //{
    //    private string myValue;
    //    public string MyValue
    //    {
    //        get => myValue;
    //        set
    //        {
    //            if (myValue != value)
    //            {
    //                myValue = value;
    //                OnPropertyChanged(nameof(MyValue));
    //            }
    //        }
    //    }

    //    public event PropertyChangedEventHandler PropertyChanged;
    //    protected virtual void OnPropertyChanged(string propertyName) =>
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //}
}