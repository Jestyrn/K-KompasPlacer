namespace MainForm
{
    public partial class MainForm : Form
    {
        private string ReaderPath;
        private string WriterPath;

        public MainForm()
        {
            InitializeComponent();
        }

        private void ChousePath_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "�������� DXF ����";
            ofd.Filter = "����� ������� (*.dxf)|*.dxf";
            DialogResult result = ofd.ShowDialog();

            if (result == DialogResult.OK)
                ReaderPath = ofd.FileName;

            WidthTextBox.Enabled = true;
            HeightTextBox.Enabled = true;
            PadingTextBox.Enabled = true;
            ListPadingTextBox.Enabled = true;
            DetailsPadingTextBox.Enabled = true;

            SaveSettingsButton.Enabled = true;
            CalculateButton.Enabled = true;
            
            // ������� �������
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "���� ��� ����������";
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK)
                WriterPath = fbd.SelectedPath;
        }
    }
}
