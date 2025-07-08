using System;

namespace DesktopNotifications
{
    /// <summary>
    /// 表示应用程序上下文，包含应用程序的基本信息
    /// </summary>
    public class ApplicationContext
    {
        /// <summary>
        /// 初始化应用程序上下文
        /// </summary>
        /// <param name="name">应用程序名称</param>
        /// <exception cref="ArgumentNullException">当name为null时抛出</exception>
        public ApplicationContext(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// 获取应用程序名称
        /// </summary>
        public string Name { get; }
    }
}