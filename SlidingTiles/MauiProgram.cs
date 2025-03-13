using Microsoft.Extensions.Logging;
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

            Debug.WriteLine("MauiApp building completed");
            return builder.Build();
        }
    }
}
