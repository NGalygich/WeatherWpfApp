using System.Collections.Generic;
using Newtonsoft.Json;

namespace WeatherWpfApp.Models
{
    public class ForecastResponse
    {
        [JsonProperty("list")]
        public List<ForecastItem> Items { get; set; }
    }

    public class ForecastItem
    {
        [JsonProperty("dt")]
        public long Timestamp { get; set; }

        [JsonProperty("main")]
        public MainData Main { get; set; }

        [JsonProperty("weather")]
        public List<WeatherInfo> Weather { get; set; }

        [JsonProperty("dt_txt")]
        public string DateTimeText { get; set; }

        public string Date => DateTimeText.Split(' ')[0];
    }
}
