using Fluent;
using ViewModels;

namespace ZeroExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainViewModel ViewModel { get; }
        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();
            ViewModel = mainViewModel;
            DataContext = ViewModel;
        }
    }
}