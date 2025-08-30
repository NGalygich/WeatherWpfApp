using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WeatherWpfApp.Models;
using WeatherWpfApp.Services;
using System.Collections.Generic;
using System.Linq;

namespace WeatherWpfApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly WeatherService _weatherService;
        private readonly IGeoLocationService _geoLocationService;
        private string _cityName;
        private WeatherResponse _currentWeather;
        private bool _isLoading;
        private List<ForecastItem> _dailyForecast;
        public List<ForecastItem> DailyForecast
        {
            get => _dailyForecast;
            set { _dailyForecast = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            _weatherService = new WeatherService();
            _geoLocationService = new GeoLocationService();

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
                var weatherTask = _weatherService.GetCurrentWeatherAsync(CityName);
                var forecastTask = _weatherService.Get5DayForecastAsync(CityName);

                await Task.WhenAll(weatherTask, forecastTask);

                CurrentWeather = await weatherTask;
                var forecast = await forecastTask;

                DailyForecast = forecast.Items
                    .Where(x => x.DateTimeText.Contains("6:00:00"))
                    .Take(5)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var location = await _geoLocationService.GetUserLocationAsync();
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
}