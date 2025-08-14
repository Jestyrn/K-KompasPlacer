using MainWindowsApp.Model;
using MainWindowsApp.VeiwModel;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TestModule;

namespace MainWindowsApp
{
    public partial class MainWindow : Window
    {
        private string ReaderPath;
        private string WriterPath;

        private bool ReaderChosen = false;
        private bool WriterChosen = false;

        private bool ReadyToNesting = false;
        private AppViewModel ViewModel;
        private DataController Controller;

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new AppViewModel();
            Controller = new DataController();
            DataContext = ViewModel;
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

            RealSize.IsEnabled = false;
            BorderPading.IsEnabled = false;
            ListPading.IsEnabled = false;
            DetailsPading.IsEnabled = false;
            StartButton.IsEnabled = false;

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

            RealSize.IsEnabled = false;
            BorderPading.IsEnabled = false;
            ListPading.IsEnabled = false;
            DetailsPading.IsEnabled = false;
            StartButton.IsEnabled = false;

            SaveFolder.Text = System.IO.Path.GetFileName(WriterPath);
            WriterChosen = true;

            CheckState();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;

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
                    string text = "Размеры не валидны\n" +
                        "Отсутствует сеппаратор между шириной и высотой (\" : \")";

                    MessageBox.Show(text, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                    throw new Exception(text);
                }

                if (((currentWidth >= minimumWidth) && (currentHeight >= minimumHeight)) || ((currentWidth >= minimumHeight) && (currentHeight >= currentWidth)))
                {
                    double pading = double.TryParse(BorderPading.Text, out a) ? a : 0;
                    double listPading = double.TryParse(ListPading.Text, out a) ? a : 0;
                    double detailPading = double.TryParse(DetailsPading.Text, out a) ? a : 0;

                    string size = RealSize.Text;
                    string insPad = BorderPading.Text;
                    string detPad = DetailsPading.Text;
                    string fraPad = ListPading.Text;

                    await Task.Run(() => Controller.ExecuteNesting(size, insPad, detPad, fraPad));
                }
                else
                {
                    MessageBox.Show("Размеры оказаль некоректными");
                    return;
                }
            }

            StartButton.IsEnabled = true;
        }

        private async void CheckState()
        {
            if (ReaderChosen && WriterChosen)
            {
                MessageBox.Show("Готовятся данные для отображения, пожалуйста подождите.\n\nПосле окончания элементы управления должны стать активны.", "Ожидайте.", MessageBoxButton.OK, MessageBoxImage.Information);
                await Task.Factory.StartNew(() => Controller.CalculateDetails(ReaderPath, WriterPath));

                UpdateUIWithData();

                RealSize.IsEnabled = true;
                BorderPading.IsEnabled = true;
                ListPading.IsEnabled = true;
                DetailsPading.IsEnabled = true;
                StartButton.IsEnabled = true;

                ReadyToNesting = true;
            }
        }

        private void UpdateUIWithData()
        {
            ViewModel.Frame.DetailsCount = Controller.DetailsCount;
            ViewModel.Frame.MinSize = Controller.MinSize;
            ViewModel.Frame.MinFrames = Controller.FramesCount;
            ViewModel.InputView.FrameSize = Controller.MinSize;
        }
    }
}