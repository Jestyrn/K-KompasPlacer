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

namespace MainWindowsApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            box.Content = "Перенесите сюда файл .DXF\nИли укажите путь нажав на кнопку";
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

        private void DragEnter(object sender, RoutedEventArgs e)
        {
            box.Content = "Что-то появилось";
        }

        private void DragLeave(object sender, RoutedEventArgs e)
        {
            box.Content = "Что-то пропало";
        }

        private void DragDrop(object sender, RoutedEventArgs e)
        {
            box.Content = "Что-то съедено";
        }
    }
}