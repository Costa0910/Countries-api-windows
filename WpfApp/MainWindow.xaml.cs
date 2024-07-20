using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ClassLibraryAPI.Models;
using ClassLibraryAPI.Services;
using WpfApp.UIServices;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly APIService _api;
        private readonly SqLiteService _db;
        private List<Country> _countries;
        private Progress<double> Progress;
        public MainWindow()
        {
            InitializeComponent();
            _api = new APIService();
            _db = new SqLiteService();

            Progress = new Progress<double>(value =>
            {
                // Update progress bar value
                progress.Value = value;
            });

        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            status.Text = "Checking Connection...";
            var result = await NetworkService.CheckConnection();
            if (result.Status)
            {
                // Get data from API
                status.Text = "Connected";
                var response = await _api.GetCountriesAsync("/v3.1/all");
                if (response.Status)
                {
                    status.Text = "Countries Loaded from api";
                    _countries = (List<Country>)response.Result;
                    // Update UI
                    UpdateUI(_countries);

                    // delete data from db
                    status.Text = "Deleting data from db...";
                    await _db.DeleteDataAsync();

                    // Save data to SQLite
                    // ADD
                    status.Text = "Saving Countries to DB...";
                    var saveResult = await _db.SaveCountriesAsync(_countries, Progress);
                    if (saveResult.Status)
                    {
                        status.Text = "Countries Saved to DB";
                    }
                    else
                    {
                        DialogService.ShowMessage("Erro", "Erro ao tentar guardar os países na base de dados");
                        status.Text = "Failed to Save Countries to DB";
                    }
                }
                else
                {
                    status.Text = "Failed to Load Countries from API, trying DB...";
                    // Get data from SQLite
                    await GettingFromDb();
                }
            }
            else
            {
                // Get data from SQLite
                status.Text = "Not Connected. Trying DB...";
                await GettingFromDb();
            }
        }

        private void UpdateUI(List<Country> countries)
        {
            listBox_Countries.ItemsSource = null;
            listBox_Countries.ItemsSource = countries;
            listBox_Countries.DisplayMemberPath = "DisplayName";
        }

        private async Task GettingFromDb()
        {
            // Add progress bar Name
            var dbResult = await _db.GetCountriesAsync(Progress);

            if (dbResult.Status)
            {
                _countries = (List<Country>)dbResult.Result;
                UpdateUI(_countries);
                status.Text = "Countries Loaded from DB";
            }
            else
            {
                DialogService.ShowMessage("Erro", "Erro ao tentar carregar os países da base de dados");
                status.Text = "Failed to Load Countries from DB";
            }
        }

        private void search_TextChanged(object sender, TextChangedEventArgs e)
        {
            //var selected = _countries.Where(c => c.Name.ToLower().Contains(search.Text.ToLower())).ToList();

        }

        private void listBox_Countries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var country = (Country)listBox_Countries.SelectedItem;
            if (country == null) return;
            //if (country != null)
            //{
            //    var countryWindow = new CountryWindow(country);
            //    countryWindow.Show();
            //}

            // show detais in the same window
            details.Text = $"Detalhes do país: {country.DisplayName}";
            name.Text = $"Nome Oficial: {country.Name.Official}";
            capital.Text = $"Capital: {string.Join(", ", country.Capital)}";
            region.Text = $"Região: {country.Region}";
            subregion.Text = $"Sub-refião: {country.Subregion}";
            population.Text = $"População: {country.Population:N}";
            if (country.Gini.ContainsKey("default")) 
            {
                gini.Text = "Índice Gini: Não disponível";
            }
            else
            {
                var ginis = country.Gini.Select(g => $"{g.Key} -> {g.Value}").ToList();

               gini.Text = string.Join(", ", ginis);
               gini.Text = $"Índice Gini: {string.Join(", ", ginis)}";
            }

            if (country.Languages.ContainsKey("default"))
            {
                languages.Text = "Linguas: Não disponível";
            }
            else
            {
                var lang = country.Languages.Select(l => l.Value).ToList();
                languages.Text = $"Linguas: {string.Join(", ", lang)}";
            }

            // img
            var url = new BitmapImage(new Uri(country.Flags.Png, UriKind.RelativeOrAbsolute));
            img.Source = url;

        }
    }
}