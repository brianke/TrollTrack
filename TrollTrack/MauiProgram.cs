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
            builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

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

            return builder.Build();
        }
    }
}
