using Microsoft.Extensions.Logging;
using SlidingTiles.Handlers;
using System.Diagnostics;

namespace SlidingTiles
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Debug.WriteLine("CreateMauiApp called");
            
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug().SetMinimumLevel(LogLevel.Trace);
#endif

            // Register our custom picker handler for chevron color customization
            PickerHandlerCustomization.CustomizePickerHandler();

            Debug.WriteLine("MauiApp building completed");
            return builder.Build();
        }
    }
}
