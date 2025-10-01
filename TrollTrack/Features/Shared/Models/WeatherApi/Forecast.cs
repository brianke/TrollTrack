using Newtonsoft.Json;

namespace TrollTrack.Features.Shared.Models.WeatherApi;

public class Forecast
{
    [JsonProperty("forecastday")]
    public List<ForecastDay> ForecastDay { get; set; }
}