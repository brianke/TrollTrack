using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using TrollTrack.Features.Analytics;
using TrollTrack.Features.Catches;
using TrollTrack.Features.Dashboard;
using TrollTrack.Features.Lures;
using TrollTrack.Features.Programs;
using TrollTrack.Services;

namespace TrollTrack
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register HTTP client
            builder.Services.AddSingleton<HttpClient>();

            // Register services
            builder.Services.AddSingleton<ILocationService, LocationService>();
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
            builder.Services.AddSingleton<IWeatherService, WeatherService>();

            // Register ViewModels
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<CatchesViewModel>();
            builder.Services.AddTransient<LuresViewModel>();
            builder.Services.AddTransient<ProgramsViewModel>();
            builder.Services.AddTransient<AnalyticsViewModel>();

            // Register Views
            builder.Services.AddTransient<DashboardView>();
            builder.Services.AddTransient<CatchesView>();
            builder.Services.AddTransient<LuresView>();
            builder.Services.AddTransient<ProgramsView>();
            builder.Services.AddTransient<AnalyticsView>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            //var app = builder.Build();

            //// Initialize configuration on app startup
            //_ = Task.Run(async () =>
            //{ 
            //    // TODO: Remove before deploying
            //    // Replace "your-actual-api-key-here" with your real key
            //    await SecureStorage.SetAsync("WeatherApiKey", "c94ed9868e9447c2b2e145937252508");
            //    System.Diagnostics.Debug.WriteLine("Test API key set!");

            //    try
            //    {
            //        await ConfigurationService.InitializeAsync();
            //    }
            //    catch (Exception ex)
            //    {
            //        System.Diagnostics.Debug.WriteLine($"Failed to initialize configuration: {ex.Message}");
            //    }
            //});

            return builder.Build();
        }
    }
}
