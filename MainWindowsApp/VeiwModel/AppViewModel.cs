using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace MainWindowsApp.VeiwModel
{
    class AppViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ==== InputView часть ====
        private string _frameSize;
        public string FrameSize
        {
            get => _frameSize;
            set
            {
                _frameSize = CheckFormat(value);
                OnPropertyChanged(nameof(FrameSize));
                CalculateState();
            }
        }

        private string _padingInside;
        public string PadingInside
        {
            get => _padingInside;
            set
            {
                _padingInside = value;
                CalculateState();
                OnPropertyChanged(nameof(PadingInside));
            }
        }

        private string _framePading;
        public string FramePading
        {
            get => _framePading;
            set
            {
                _framePading = value;
                OnPropertyChanged(nameof(FramePading));
                CalculateState();
            }
        }

        private string _detailPading;
        public string DetailPading
        {
            get => _detailPading;
            set
            {
                _detailPading = value;
                OnPropertyChanged(nameof(DetailPading));
                CalculateState();
            }
        }

        // ==== FrameView часть ====
        private string _detailsCount;
        public string DetailsCount
        {
            get => _detailsCount;
            set
            {
                _detailsCount = value;
                OnPropertyChanged(nameof(DetailsCount));
                CalculateState();
            }
        }

        public string MindMinSize = "1 : 1";
        public string MindDetailsArea = "100";
        private string _minSize;
        public string MinSize
        {
            get => _minSize;
            set
            {
                _minSize = value;
                OnPropertyChanged(nameof(MinSize));
            }
        }

        private string _minFrames;
        public string MinFrames
        {
            get => _minFrames;
            set
            {
                _minFrames = value;
                OnPropertyChanged(nameof(MinFrames));
            }
        }

        private string _nameOfFile;
        public string NameOfFile
        {
            get => _nameOfFile;
            set
            {
                _nameOfFile = value;
                OnPropertyChanged(nameof(NameOfFile));
            }
        }

        private string _nameOfFolder;
        public string NameOfFolder
        {
            get => _nameOfFolder;
            set
            {
                _nameOfFolder = value;
                OnPropertyChanged(nameof(NameOfFolder));
            }
        }

        // ==== Конструктор ====
        public AppViewModel()
        {
            // инициализация Input
            _frameSize = "0000 : 0000";
            _padingInside = "0";
            _framePading = "0";
            _detailPading = "0";

            // инициализация Frame
            _detailsCount = "0";
            _minSize = "0000 : 0000";
            _minFrames = "0";
            _nameOfFile = "FileName.dxf";
            _nameOfFolder = "\\FolderName\\";
        }

        // ==== Логика ====
        private void CalculateState()
        {
            double a;
            int b;

            double minWidth = double.TryParse(MindMinSize.Split(" : ")[0], out a) ? a : 1;
            double minHeight = double.TryParse(MindMinSize.Split(" : ")[1], out a) ? a : 1;

            double currentWidth = double.TryParse(FrameSize.Split(" : ")[0], out a) ? a : 1;
            double currentHeight = double.TryParse(FrameSize.Split(" : ")[1], out a) ? a : 1;

            int count = int.TryParse(DetailsCount, out b) ? b : 1;
            int pading = int.TryParse(PadingInside, out b) ? b : 1;

            MinSize = $"{minWidth + (pading * 2)} : {minHeight + (pading * 2)}";
        }

        private string CheckFormat(string size)
        {
            size = size.Replace(" ", "").Replace(":", " : ").Replace(";", ":").Replace("^", ":");
            size = Regex.Replace(size, "[A-Za-zА-Яа-я]", "");

            if (size.Contains(":") && size.Count(c => c == ':') > 1)
                return "0 : 0";

            return size;
        }
    }
}
