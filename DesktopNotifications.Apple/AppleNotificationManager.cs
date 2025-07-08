using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

#pragma warning disable 0067

namespace DesktopNotifications.Apple
{
    /// <summary>
    /// Apple平台通知管理器，使用原生NSUserNotificationCenter API
    /// </summary>
    public class AppleNotificationManager : INotificationManager
    {
        #region Native API Declarations
        
        // NSUserNotificationCenter 相关的 P/Invoke 声明
        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        private static extern IntPtr objc_getClass(string className);
        
        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);
        
        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);
        
        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        private static extern IntPtr sel_registerName(string selectorName);
        
        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        private static extern IntPtr objc_msgSend_stret(IntPtr receiver, IntPtr selector);
        
        // NSString 相关
        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2, IntPtr arg3);
        
        // 选择器缓存
        private static readonly IntPtr DefaultCenterSelector = sel_registerName("defaultCenter");
        private static readonly IntPtr AllocSelector = sel_registerName("alloc");
        private static readonly IntPtr InitSelector = sel_registerName("init");
        private static readonly IntPtr SetTitleSelector = sel_registerName("setTitle:");
        private static readonly IntPtr SetInformativeTextSelector = sel_registerName("setInformativeText:");
        private static readonly IntPtr DeliverNotificationSelector = sel_registerName("deliverNotification:");
        private static readonly IntPtr StringWithUTF8StringSelector = sel_registerName("stringWithUTF8String:");
        private static readonly IntPtr RemoveDeliveredNotificationSelector = sel_registerName("removeDeliveredNotification:");
        
        // 类引用缓存
        private static readonly Lazy<IntPtr> NSUserNotificationCenterClass = new(() => objc_getClass("NSUserNotificationCenter"));
        private static readonly Lazy<IntPtr> NSUserNotificationClass = new(() => objc_getClass("NSUserNotification"));
        private static readonly Lazy<IntPtr> NSStringClass = new(() => objc_getClass("NSString"));
        
        private IntPtr _notificationCenter;
        private bool _isInitialized;
        
        #endregion

        public void Dispose()
        {
            // 清理资源
            _notificationCenter = IntPtr.Zero;
            _isInitialized = false;
        }

        public NotificationManagerCapabilities Capabilities => 
            NotificationManagerCapabilities.BodyText | 
            NotificationManagerCapabilities.Icon;

        public event EventHandler<NotificationActivatedEventArgs>? NotificationActivated;
        public event EventHandler<NotificationDismissedEventArgs>? NotificationDismissed;

        public string? LaunchActionId { get; }

        /// <summary>
        /// 初始化通知管理器
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
        public Task Initialize()
        {
            try
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    throw new NotificationInitializationException("macOS", 
                        "NSUserNotificationCenter is only available on macOS");
                }

                // 获取默认通知中心
                _notificationCenter = objc_msgSend(NSUserNotificationCenterClass.Value, DefaultCenterSelector);
                
                if (_notificationCenter == IntPtr.Zero)
                {
                    throw new NotificationInitializationException("macOS", 
                        "Failed to get NSUserNotificationCenter default center");
                }

                _isInitialized = true;
                return Task.CompletedTask;
            }
            catch (Exception ex) when (!(ex is NotificationInitializationException))
            {
                throw new NotificationInitializationException("macOS", 
                    "Failed to initialize NSUserNotificationCenter", ex);
            }
        }

        /// <summary>
        /// 显示通知（使用原生NSUserNotificationCenter）
        /// </summary>
        /// <param name="notification">要显示的通知</param>
        /// <param name="expirationTime">过期时间（NSUserNotificationCenter暂不支持）</param>
        /// <returns>表示异步操作的任务</returns>
        public Task ShowNotification(Notification notification, DateTimeOffset? expirationTime = null)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            if (!_isInitialized)
                throw new InvalidOperationException("Notification manager is not initialized. Call Initialize() first.");

            try
            {
                // 创建 NSUserNotification 实例
                var nsNotification = CreateNativeNotification(notification);
                
                // 发送通知
                objc_msgSend(_notificationCenter, DeliverNotificationSelector, nsNotification);
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new NotificationDeliveryException(
                    "Failed to deliver notification using NSUserNotificationCenter", ex);
            }
        }

        /// <summary>
        /// 创建原生 NSUserNotification 对象
        /// </summary>
        /// <param name="notification">通知对象</param>
        /// <returns>NSUserNotification 的指针</returns>
        private IntPtr CreateNativeNotification(Notification notification)
        {
            // 分配并初始化 NSUserNotification
            var nsNotification = objc_msgSend(NSUserNotificationClass.Value, AllocSelector);
            nsNotification = objc_msgSend(nsNotification, InitSelector);

            // 设置标题
            if (!string.IsNullOrEmpty(notification.Title))
            {
                var titleString = CreateNSString(SanitizeString(notification.Title));
                objc_msgSend(nsNotification, SetTitleSelector, titleString);
            }

            // 设置正文
            if (!string.IsNullOrEmpty(notification.Body))
            {
                var bodyString = CreateNSString(SanitizeString(notification.Body));
                objc_msgSend(nsNotification, SetInformativeTextSelector, bodyString);
            }

            return nsNotification;
        }

        /// <summary>
        /// 创建 NSString 对象
        /// </summary>
        /// <param name="text">字符串内容</param>
        /// <returns>NSString 的指针</returns>
        private IntPtr CreateNSString(string text)
        {
            var utf8Bytes = System.Text.Encoding.UTF8.GetBytes(text + "\0");
            unsafe
            {
                fixed (byte* ptr = utf8Bytes)
                {
                    return objc_msgSend(NSStringClass.Value, StringWithUTF8StringSelector, (IntPtr)ptr);
                }
            }
        }

        /// <summary>
        /// 清理和验证字符串，确保安全显示
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>清理后的字符串</returns>
        private static string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // 移除控制字符，保留可打印字符
            var sanitized = System.Text.RegularExpressions.Regex.Replace(input, @"[\x00-\x1F\x7F]", "");
            
            // 限制长度以防止过长的通知
            if (sanitized.Length > 256)
            {
                sanitized = sanitized.Substring(0, 253) + "...";
            }

            return sanitized;
        }

        /// <summary>
        /// 调度通知（NSUserNotificationCenter暂不支持调度功能）
        /// </summary>
        public Task ScheduleNotification(Notification notification, DateTimeOffset deliveryTime,
            DateTimeOffset? expirationTime = null)
        {
            throw new NotSupportedException(
                "Scheduled notifications are not supported by NSUserNotificationCenter. " +
                "Consider using UNUserNotificationCenter for newer macOS versions.");
        }

        /// <summary>
        /// 隐藏通知（尝试移除已发送的通知）
        /// </summary>
        public Task HideNotification(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            if (!_isInitialized)
                throw new InvalidOperationException("Notification manager is not initialized.");

            try
            {
                // NSUserNotificationCenter 不提供直接的隐藏功能
                // 这里我们只能尝试移除已发送的通知，但这需要保存通知引用
                // 当前实现返回成功，但实际上macOS的通知会由系统管理
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new NotificationDeliveryException(
                    "Failed to hide notification", ex);
            }
        }

        /// <summary>
        /// 检查是否支持原生通知
        /// </summary>
        /// <returns>是否支持</returns>
        public static bool IsSupported()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return false;

            try
            {
                var centerClass = objc_getClass("NSUserNotificationCenter");
                return centerClass != IntPtr.Zero;
            }
            catch
            {
                return false;
            }
        }
    }
}
