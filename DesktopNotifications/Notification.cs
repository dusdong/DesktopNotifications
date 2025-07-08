using System;
using System.Collections.Generic;

namespace DesktopNotifications
{
    /// <summary>
    /// 表示一个桌面通知，包含标题、正文、图片和按钮等信息
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// 初始化一个新的通知实例
        /// </summary>
        public Notification()
        {
            Buttons = new List<(string Title, string ActionId)>();
        }

        /// <summary>
        /// 获取或设置通知的标题
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// 获取或设置通知的正文内容
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// 获取或设置通知正文中显示的图片路径
        /// </summary>
        public string? BodyImagePath { get; set; }

        /// <summary>
        /// 获取或设置通知图片的替代文本，用于辅助功能
        /// </summary>
        public string BodyImageAltText { get; set; } = "Image";

        /// <summary>
        /// 获取通知的按钮列表，每个按钮包含标题和动作标识符
        /// </summary>
        public List<(string Title, string ActionId)> Buttons { get; }
    }
}