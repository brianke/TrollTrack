using Newtonsoft.Json;

namespace TrollTrack.Features.Shared.Models.WeatherApi;

public class WeatherApiResponse
{
    [JsonProperty("location")]
    public Location Location { get; set; } = new Location();

    [JsonProperty("current")]
    public Current Current { get; set; } = new Current();

    [JsonProperty("forecast")]
    public Forecast Forecast { get; set; } = new Forecast();
}