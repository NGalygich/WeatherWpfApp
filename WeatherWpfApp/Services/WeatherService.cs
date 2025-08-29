using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using WeatherWpfApp.Models;

namespace WeatherWpfApp.Services
{
    public class WeatherService
    {
        private const string ApiKey = "ключ api";
        private const string BaseUrl = "https://api.openweathermap.org/data/2.5";
        private readonly HttpClient _httpClient;

        public WeatherService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<WeatherResponse> GetCurrentWeatherAsync(string cityName)
        {
            var url = $"{BaseUrl}/weather?q={cityName}&appid={ApiKey}&units=metric&lang=ru";
            var response = await _httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<WeatherResponse>(response);
        }

        public async Task<ForecastResponse> Get5DayForecastAsync(string cityName)
        {
            var url = $"{BaseUrl}/forecast?q={cityName}&appid={ApiKey}&units=metric&lang=ru";
            var response = await _httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<ForecastResponse>(response);
        }
    }
}
