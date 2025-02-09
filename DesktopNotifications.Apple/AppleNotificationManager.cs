using System;
using System.Diagnostics;
using System.Threading.Tasks;

#pragma warning disable 0067

namespace DesktopNotifications.Apple
{
    public class AppleNotificationManager : INotificationManager
    {
        public void Dispose()
        {
        }

        public NotificationManagerCapabilities Capabilities => NotificationManagerCapabilities.BodyText;

        public event EventHandler<NotificationActivatedEventArgs>? NotificationActivated;
        public event EventHandler<NotificationDismissedEventArgs>? NotificationDismissed;

        public string? LaunchActionId { get; }

        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        public Task ShowNotification(Notification notification, DateTimeOffset? expirationTime = null)
        {
            ExecuteBashCommand($"osascript -e 'display notification \"{notification.Body}\" with title \"{notification.Title}\"'");
            return Task.CompletedTask;
        }

        private void ExecuteBashCommand(string command)
        {
            // According to: https://stackoverflow.com/a/15262019/637142/
            // And https://stackoverflow.com/questions/23029218/run-bash-commands-from-mono-c-sharp
            // Thanks to this we will pass everything as one command

            command = command.Replace("\"","\"\"");

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \""+ command + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();
        }

        public Task ScheduleNotification(Notification notification, DateTimeOffset deliveryTime,
            DateTimeOffset? expirationTime = null)
        {
            return Task.CompletedTask;
        }

        public Task HideNotification(Notification notification)
        {
            return Task.CompletedTask;
        }
    }
}
