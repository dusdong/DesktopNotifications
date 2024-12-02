using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using DesktopNotifications.FreeDesktop;
using DesktopNotifications.Windows;
using DesktopNotifications.Apple;
using System.Runtime.InteropServices;

namespace DesktopNotifications.Avalonia;

/// <summary>
/// Extensions for <see cref="AppBuilder" />
/// </summary>
public static class AppBuilderExtensions
{
    /// <summary>
    /// Setups the <see cref="INotificationManager" /> for the current platform and
    /// binds it to the service locator (<see cref="AvaloniaLocator" />).
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static AppBuilder SetupDesktopNotifications(this AppBuilder builder, out INotificationManager? manager)
    {
        manager = null;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var context = WindowsApplicationContext.FromCurrentProcess("Babble App");
            manager = new WindowsNotificationManager(context);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            manager = new AppleNotificationManager();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var context = FreeDesktopApplicationContext.FromCurrentProcess("Icon_512x512.png"); // From Babble.Avalonia.Desktop
            manager = new FreeDesktopNotificationManager(context);
        }

        manager.Initialize().GetAwaiter().GetResult();

        var manager_ = manager;
        builder.AfterSetup(b =>
        {
            if (b.Instance?.ApplicationLifetime is IControlledApplicationLifetime lifetime)
            {
                lifetime.Exit += (s, e) => { manager_.Dispose(); };
            }
        });

        return builder;
    }
}