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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register Services
            builder.Services.AddSingleton<LocationService>();
            //builder.Services.AddSingleton<DatabaseService>();
            //builder.Services.AddSingleton<WeatherService>();
            //builder.Services.AddSingleton<AIRecommendationService>();

            // Register ViewModels
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<CatchesViewModel>();
            builder.Services.AddTransient<LuresViewModel>();
            builder.Services.AddTransient<ProgramsViewModel>();
            builder.Services.AddTransient<CatchesViewModel>();

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
