using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestModule;

namespace MainWindowsApp.VeiwModel
{
    class FrameView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private List<Detail> details;

        private string _detailsCount;
        private string _minSize;
        private string _minFrames;
        private string _nameOfFile;
        private string _nameOfFolder;

        public string DetailsCount
        {
            get
            {
                return _detailsCount;
            }

            set
            {
                _detailsCount = value;
                OnPropertyChanged(nameof(DetailsCount));
            }
        }
        public string MinSize
        {
            get
            {
                return _minSize;
            }

            set
            {
                _minSize = value;
                OnPropertyChanged(nameof(MinSize));
            }
        }
        public string MinFrames
        {
            get
            {
                return _minFrames;
            }

            set
            {
                _minFrames = value;
                OnPropertyChanged(nameof(MinFrames));
            }
        }
        public string NameOfFile
        {
            get
            {
                return _nameOfFile;
            }

            set
            {
                _nameOfFile = value;
                OnPropertyChanged(nameof(NameOfFile));
            }
        }
        public string NameOfFolder
        {
            get
            {
                return _nameOfFolder;
            }

            set
            {
                _nameOfFolder = value;
                OnPropertyChanged(nameof(NameOfFolder));
            }
        }

        public FrameView()
        {
            DetailsCount = "0000";
            MinSize = "0000 : 0000";
            MinFrames = "0000";
            NameOfFile = "FileName.dxf";
            NameOfFolder = "\\FolderName\\";
        }
    }
}
