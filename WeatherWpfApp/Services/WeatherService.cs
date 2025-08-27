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
        private const string ApiKey = "0f3e3d9a05caf25bdd1b3ce2646b309f";
        private readonly HttpClient _httpClient;

        public WeatherService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<WeatherResponse> GetCurrentWeatherAsync(string cityName)
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={ApiKey}&units=metric&lang=ru";

            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<WeatherResponse>(content);

            return result;
        }
    }
}
