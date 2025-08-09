namespace MainForm
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            button3 = new Button();
            ChousePath = new Button();
            label15 = new Label();
            label1 = new Label();
            panel2 = new Panel();
            WriterPathLabel = new Label();
            ReaderPathLabel = new Label();
            MinCount = new Label();
            label8 = new Label();
            label17 = new Label();
            CurrentSize = new Label();
            label16 = new Label();
            label6 = new Label();
            MinSize = new Label();
            label4 = new Label();
            label10 = new Label();
            DetailsCount = new Label();
            label2 = new Label();
            panel3 = new Panel();
            HeightTextBox = new NumericUpDown();
            WidthTextBox = new NumericUpDown();
            CalculateButton = new Button();
            DetailsPadingTextBox = new TextBox();
            ListPadingTextBox = new TextBox();
            PadingTextBox = new TextBox();
            label11 = new Label();
            label12 = new Label();
            label20 = new Label();
            label13 = new Label();
            label14 = new Label();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)HeightTextBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)WidthTextBox).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ControlDark;
            panel1.Controls.Add(button3);
            panel1.Controls.Add(ChousePath);
            panel1.Controls.Add(label15);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(12, 12);
            panel1.Name = "panel1";
            panel1.Size = new Size(776, 65);
            panel1.TabIndex = 0;
            // 
            // button3
            // 
            button3.Location = new Point(146, 34);
            button3.Name = "button3";
            button3.Size = new Size(134, 23);
            button3.TabIndex = 1;
            button3.Text = "Указать путь";
            button3.UseVisualStyleBackColor = true;
            button3.Click += SavePath_Click;
            // 
            // ChousePath
            // 
            ChousePath.Location = new Point(146, 5);
            ChousePath.Name = "ChousePath";
            ChousePath.Size = new Size(134, 23);
            ChousePath.TabIndex = 1;
            ChousePath.Text = "Указать путь";
            ChousePath.UseVisualStyleBackColor = true;
            ChousePath.Click += ChoosePath_Click;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(13, 38);
            label15.Name = "label15";
            label15.Size = new Size(122, 15);
            label15.TabIndex = 0;
            label15.Text = "Путь для сохранения";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 9);
            label1.Name = "label1";
            label1.Size = new Size(104, 15);
            label1.TabIndex = 0;
            label1.Text = "Путь к DXF файлу";
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ControlDark;
            panel2.Controls.Add(WriterPathLabel);
            panel2.Controls.Add(ReaderPathLabel);
            panel2.Controls.Add(MinCount);
            panel2.Controls.Add(label8);
            panel2.Controls.Add(label17);
            panel2.Controls.Add(CurrentSize);
            panel2.Controls.Add(label16);
            panel2.Controls.Add(label6);
            panel2.Controls.Add(MinSize);
            panel2.Controls.Add(label4);
            panel2.Controls.Add(label10);
            panel2.Controls.Add(DetailsCount);
            panel2.Controls.Add(label2);
            panel2.Location = new Point(12, 93);
            panel2.Name = "panel2";
            panel2.Size = new Size(358, 345);
            panel2.TabIndex = 0;
            // 
            // WriterPathLabel
            // 
            WriterPathLabel.AutoSize = true;
            WriterPathLabel.Location = new Point(205, 319);
            WriterPathLabel.Name = "WriterPathLabel";
            WriterPathLabel.Size = new Size(38, 15);
            WriterPathLabel.TabIndex = 0;
            WriterPathLabel.Text = "label2";
            // 
            // ReaderPathLabel
            // 
            ReaderPathLabel.AutoSize = true;
            ReaderPathLabel.Location = new Point(205, 283);
            ReaderPathLabel.Name = "ReaderPathLabel";
            ReaderPathLabel.Size = new Size(38, 15);
            ReaderPathLabel.TabIndex = 0;
            ReaderPathLabel.Text = "label2";
            // 
            // MinCount
            // 
            MinCount.AutoSize = true;
            MinCount.Location = new Point(205, 181);
            MinCount.Name = "MinCount";
            MinCount.Size = new Size(38, 15);
            MinCount.TabIndex = 0;
            MinCount.Text = "label2";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(13, 185);
            label8.Name = "label8";
            label8.Size = new Size(175, 15);
            label8.TabIndex = 0;
            label8.Text = "Минимум листов потребуется";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(13, 319);
            label17.Name = "label17";
            label17.Size = new Size(122, 15);
            label17.TabIndex = 0;
            label17.Text = "Путь для сохранения";
            // 
            // CurrentSize
            // 
            CurrentSize.AutoSize = true;
            CurrentSize.Location = new Point(205, 136);
            CurrentSize.Name = "CurrentSize";
            CurrentSize.Size = new Size(38, 15);
            CurrentSize.TabIndex = 0;
            CurrentSize.Text = "label2";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(13, 283);
            label16.Name = "label16";
            label16.Size = new Size(104, 15);
            label16.TabIndex = 0;
            label16.Text = "Путь к DXF файлу";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(13, 137);
            label6.Name = "label6";
            label6.Size = new Size(142, 15);
            label6.TabIndex = 0;
            label6.Text = "Текущие размеры листа";
            // 
            // MinSize
            // 
            MinSize.AutoSize = true;
            MinSize.Location = new Point(205, 95);
            MinSize.Name = "MinSize";
            MinSize.Size = new Size(38, 15);
            MinSize.TabIndex = 0;
            MinSize.Text = "label2";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(13, 95);
            label4.Name = "label4";
            label4.Size = new Size(175, 15);
            label4.TabIndex = 0;
            label4.Text = "Минимальные размеры листа";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(146, 23);
            label10.Name = "label10";
            label10.Size = new Size(68, 15);
            label10.TabIndex = 0;
            label10.Text = "Статистика";
            // 
            // DetailsCount
            // 
            DetailsCount.AutoSize = true;
            DetailsCount.Location = new Point(205, 53);
            DetailsCount.Name = "DetailsCount";
            DetailsCount.Size = new Size(38, 15);
            DetailsCount.TabIndex = 0;
            DetailsCount.Text = "label2";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(13, 53);
            label2.Name = "label2";
            label2.Size = new Size(118, 15);
            label2.TabIndex = 0;
            label2.Text = "Количество деталей";
            // 
            // panel3
            // 
            panel3.BackColor = SystemColors.ControlDark;
            panel3.Controls.Add(HeightTextBox);
            panel3.Controls.Add(WidthTextBox);
            panel3.Controls.Add(CalculateButton);
            panel3.Controls.Add(DetailsPadingTextBox);
            panel3.Controls.Add(ListPadingTextBox);
            panel3.Controls.Add(PadingTextBox);
            panel3.Controls.Add(label11);
            panel3.Controls.Add(label12);
            panel3.Controls.Add(label20);
            panel3.Controls.Add(label13);
            panel3.Controls.Add(label14);
            panel3.Location = new Point(403, 93);
            panel3.Name = "panel3";
            panel3.Size = new Size(385, 345);
            panel3.TabIndex = 0;
            // 
            // HeightTextBox
            // 
            HeightTextBox.Enabled = false;
            HeightTextBox.Location = new Point(253, 51);
            HeightTextBox.Name = "HeightTextBox";
            HeightTextBox.Size = new Size(100, 23);
            HeightTextBox.TabIndex = 2;
            HeightTextBox.ValueChanged += TextBox_ValueChanged;
            // 
            // WidthTextBox
            // 
            WidthTextBox.Enabled = false;
            WidthTextBox.Location = new Point(147, 51);
            WidthTextBox.Name = "WidthTextBox";
            WidthTextBox.Size = new Size(100, 23);
            WidthTextBox.TabIndex = 2;
            WidthTextBox.ValueChanged += TextBox_ValueChanged;
            // 
            // CalculateButton
            // 
            CalculateButton.Enabled = false;
            CalculateButton.Location = new Point(13, 307);
            CalculateButton.Name = "CalculateButton";
            CalculateButton.Size = new Size(358, 35);
            CalculateButton.TabIndex = 1;
            CalculateButton.Text = "Начать рассчет";
            CalculateButton.UseVisualStyleBackColor = true;
            // 
            // DetailsPadingTextBox
            // 
            DetailsPadingTextBox.Enabled = false;
            DetailsPadingTextBox.Location = new Point(147, 173);
            DetailsPadingTextBox.Name = "DetailsPadingTextBox";
            DetailsPadingTextBox.PlaceholderText = "Ввод отступа (0)";
            DetailsPadingTextBox.Size = new Size(100, 23);
            DetailsPadingTextBox.TabIndex = 1;
            // 
            // ListPadingTextBox
            // 
            ListPadingTextBox.Enabled = false;
            ListPadingTextBox.Location = new Point(147, 129);
            ListPadingTextBox.Name = "ListPadingTextBox";
            ListPadingTextBox.PlaceholderText = "Ввод отступа (0)";
            ListPadingTextBox.Size = new Size(100, 23);
            ListPadingTextBox.TabIndex = 1;
            // 
            // PadingTextBox
            // 
            PadingTextBox.Enabled = false;
            PadingTextBox.Location = new Point(147, 87);
            PadingTextBox.Name = "PadingTextBox";
            PadingTextBox.PlaceholderText = "Ввод отступа (0)";
            PadingTextBox.Size = new Size(100, 23);
            PadingTextBox.TabIndex = 1;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(183, 23);
            label11.Name = "label11";
            label11.Size = new Size(95, 15);
            label11.TabIndex = 0;
            label11.Text = "Ввод и действия";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(13, 53);
            label12.Name = "label12";
            label12.Size = new Size(90, 15);
            label12.TabIndex = 0;
            label12.Text = "Размеры листа";
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new Point(13, 136);
            label20.Name = "label20";
            label20.Size = new Size(125, 15);
            label20.TabIndex = 0;
            label20.Text = "Отступ между листов";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(13, 95);
            label13.Name = "label13";
            label13.Size = new Size(103, 15);
            label13.TabIndex = 0;
            label13.Text = "Отступы от краев";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(13, 176);
            label14.Name = "label14";
            label14.Size = new Size(130, 15);
            label14.TabIndex = 0;
            label14.Text = "Отступ между деталей";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "MainForm";
            Text = "Dxf - Nesting";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)HeightTextBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)WidthTextBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button ChousePath;
        private Label label1;
        private Panel panel2;
        private Label MinCount;
        private Label label8;
        private Label CurrentSize;
        private Label label6;
        private Label MinSize;
        private Label label4;
        private Label label10;
        private Label DetailsCount;
        private Label label2;
        private Panel panel3;
        private Button SaveSettingsButton;
        private TextBox DetailsPadingTextBox;
        private TextBox PadingTextBox;
        private Label label11;
        private Label label12;
        private Label label13;
        private Label label14;
        private Button button3;
        private Label label15;
        private Button CalculateButton;
        private Label WriterPathLabel;
        private Label ReaderPathLabel;
        private Label label17;
        private Label label16;
        private TextBox ListPadingTextBox;
        private Label label20;
        private NumericUpDown HeightTextBox;
        private NumericUpDown WidthTextBox;
    }
}
