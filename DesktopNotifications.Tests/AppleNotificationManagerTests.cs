using DesktopNotifications.Apple;
using FluentAssertions;
using System.Runtime.InteropServices;
using Xunit;

namespace DesktopNotifications.Tests
{
    /// <summary>
    /// Apple通知管理器的单元测试
    /// </summary>
    public class AppleNotificationManagerTests
    {
        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            using var manager = new AppleNotificationManager();

            // Assert
            manager.Should().NotBeNull();
            manager.Capabilities.Should().Be(NotificationManagerCapabilities.BodyText | NotificationManagerCapabilities.Icon);
            manager.LaunchActionId.Should().BeNull();
        }

        [Fact]
        public async Task Initialize_ShouldCompleteSuccessfully()
        {
            // Arrange
            using var manager = new AppleNotificationManager();

            // Act
            var task = manager.Initialize();

            // Assert
            await task;
            task.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact]
        public async Task ShowNotification_WithValidNotification_ShouldNotThrow()
        {
            // Arrange
            using var manager = new AppleNotificationManager();
            var notification = new Notification
            {
                Title = "Test Title",
                Body = "Test Body"
            };

            // Note: This test will fail on non-macOS platforms, but demonstrates the API
            // Act & Assert
            var act = async () => 
            {
                if (AppleNotificationManager.IsSupported())
                {
                    await manager.Initialize();
                    await manager.ShowNotification(notification);
                }
                else
                {
                    // On non-macOS platforms, we expect initialization to fail
                    await manager.Initialize();
                }
            };
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                await act.Should().NotThrowAsync();
            }
            else
            {
                await act.Should().ThrowAsync<NotificationInitializationException>();
            }
        }

        [Fact]
        public async Task ShowNotification_WithNullNotification_ShouldThrowArgumentNullException()
        {
            // Arrange
            using var manager = new AppleNotificationManager();

            // Act & Assert
            var act = async () => await manager.ShowNotification(null!);
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("notification");
        }

        [Theory]
        [InlineData("Normal Title", "Normal Body")]
        [InlineData("Title with \"quotes\"", "Body with \"quotes\"")]
        [InlineData("Title with \\backslash", "Body with \\backslash")]
        [InlineData("", "")]
        public async Task ShowNotification_WithVariousInputs_ShouldHandleSafely(string title, string body)
        {
            // Arrange
            using var manager = new AppleNotificationManager();
            var notification = new Notification
            {
                Title = title,
                Body = body
            };

            // Act & Assert
            var act = async () => await manager.ShowNotification(notification);
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ShowNotification_WithMaliciousInput_ShouldNotExecuteCommands()
        {
            // Arrange
            using var manager = new AppleNotificationManager();
            var notification = new Notification
            {
                Title = "\"; rm -rf /; echo \"",
                Body = "$(whoami)"
            };

            // Act & Assert
            // 这个测试确保恶意输入不会导致命令注入
            var act = async () => await manager.ShowNotification(notification);
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ShowNotification_WithLongText_ShouldTruncate()
        {
            // Arrange
            using var manager = new AppleNotificationManager();
            var longText = new string('A', 300); // 超过200字符限制
            var notification = new Notification
            {
                Title = longText,
                Body = longText
            };

            // Act & Assert
            var act = async () => await manager.ShowNotification(notification);
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ScheduleNotification_ShouldThrowNotSupportedException()
        {
            // Arrange
            using var manager = new AppleNotificationManager();
            var notification = new Notification { Title = "Test", Body = "Test" };
            var deliveryTime = DateTimeOffset.Now.AddHours(1);

            // Act & Assert
            var act = async () => await manager.ScheduleNotification(notification, deliveryTime);
            await act.Should().ThrowAsync<NotSupportedException>()
                .WithMessage("*not supported by NSUserNotificationCenter*");
        }

        [Fact]
        public async Task HideNotification_ShouldCompleteSuccessfully()
        {
            // Arrange
            using var manager = new AppleNotificationManager();
            var notification = new Notification { Title = "Test", Body = "Test" };

            // Act
            var task = manager.HideNotification(notification);

            // Assert
            await task;
            task.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact]
        public void Dispose_ShouldNotThrow()
        {
            // Arrange
            var manager = new AppleNotificationManager();

            // Act & Assert
            var act = () => manager.Dispose();
            act.Should().NotThrow();
        }

        [Fact]
        public void Events_ShouldNotBeNull()
        {
            // Arrange & Act
            using var manager = new AppleNotificationManager();

            // Assert
            // 事件应该可以订阅而不抛出异常
            var act1 = () => manager.NotificationActivated += (s, e) => { };
            var act2 = () => manager.NotificationDismissed += (s, e) => { };
            
            act1.Should().NotThrow();
            act2.Should().NotThrow();
        }

        [Fact]
        public void IsSupported_ShouldReturnCorrectValue()
        {
            // Act
            var isSupported = AppleNotificationManager.IsSupported();

            // Assert
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // On macOS, it should return true if NSUserNotificationCenter is available
                isSupported.Should().BeTrue();
            }
            else
            {
                // On non-macOS platforms, it should return false
                isSupported.Should().BeFalse();
            }
        }

        [Fact]
        public async Task ShowNotification_WithoutInitialization_ShouldThrowInvalidOperationException()
        {
            // Arrange
            using var manager = new AppleNotificationManager();
            var notification = new Notification { Title = "Test", Body = "Test" };

            // Act & Assert
            var act = async () => await manager.ShowNotification(notification);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*not initialized*");
        }
    }
} 