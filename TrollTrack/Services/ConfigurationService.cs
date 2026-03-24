using System.Reflection;
using Newtonsoft.Json.Linq;

namespace TrollTrack.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly JObject _secrets;
    private const string Namespace = "TrollTrack";
    private const string FileName = "Configuration.appsettings.json";

    public string ApiKey => _secrets?["WeatherApi"]?["ApiKey"]?.Value<string>() ?? string.Empty;

    public ConfigurationService()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{Namespace}.{FileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException("Could not find configuration file.", resourceName);
        }

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        _secrets = JObject.Parse(json);
    }
}