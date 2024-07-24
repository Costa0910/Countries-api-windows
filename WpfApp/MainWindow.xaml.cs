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
                    UpdateUI(SortCountries(_countries));

                    // delete data from db
                    status.Text = "Deleting Countries from db...";
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

            // fire change selection event to display country details 
            listBox_Countries.SelectedIndex = 0;
        }

        private async Task GettingFromDb()
        {
            // Add progress bar Name
            var dbResult = await _db.GetCountriesAsync(Progress);

            if (dbResult.Status)
            {
                _countries = (List<Country>)dbResult.Result;
                UpdateUI(SortCountries(_countries));
                status.Text = "Countries Loaded from DB";
            }
            else
            {
                DialogService.ShowMessage("Erro", "Erro ao tentar carregar os países da base de dados");
                status.Text = "Failed to Load Countries from DB";
            }
        }

        /// <summary>
        /// Order countries by name
        /// </summary>
        private List<Country> SortCountries(List<Country> country)
        {
            return country.OrderBy(c => c.Name.Common).ToList();
        }

        private async void search_TextChanged(object sender, TextChangedEventArgs e)
        {
            // not filter while user is still typing
            await Task.Delay(1000);

            var selected = _countries.Where(c => c.Name.Common.ToLower().Contains(search.Text.ToLower())).ToList();

            if (selected == null || string.IsNullOrWhiteSpace(search.Text))
            {
                UpdateUI(SortCountries(_countries));
            } else
            {
                UpdateUI(SortCountries(selected));
            }
        }

        private void listBox_Countries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var country = (Country)listBox_Countries.SelectedItem;
            if (country == null) return;

            // show detais in the same window
            name.Text = country.DisplayName;
            nameOfficial.Text = country.Name.Official;
            capital.Text = string.Join(", ", country.Capital);
            region.Text = country.Region;
            subregion.Text = country.Subregion;
            population.Text = country.Population.ToString("N");
            if (country.Gini.ContainsKey("default")) 
            {
                gini.Text = "Não disponível";
            }
            else
            {
                var ginis = country.Gini.Select(g => $"{g.Key} -> {g.Value}").ToList();

               gini.Text = string.Join(", ", ginis);
            }

            if (country.Languages.ContainsKey("default"))
            {
                languages.Text = "Não disponível";
            }
            else
            {
                var lang = country.Languages.Select(l => l.Value).ToList();
                languages.Text = string.Join(", ", lang);
            }

            // img
            var url = new BitmapImage(new Uri(country.Flags.Png, UriKind.RelativeOrAbsolute));
            img.Source = url;

        }
    }
}