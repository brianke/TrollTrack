using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using TrollTrack.Features.RodSetup;

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
                    fonts.AddFont("RobotoMono-Regular.ttf", "RobotoMono");
                });

            // Register HTTP client
            builder.Services.AddSingleton<HttpClient>();

            // Register services
            builder.Services.AddSingleton<ILocationService, LocationService>();
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
            builder.Services.AddSingleton<IWeatherService, WeatherService>();
            builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
            builder.Services.AddSingleton<ITripService, TripService>();
            builder.Services.AddSingleton<IRodSetupService, RodSetupService>();

            // Register ViewModels
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<CatchesViewModel>();
            builder.Services.AddTransient<RodSetupViewModel>();
            builder.Services.AddTransient<LuresViewModel>();
            builder.Services.AddTransient<AnalyticsViewModel>();

            // Register Views
            builder.Services.AddTransient<DashboardView>();
            builder.Services.AddTransient<CatchesView>();
            builder.Services.AddTransient<NewTripView>();
            builder.Services.AddTransient<RodSetupPopup>();
            builder.Services.AddTransient<LuresView>();
            builder.Services.AddTransient<AnalyticsView>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // Seed data on first run
            var dbService = app.Services.GetRequiredService<IDatabaseService>();
            dbService.SeedInitialDataAsync();

            return app;
        }
    }
}
