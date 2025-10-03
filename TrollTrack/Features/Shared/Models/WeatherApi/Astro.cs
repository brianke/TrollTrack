using Newtonsoft.Json;

namespace TrollTrack.Features.Shared.Models.WeatherApi;

public class Astro
{
    [JsonProperty("sunrise")]
    public string Sunrise { get; set; }

    [JsonProperty("sunset")]
    public string Sunset { get; set; }

    [JsonProperty("moonrise")]
    public string Moonrise { get; set; }

    [JsonProperty("moonset")]
    public string Moonset { get; set; }

    [JsonProperty("moon_phase")]
    public string MoonPhase { get; set; }

    [JsonProperty("moon_illumination")]
    public string MoonIllumination { get; set; }
}