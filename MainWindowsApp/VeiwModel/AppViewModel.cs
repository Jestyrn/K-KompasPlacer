using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainWindowsApp.VeiwModel
{
    class AppViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }



        private FrameView _frame;
        public FrameView Frame
        {
            get { return _frame; }
            set
            {
                _frame = value;
                OnPropertyChanged(nameof(Frame));
            }
        }

        private InputView _input;
        public InputView InputView
        {
            get
            {
                return _input;
            }
            set
            {
                _input = value;
                OnPropertyChanged(nameof(InputView));
            }
        }

        public AppViewModel()
        {
            _frame = new FrameView();
            _input = new InputView();
        }
    }
}
