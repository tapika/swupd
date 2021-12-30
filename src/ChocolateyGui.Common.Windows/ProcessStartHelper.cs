using Autofac;
using ChocolateyGui.Common.Windows.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace ChocolateyGui.Common.Windows
{
    public static class ProcessStartHelper
    {
        /// <summary>
        /// Open url in external web browser
        /// See also: https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
        /// </summary>
        /// <param name="url">url to open</param>
        public static void OpenUrl(string url)
        {
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
               MessageBox.Show(
                    $"Invalid URL '{url}' - must start from http:// or https://",
                    "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
