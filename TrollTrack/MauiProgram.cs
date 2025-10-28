using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

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

            // Register ViewModels
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<CatchesViewModel>();
            builder.Services.AddTransient<LuresViewModel>();
            builder.Services.AddTransient<ProgramsViewModel>();
            builder.Services.AddTransient<AnalyticsViewModel>();
            //builder.Services.AddTransient<TripViewModel>();

            // Register Views
            builder.Services.AddTransient<DashboardView>();
            builder.Services.AddTransient<CatchesView>();
            builder.Services.AddTransient<LuresView>();
            builder.Services.AddTransient<ProgramsView>();
            builder.Services.AddTransient<AnalyticsView>();
            //builder.Services.AddTransient<TripView>();
            //builder.Services.AddTransient<TripCatchesContainerView>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
