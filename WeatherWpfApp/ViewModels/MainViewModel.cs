using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using WeatherWpfApp.Models;
using WeatherWpfApp.Services;

namespace WeatherWpfApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly WeatherService _weatherService;
        private string _cityName;
        private WeatherResponse _currentWeather;

        public MainViewModel()
        {
            _weatherService = new WeatherService();
            SearchCommand = new RelayCommand(async () => await LoadWeatherData());
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

        public ICommand SearchCommand { get; }

        private async Task LoadWeatherData()
        {
            if (string.IsNullOrWhiteSpace(CityName))
                return;

            var weather = await _weatherService.GetCurrentWeatherAsync(CityName);
            CurrentWeather = weather;
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
        public RelayCommand(Func<Task> execute) => _execute = execute;
        public bool CanExecute(object parameter) => true;
        public async void Execute(object parameter) => await _execute();
        public event EventHandler CanExecuteChanged;
    }
}
