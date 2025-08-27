using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WeatherWpfApp.Models
{
    public class WeatherResponse
    {
        [JsonProperty("name")]
        public string CityName { get; set; }

        [JsonProperty("main")]
        public MainData Main { get; set; }

        [JsonProperty("weather")]
        public WeatherInfo[] Weather { get; set; }

        [JsonProperty("wind")]
        public WindData Wind { get; set; }
    }
    public class MainData
    {
        [JsonProperty("temp")]
        public double Temperature { get; set; }

        [JsonProperty("feels_like")]
        public double FeelsLike { get; set; }

        [JsonProperty("humidity")]
        public int Humidity { get; set; }
    }

    public class WeatherInfo
    {
        [JsonProperty("main")]
        public string Main { get; set; } 

        [JsonProperty("description")]
        public string Description { get; set; } 

        [JsonProperty("icon")]
        public string Icon { get; set; } 
    }

    public class WindData
    {
        [JsonProperty("speed")]
        public double Speed { get; set; }
    }
}
