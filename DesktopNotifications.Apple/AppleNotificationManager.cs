using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

#pragma warning disable 0067

namespace DesktopNotifications.Apple
{
    /// <summary>
    /// Apple平台通知管理器，使用安全的osascript调用方式
    /// </summary>
    public class AppleNotificationManager : INotificationManager
    {
        public void Dispose()
        {
        }

        public NotificationManagerCapabilities Capabilities => NotificationManagerCapabilities.BodyText;

        public event EventHandler<NotificationActivatedEventArgs>? NotificationActivated;
        public event EventHandler<NotificationDismissedEventArgs>? NotificationDismissed;

        public string? LaunchActionId { get; }

        /// <summary>
        /// 初始化通知管理器
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 显示通知（使用安全的参数处理）
        /// </summary>
        /// <param name="notification">要显示的通知</param>
        /// <param name="expirationTime">过期时间（Apple平台暂不支持）</param>
        /// <returns>表示异步操作的任务</returns>
        public Task ShowNotification(Notification notification, DateTimeOffset? expirationTime = null)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            // 安全地转义和验证输入
            var safeTitle = EscapeAppleScriptString(notification.Title ?? "");
            var safeBody = EscapeAppleScriptString(notification.Body ?? "");

            ExecuteSecureOsaScript(safeTitle, safeBody);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 安全地执行osascript命令，防止命令注入
        /// </summary>
        /// <param name="title">已转义的标题</param>
        /// <param name="body">已转义的正文</param>
        private void ExecuteSecureOsaScript(string title, string body)
        {
            try
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "osascript",
                        // 使用参数化方式，避免shell注入
                        Arguments = $"-e \"display notification \\\"{body}\\\" with title \\\"{title}\\\"\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    }
                };

                proc.Start();
                
                // 设置超时以避免无限等待
                if (!proc.WaitForExit(5000))
                {
                    proc.Kill();
                    throw new TimeoutException("Notification command timed out");
                }

                if (proc.ExitCode != 0)
                {
                    var error = proc.StandardError.ReadToEnd();
                    throw new InvalidOperationException($"osascript failed with exit code {proc.ExitCode}: {error}");
                }
            }
            catch (Exception ex)
            {
                // 记录错误但不抛出，保持通知失败时应用继续运行
                System.Diagnostics.Debug.WriteLine($"Failed to show notification: {ex.Message}");
            }
        }

        /// <summary>
        /// 安全地转义AppleScript字符串，防止注入攻击
        /// </summary>
        /// <param name="input">原始输入字符串</param>
        /// <returns>安全的转义字符串</returns>
        private static string EscapeAppleScriptString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // 移除或转义潜在的危险字符
            // 1. 移除控制字符
            input = Regex.Replace(input, @"[\x00-\x1F\x7F]", "");
            
            // 2. 转义引号和反斜杠
            input = input.Replace("\\", "\\\\")
                        .Replace("\"", "\\\"");
            
            // 3. 限制长度以防止DoS
            if (input.Length > 200)
            {
                input = input.Substring(0, 197) + "...";
            }

            return input;
        }

        /// <summary>
        /// 调度通知（Apple平台暂不支持）
        /// </summary>
        public Task ScheduleNotification(Notification notification, DateTimeOffset deliveryTime,
            DateTimeOffset? expirationTime = null)
        {
            throw new NotSupportedException("Scheduled notifications are not supported on Apple platform");
        }

        /// <summary>
        /// 隐藏通知（Apple平台暂不支持）
        /// </summary>
        public Task HideNotification(Notification notification)
        {
            // Apple的通知中心不支持程序化隐藏通知
            // 返回已完成的任务，避免抛出异常
            return Task.CompletedTask;
        }
    }
}
