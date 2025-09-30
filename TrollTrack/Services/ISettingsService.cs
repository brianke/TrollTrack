namespace TrollTrack.Services
{
    public interface ISettingsService
    {
        string WeatherApiKey { get; set; }
        bool IsWeatherApiConfigured();
        (bool IsValid, string Message) GetWeatherApiKeyStatus();
    }
}