using Newtonsoft.Json;

namespace TrollTrack.Features.Shared.Models.WeatherApi;

public class Condition
{
    [JsonProperty("text")]
    public string Text { get; set; } = string.Empty;

    [JsonProperty("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonProperty("code")]
    public int Code { get; set; }
}