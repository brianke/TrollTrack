using Newtonsoft.Json;

namespace TrollTrack.Features.Shared.Models.WeatherApi;

public class Astro
{
    [JsonProperty("sunrise")]
    public string Sunrise { get; set; } = string.Empty;

    [JsonProperty("sunset")]
    public string Sunset { get; set; } = string.Empty;

    [JsonProperty("moonrise")]
    public string Moonrise { get; set; } = string.Empty;

    [JsonProperty("moonset")]
    public string Moonset { get; set; } = string.Empty;

    [JsonProperty("moon_phase")]
    public string MoonPhase { get; set; } = string.Empty;

    [JsonProperty("moon_illumination")]
    public string MoonIllumination { get; set; } = string.Empty;
}