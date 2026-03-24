using Newtonsoft.Json;

namespace TrollTrack.Features.Shared.Models.WeatherApi;

public class ForecastDay
{
    [JsonProperty("date")]
    public string Date { get; set; } = string.Empty;

    [JsonProperty("date_epoch")]
    public long DateEpoch { get; set; }

    [JsonProperty("day")]
    public Day Day { get; set; } = new Day();

    [JsonProperty("astro")]
    public Astro Astro { get; set; } = new Astro();

    [JsonProperty("hour")]
    public List<Hour> Hour { get; set; } = new List<Hour>();
}