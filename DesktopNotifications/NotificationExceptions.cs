using System;

namespace DesktopNotifications
{
    /// <summary>
    /// 通知操作相关异常的基类
    /// </summary>
    public abstract class NotificationException : Exception
    {
        /// <summary>
        /// 初始化通知异常
        /// </summary>
        /// <param name="message">异常消息</param>
        protected NotificationException(string message) : base(message)
        {
        }

        /// <summary>
        /// 初始化通知异常
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="innerException">内部异常</param>
        protected NotificationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// 通知发送失败时抛出的异常
    /// </summary>
    public class NotificationDeliveryException : NotificationException
    {
        /// <summary>
        /// 错误代码（平台相关）
        /// </summary>
        public object? ErrorCode { get; }

        /// <summary>
        /// 初始化通知发送异常
        /// </summary>
        /// <param name="message">异常消息</param>
        public NotificationDeliveryException(string message) : base(message)
        {
        }

        /// <summary>
        /// 初始化通知发送异常
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="errorCode">平台相关的错误代码</param>
        public NotificationDeliveryException(string message, object errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// 初始化通知发送异常
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="innerException">内部异常</param>
        public NotificationDeliveryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// 初始化通知发送异常
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="errorCode">平台相关的错误代码</param>
        /// <param name="innerException">内部异常</param>
        public NotificationDeliveryException(string message, object errorCode, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }

    /// <summary>
    /// 通知管理器初始化失败时抛出的异常
    /// </summary>
    public class NotificationInitializationException : NotificationException
    {
        /// <summary>
        /// 平台名称
        /// </summary>
        public string Platform { get; }

        /// <summary>
        /// 初始化通知初始化异常
        /// </summary>
        /// <param name="platform">平台名称</param>
        /// <param name="message">异常消息</param>
        public NotificationInitializationException(string platform, string message) : base(message)
        {
            Platform = platform;
        }

        /// <summary>
        /// 初始化通知初始化异常
        /// </summary>
        /// <param name="platform">平台名称</param>
        /// <param name="message">异常消息</param>
        /// <param name="innerException">内部异常</param>
        public NotificationInitializationException(string platform, string message, Exception innerException) : base(message, innerException)
        {
            Platform = platform;
        }
    }

    /// <summary>
    /// 通知调度失败时抛出的异常
    /// </summary>
    public class NotificationSchedulingException : NotificationException
    {
        /// <summary>
        /// 计划的发送时间
        /// </summary>
        public DateTimeOffset ScheduledTime { get; }

        /// <summary>
        /// 初始化通知调度异常
        /// </summary>
        /// <param name="scheduledTime">计划的发送时间</param>
        /// <param name="message">异常消息</param>
        public NotificationSchedulingException(DateTimeOffset scheduledTime, string message) : base(message)
        {
            ScheduledTime = scheduledTime;
        }

        /// <summary>
        /// 初始化通知调度异常
        /// </summary>
        /// <param name="scheduledTime">计划的发送时间</param>
        /// <param name="message">异常消息</param>
        /// <param name="innerException">内部异常</param>
        public NotificationSchedulingException(DateTimeOffset scheduledTime, string message, Exception innerException) : base(message, innerException)
        {
            ScheduledTime = scheduledTime;
        }
    }
} 