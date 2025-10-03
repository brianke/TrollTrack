using Newtonsoft.Json;

namespace TrollTrack.Features.Shared.Models.WeatherApi;

public class WeatherApiResponse
{
    [JsonProperty("location")]
    public Location Location { get; set; }

    [JsonProperty("current")]
    public Current Current { get; set; }

    [JsonProperty("forecast")]
    public Forecast Forecast { get; set; }
}