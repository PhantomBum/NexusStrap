using System.IO;
using System.Text;
using System.Windows;

namespace NexusStrap;

internal static class StartupDiagnostics
{
    public static void WriteFatal(Exception ex)
    {
        try
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NexusStrap", "Logs");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "fatal-startup.txt");
            var sb = new StringBuilder();
            sb.AppendLine(DateTime.Now.ToString("O"));
            sb.AppendLine(ex.ToString());
            File.WriteAllText(path, sb.ToString());

            MessageBox.Show(
                $"NexusStrap failed to start.\n\n{ex.Message}\n\nDetails saved to:\n{path}",
                "NexusStrap",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch
        {
            MessageBox.Show(
                $"NexusStrap failed to start:\n\n{ex.Message}\n\n{ex}",
                "NexusStrap",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
