using TrollTrack.Configuration;

namespace TrollTrack.Services
{
    public class SettingsService : ISettingsService
    {
        public string WeatherApiKey
        {
            get => SecureStorage.GetAsync("WeatherApiKey").Result ?? "YOUR_API_KEY_HERE";
            set => SecureStorage.SetAsync("WeatherApiKey", value);
        }

        public bool IsWeatherApiConfigured()
        {
            var apiKey = WeatherApiKey;
            return !string.IsNullOrEmpty(apiKey) && apiKey != "YOUR_API_KEY_HERE";
        }

        public (bool IsValid, string Message) GetWeatherApiKeyStatus()
        {
            var apiKey = WeatherApiKey;

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
            {
                return (false, "Weather API key is not configured. Please set your WeatherAPI.com key in Settings.");
            }

            if (apiKey.Length < 10)
            {
                return (false, "Weather API key appears to be invalid. Please check your key.");
            }

            return (true, "Weather API key is configured.");
        }
    }
}