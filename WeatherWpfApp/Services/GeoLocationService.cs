using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using WeatherWpfApp.Models;


namespace WeatherWpfApp.Services
{
    public class GeoLocationService : IGeoLocationService
    {
        public async Task<string> GetUserLocationAsync()
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
    }
}
