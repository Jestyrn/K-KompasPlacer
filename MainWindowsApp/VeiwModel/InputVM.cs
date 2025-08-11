using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainWindowsApp.VeiwModel
{
    class InputView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        private string _frameSize;
        public string FrameSize
        {
            get { return _frameSize; }
            set
            {
                _frameSize = value;
                OnPropertyChanged(nameof(FrameSize));
            }
        }

        private string _padingInside;
        public string PadingInside
        {
            get { return _padingInside; }
            set
            {
                _padingInside = value;
                OnPropertyChanged(nameof(PadingInside));
            }
        }

        private string _framePading;
        public string FramePading
        {
            get { return _framePading; }
            set
            {
                _framePading = value;
                OnPropertyChanged(nameof(FramePading));
            }
        }

        private string _detailPading;
        public string DetailPading
        {
            get { return _detailPading; }
            set
            {
                _detailPading = value;
                OnPropertyChanged(nameof(DetailPading));
            }
        }


        public InputView()
        {
            _frameSize = "0000 : 0000";
            _padingInside = "0000";
            _framePading = "0000";
            _detailPading = "0000";
        }
    }
}
