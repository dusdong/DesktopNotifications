using System;
using System.Threading.Tasks;

namespace DesktopNotifications
{
    /// <summary>
    /// Interface for notification managers that handle the presentation and lifetime of notifications.
    /// </summary>
    public interface INotificationManager : IDisposable
    {
        /// <summary>
        /// The action identifier the process was launched with.
        /// <remarks>
        /// "default" denotes the platform-specific default action.
        /// On Windows this means the user simply clicked the notification body.
        /// </remarks>
        /// </summary>
        string? LaunchActionId { get; }

        /// <summary>
        /// Retrieve the capabilities of the notification manager (and its respective platform backend)
        /// </summary>
        NotificationManagerCapabilities Capabilities { get; }

        /// <summary>
        /// Raised when a notification was activated. The notion of "activation" varies from platform to platform.
        /// </summary>
        event EventHandler<NotificationActivatedEventArgs> NotificationActivated;

        /// <summary>
        /// Raised when a notification was dismissed. The exact reason can be found in
        /// <see cref="NotificationDismissedEventArgs" />.
        /// </summary>
        event EventHandler<NotificationDismissedEventArgs> NotificationDismissed;

        /// <summary>
        /// Initializes the notification manager.
        /// </summary>
        /// <returns>A task representing the asynchronous initialization operation.</returns>
        /// <exception cref="NotificationInitializationException">Thrown when initialization fails.</exception>
        Task Initialize();

        /// <summary>
        /// Schedules a notification for delivery.
        /// </summary>
        /// <param name="notification">The notification to present.</param>
        /// <param name="expirationTime">The expiration time marking the point when the notification gets removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when notification is null.</exception>
        /// <exception cref="ArgumentException">Thrown when expirationTime is in the past.</exception>
        /// <exception cref="NotificationDeliveryException">Thrown when the notification delivery fails.</exception>
        Task ShowNotification(Notification notification, DateTimeOffset? expirationTime = null);

        /// <summary>
        /// Hides an already delivered notification (if possible).
        /// If the notification is scheduled for delivery the schedule will be cancelled.
        /// </summary>
        /// <param name="notification">The notification to hide</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when notification is null.</exception>
        Task HideNotification(Notification notification);

        /// <summary>
        /// 调度一个通知在指定时间发送
        /// </summary>
        /// <param name="notification">要发送的通知</param>
        /// <param name="deliveryTime">通知发送时间</param>
        /// <param name="expirationTime">通知过期时间（可选）</param>
        /// <returns>表示异步操作的任务</returns>
        /// <exception cref="ArgumentNullException">当notification为null时抛出</exception>
        /// <exception cref="NotificationSchedulingException">当deliveryTime无效时抛出</exception>
        Task ScheduleNotification(
            Notification notification,
            DateTimeOffset deliveryTime,
            DateTimeOffset? expirationTime = null);
    }
}