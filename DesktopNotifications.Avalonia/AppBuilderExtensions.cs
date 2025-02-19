using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using DesktopNotifications.FreeDesktop;
using DesktopNotifications.Windows;
using DesktopNotifications.Apple;
using System.Runtime.InteropServices;
using System;

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
            // On Windows, setting the TargetOS to 10.0.17763.0 fixes things
            // https://github.com/pr8x/DesktopNotifications/issues/27
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
        else
        {
            // We don't have an implementation for this platform (what are you running this on??)
            manager = null;
            return builder;
        }

        try
        {
            manager.Initialize().GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            // Notifications are disabled, just skip
            return builder;
        }

        var managerCopy = manager;
        builder.AfterSetup(b =>
        {
            if (b.Instance?.ApplicationLifetime is IControlledApplicationLifetime lifetime)
            {
                lifetime.Exit += (_, _) => { managerCopy.Dispose(); };
            }
        });

        return builder;
    }
}
