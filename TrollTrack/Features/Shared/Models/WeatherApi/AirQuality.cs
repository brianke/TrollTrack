using Newtonsoft.Json;

namespace TrollTrack.Features.Shared.Models.WeatherApi;

public class AirQuality
{
    [JsonProperty("co")]
    public double Co { get; set; }

    [JsonProperty("no2")]
    public double No2 { get; set; }

    [JsonProperty("o3")]
    public double O3 { get; set; }

    [JsonProperty("so2")]
    public double So2 { get; set; }

    [JsonProperty("pm2_5")]
    public double Pm25 { get; set; }

    [JsonProperty("pm10")]
    public double Pm10 { get; set; }

    [JsonProperty("us-epa-index")]
    public int UsEpaIndex { get; set; }

    [JsonProperty("gb-defra-index")]
    public int GbDefraIndex { get; set; }
}