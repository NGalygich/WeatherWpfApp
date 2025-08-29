using System.Threading.Tasks;

namespace WeatherWpfApp.Services
{
    public interface IGeoLocationService
    {
        Task<string> GetUserLocationAsync();
        Task<(double Latitude, double Longitude)> GetUserCoordinatesAsync();
    }
}
