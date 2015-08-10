using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AzureDNSManager
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialog(string caption, string prompt, string def)
        {
            InitializeComponent();
            this.DataContext = new InputDialogModel(){
                Caption = caption,
                Prompt = prompt,
                Value = def
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public string Value { get { return (this.DataContext as InputDialogModel)?.Value; } }
    }
}
