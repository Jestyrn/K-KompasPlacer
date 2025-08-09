using System.Data.Common;
using TestModule;

namespace MainForm
{
    public partial class MainForm : Form
    {
        private string _readerPath;
        private string _writerPath;

        public MainForm()
        {
            InitializeComponent();
            InitializeControlsState(enabled: false);
        }

        private void InitializeControlsState(bool enabled)
        {
            WidthTextBox.Enabled = enabled;
            HeightTextBox.Enabled = enabled;
            PadingTextBox.Enabled = enabled;
            ListPadingTextBox.Enabled = enabled;
            DetailsPadingTextBox.Enabled = enabled;
            CalculateButton.Enabled = enabled;
        }

        private async void ChoosePath_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Выберите DXF файл",
                Filter = "Файлы чертежа (*.dxf)|*.dxf"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            _readerPath = ofd.FileName;
            InitializeControlsState(enabled: true);
            CalculateButton.Enabled = false;

            try
            {
                MessageBox.Show("Готовятся данные для отображения");
                var adapter = await Task.Run(() => new Adapter(TypeOfAdapter.Forms, _readerPath));

                await CheckStateAsync();
                UpdateUIWithData();
                SetNumericUpDownLimits();
                UpdatePathLabel(ReaderPathLabel, _readerPath);
            }
            finally
            {
                CalculateButton.Enabled = true;
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
        }

        private void SetNumericUpDownLimits()
        {
            var sizeParts = MinSize.Text.Split(" : ");
            if (sizeParts.Length < 2) return;

            var minWidth = Convert.ToInt32(sizeParts[0]);
            var minHeight = Convert.ToInt32(sizeParts[1]);

            WidthTextBox.Minimum = minWidth - 1;
            WidthTextBox.Maximum = minWidth * 10;
            WidthTextBox.Value = minWidth;

            HeightTextBox.Minimum = minHeight - 1;
            HeightTextBox.Maximum = minHeight * 10;
            HeightTextBox.Value = minHeight;
        }

        private void SavePath_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog
            {
                Description = "Путь для сохранения"
            };

            if (fbd.ShowDialog() != DialogResult.OK)
                return;

            _writerPath = fbd.SelectedPath;
            UpdatePathLabel(WriterPathLabel, _writerPath);
        }

        private static void UpdatePathLabel(Control label, string path)
        {
            var fileName = Path.GetFileName(path);
            label.Text = fileName ?? string.Empty;
        }

        private void TextBox_ValueChanged(object sender, EventArgs e)
        {
            CurrentSize.Text = $"{WidthTextBox.Value} : {HeightTextBox.Value}";
        }
    }
}
