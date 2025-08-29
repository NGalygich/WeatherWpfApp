using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WeatherWpfApp.Models;
using WeatherWpfApp.Services;
using System.Net.Http;
using System.Text.Json;
using System.Net;
using System.Text;
using System.IO;

namespace WeatherWpfApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly WeatherService _weatherService;
        private string _cityName;
        private WeatherResponse _currentWeather;
        private bool _isLoading;

        public MainViewModel()
        {
            _weatherService = new WeatherService();

            SearchCommand = new RelayCommand(async () => await LoadWeatherData());
            LoadUserLocationCommand = new RelayCommand(async () => await LoadUserLocationAsync());

            Task.Run(async () => await LoadUserLocationAsync());
        }

        public string CityName
        {
            get => _cityName;
            set { _cityName = value; OnPropertyChanged(); }
        }

        public WeatherResponse CurrentWeather
        {
            get => _currentWeather;
            set { _currentWeather = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ICommand SearchCommand { get; }
        public ICommand LoadUserLocationCommand { get; }

        private async Task LoadWeatherData()
        {
            if (string.IsNullOrWhiteSpace(CityName))
                return;

            IsLoading = true;
            try
            {
                var weather = await _weatherService.GetCurrentWeatherAsync(CityName);
                CurrentWeather = weather;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении погоды: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadUserLocationAsync()
        {
            IsLoading = true;
            try
            {
                var location = await GetUserLocationAsync();
                if (!string.IsNullOrEmpty(location) &&
                    location != "Местоположение не определено" &&
                    location != "Не удалось определить местоположение")
                {
                    CityName = location.Split(',')[0].Trim();
                    await LoadWeatherData();
                }
                else
                {
                    MessageBox.Show("Местоположение не определено. Введите город вручную.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при определении местоположения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task<string> GetUserLocationAsync()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                string[] endpoints = {
                    "https://ipapi.co/json/",
                    "https://ipapi.co/8.8.8.8/json/", 
                    "https://api.ipapi.com/api/check?access_key=free" 
                };

                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        var request = (HttpWebRequest)WebRequest.Create(endpoint);
                        request.UserAgent = "WeatherWpfApp/1.0";
                        request.Method = "GET";
                        request.Timeout = 5000;
                        request.Accept = "application/json";

                        using (var response = (HttpWebResponse)await request.GetResponseAsync())
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                using (var stream = response.GetResponseStream())
                                using (var reader = new StreamReader(stream, Encoding.UTF8))
                                {
                                    var jsonResponse = await reader.ReadToEndAsync();
                                    Console.WriteLine($"Response from {endpoint}: {jsonResponse}");

                                    var ipData = JsonSerializer.Deserialize<IpApiResponse>(jsonResponse);

                                    if (ipData != null && !string.IsNullOrEmpty(ipData.city))
                                    {
                                        return $"{ipData.city}, {ipData.region}, {ipData.country_name}";
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при запросе к {endpoint}: {ex.Message}");
                        continue;
                    }
                }
                return await GetLocationFromAlternativeApi();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка при определении местоположения: {ex.Message}");
                return "Не удалось определить местоположение";
            }
        }

        public async Task<(double Latitude, double Longitude)> GetUserCoordinatesAsync()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var request = (HttpWebRequest)WebRequest.Create("https://ipapi.co/json/");
                request.UserAgent = "ipapi.co/#c-sharp-v1.03";
                request.Method = "GET";
                request.Timeout = 10000;

                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var stream = response.GetResponseStream())
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            var jsonResponse = await reader.ReadToEndAsync();
                            var data = JsonSerializer.Deserialize<IpApiResponse>(jsonResponse);

                            if (data != null)
                            {
                                return (data.latitude, data.longitude);
                            }
                        }
                    }
                }

                return (0, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении координат: {ex.Message}");
                return (0, 0);
            }
        }

        private async Task<string> GetLocationFromAlternativeApi()
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("http://ip-api.com/json/");
                request.UserAgent = "WeatherWpfApp/1.0";
                request.Method = "GET";
                request.Timeout = 5000;

                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var stream = response.GetResponseStream())
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            var jsonResponse = await reader.ReadToEndAsync();
                            var data = JsonSerializer.Deserialize<IpApiAlternativeResponse>(jsonResponse);

                            if (data != null && data.status == "success" && !string.IsNullOrEmpty(data.city))
                            {
                                return $"{data.city}, {data.regionName}, {data.country}";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка альтернативного API: {ex.Message}");
            }

            return "Местоположение не определено";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public async void Execute(object parameter) => await _execute();

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    public class IpApiAlternativeResponse
    {
        public string status { get; set; }
        public string country { get; set; }
        public string countryCode { get; set; }
        public string region { get; set; }
        public string regionName { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public string timezone { get; set; }
        public string isp { get; set; }
        public string org { get; set; }
        public string @as { get; set; }
        public string query { get; set; }
    }

    public class IpApiResponse
    {
        public string ip { get; set; }
        public string version { get; set; }
        public string city { get; set; }
        public string region { get; set; }
        public string region_code { get; set; }
        public string country_code { get; set; }
        public string country_code_iso3 { get; set; }
        public string country_name { get; set; }
        public string country_capital { get; set; }
        public string country_tld { get; set; }
        public string continent_code { get; set; }
        public bool in_eu { get; set; }
        public string postal { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string timezone { get; set; }
        public string utc_offset { get; set; }
        public string country_calling_code { get; set; }
        public string currency { get; set; }
        public string currency_name { get; set; }
        public string languages { get; set; }
        public double country_area { get; set; }
        public int country_population { get; set; }
        public string asn { get; set; }
        public string org { get; set; }
        public string hostname { get; set; }
    }
}