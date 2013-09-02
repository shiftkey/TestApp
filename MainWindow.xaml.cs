using System.Windows;
using System.Windows.Input;

namespace TestApp
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel();

            InitializeComponent();
        }
    }
}
