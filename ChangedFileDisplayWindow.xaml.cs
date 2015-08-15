using System.ComponentModel;
using System.Windows;

namespace FileSaver
{
    /// <summary>
    /// Interaction logic for ChangedFileDisplayWindow.xaml
    /// </summary>
    public partial class ChangedFileDisplayWindow : Window
    {
        public ChangedFileDisplayWindow()
        {
            InitializeComponent();
            if(!App.IsDesigning)
                Closing += ChangedFileDisplayWindow_Closing;
        }

        private void ChangedFileDisplayWindow_Closing(object sender, CancelEventArgs e)
        {
            Visibility = Visibility.Hidden;
            e.Cancel = true;
        }
    }
}
