using Newtonsoft.Json;

namespace TrollTrack.Features.Shared.Models.WeatherApi;

public class ForecastDay
{
    [JsonProperty("date")]
    public string Date { get; set; }

    [JsonProperty("date_epoch")]
    public long DateEpoch { get; set; }

    [JsonProperty("day")]
    public Day Day { get; set; }

    [JsonProperty("astro")]
    public Astro Astro { get; set; }

    [JsonProperty("hour")]
    public List<Hour> Hour { get; set; }
}