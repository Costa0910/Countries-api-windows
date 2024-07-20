using System.Windows;
using System.Windows.Controls;
using ClassLibraryAPI.Services;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly APIService _api;
        private readonly SqLiteService _db;
        public MainWindow()
        {
            InitializeComponent();
            _api = new APIService();
            _db = new SqLiteService();
        }

        private void search_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}