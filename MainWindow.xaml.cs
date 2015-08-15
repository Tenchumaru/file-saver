using System.ComponentModel;
using System.Windows;

namespace FileSaver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if(!App.IsDesigning)
            {
                App.Current.Configure(this);
                Closing += MainWindow_Closing;
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Visibility = Visibility.Hidden;
            e.Cancel = true;
        }
    }
}
