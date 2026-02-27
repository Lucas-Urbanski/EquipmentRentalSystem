using Microsoft.Extensions.Logging;
using FinalProject.Data;

namespace FinalProject
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
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<Broker>();
            builder.Services.AddScoped<Customer>();
            builder.Services.AddScoped<Equipment>();
            builder.Services.AddScoped<Category>();
            builder.Services.AddScoped<Rental>();
            return builder.Build();
        }
    }
}