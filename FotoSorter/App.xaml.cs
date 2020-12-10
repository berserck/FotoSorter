using System.Windows;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace FotoSorter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {

            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Console(theme: SystemConsoleTheme.Literate)
               .WriteTo.File("c:\\temp\\logs\\FotoSorterLog.txt", rollingInterval: RollingInterval.Day)
               .CreateLogger();

            Log.Information("Starting FotoSorter!");

        }
    }
}
