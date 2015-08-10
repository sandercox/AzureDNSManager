using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDNSManager
{
    class InputDialogModel : INotifyPropertyChanged
    {
        private string _caption = "Input Dialog";
        public string Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;
                OnPropertyChanged();
            }
        }

        private string _prompt = "Input Dialog";
        public string Prompt
        {
            get { return _prompt; }
            set
            {
                _prompt = value;
                OnPropertyChanged();
            }
        }

        private string _value = "Input Dialog";
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName]string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null && !string.IsNullOrWhiteSpace(name))
                handler(this, new PropertyChangedEventArgs(name));

        }

    }
}
