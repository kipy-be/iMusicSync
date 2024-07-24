using IMusicSync.States;
using System.Windows;

namespace IMusicSync
{
    public partial class MainWindow : Window
    {
        public MainState State { get; private set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = State;
        }

        private void Logs_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            tbLogs.ScrollToEnd();
        }
    }
}