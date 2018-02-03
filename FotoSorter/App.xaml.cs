using System.Windows;
using Serilog;


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
               .WriteTo.LiterateConsole()
               .WriteTo.RollingFile("c:\\temp\\logs\\FotoSorter-{Date}.txt")
               .CreateLogger();

            Log.Information("Starting FotoSorter!");

        }
    }
}
