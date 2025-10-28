using Newtonsoft.Json;

namespace TrollTrack.Features.Shared.Models.WeatherApi;

public class Location
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("region")]
    public string Region { get; set; } = string.Empty;

    [JsonProperty("country")]
    public string Country { get; set; } = string.Empty;

    [JsonProperty("lat")]
    public double Lat { get; set; }

    [JsonProperty("lon")]
    public double Lon { get; set; }

    [JsonProperty("tz_id")]
    public string TzId { get; set; } = string.Empty;

    [JsonProperty("localtime_epoch")]
    public long LocaltimeEpoch { get; set; }

    [JsonProperty("localtime")]
    public string Localtime { get; set; } = string.Empty;
}