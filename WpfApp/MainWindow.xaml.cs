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
            loaderMessage.Text = "A verificar ligação a internet...";
            var result = await NetworkService.CheckConnection();
            if (result.Status)
            {
                // Get data from API
                loaderMessage.Text = "Ligação a internet estabelicida...";
                var response = await _api.GetCountriesAsync("/v3.1/all");
                if (response.Status)
                {
                    status.Text = "Dados dos países carregados da api...";
                    _countries = (List<Country>)response.Result;
                    // Show data in UI
                    loader.Visibility = Visibility.Collapsed;
                    main_section.Visibility = Visibility.Visible;
                    status_section.Visibility = Visibility.Visible;
                    search_section.Visibility = Visibility.Visible;
                    // Update UI
                    UpdateUI(SortCountries(_countries));

                    // delete data from db
                    status.Text = "Apagar os dados antigos da base dados...";
                    await _db.DeleteDataAsync();

                    // Save data to SQLite
                    // ADD
                    status.Text = "A guardar os países na base dados...";
                    var saveResult = await _db.SaveCountriesAsync(_countries, Progress);
                    if (saveResult.Status)
                    {
                        status.Text = "Os dados dos países guardos na base dados.";
                    }
                    else
                    {
                        DialogService.ShowMessage("Erro", "Erro ao tentar guardar os países na base de dados");
                        status.Text = "Erro ao tentar guardar os dados na base dados.";
                    }
                }
                else
                {
                    loaderMessage.Text = "Erro ao carregar dados da api, tentando carregar da base dados...";
                    // Get data from SQLite
                    await GettingFromDb();
                }
            }
            else
            {
                // Get data from SQLite
                loaderMessage.Text = "Sem ligação a internet, tentando carregar na base de dados...";
                await GettingFromDb();
            }
        }

        private void UpdateUI(List<Country> countries)
        {
            listBox_Countries.ItemsSource = null;
            listBox_Countries.ItemsSource = countries;
            //listBox_Countries.DisplayMemberPath = "DisplayName";

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

                // Show data in UI
                loader.Visibility = Visibility.Collapsed;
                main_section.Visibility = Visibility.Visible;
                status_section.Visibility = Visibility.Visible;
                search_section.Visibility = Visibility.Visible;
                UpdateUI(SortCountries(_countries));
                status.Text = "Dados dos países carregados da base dados.";
            }
            else
            {
                DialogService.ShowMessage("Erro", "Erro ao tentar carregar os países da base de dados.");
                loaderMessage.Text = dbResult.Message;
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
            await Task.Delay(500);

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
            var url = new BitmapImage (new Uri(country.Flags.Png, UriKind.RelativeOrAbsolute));
            img.Source = url;
        }
    }
}