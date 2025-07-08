using FluentAssertions;
using Xunit;

namespace DesktopNotifications.Tests
{
    /// <summary>
    /// 通知异常类型的单元测试
    /// </summary>
    public class NotificationExceptionsTests
    {
        [Fact]
        public void NotificationDeliveryException_WithMessage_ShouldSetMessage()
        {
            // Arrange
            const string message = "Test delivery error";

            // Act
            var exception = new NotificationDeliveryException(message);

            // Assert
            exception.Message.Should().Be(message);
            exception.ErrorCode.Should().BeNull();
        }

        [Fact]
        public void NotificationDeliveryException_WithMessageAndErrorCode_ShouldSetBoth()
        {
            // Arrange
            const string message = "Test delivery error";
            const int errorCode = 12345;

            // Act
            var exception = new NotificationDeliveryException(message, errorCode);

            // Assert
            exception.Message.Should().Be(message);
            exception.ErrorCode.Should().Be(errorCode);
        }

        [Fact]
        public void NotificationDeliveryException_WithInnerException_ShouldSetInnerException()
        {
            // Arrange
            const string message = "Test delivery error";
            var innerException = new InvalidOperationException("Inner error");

            // Act
            var exception = new NotificationDeliveryException(message, innerException);

            // Assert
            exception.Message.Should().Be(message);
            exception.InnerException.Should().Be(innerException);
        }

        [Fact]
        public void NotificationInitializationException_WithPlatform_ShouldSetPlatform()
        {
            // Arrange
            const string platform = "Windows";
            const string message = "Initialization failed";

            // Act
            var exception = new NotificationInitializationException(platform, message);

            // Assert
            exception.Platform.Should().Be(platform);
            exception.Message.Should().Be(message);
        }

        [Fact]
        public void NotificationSchedulingException_WithScheduledTime_ShouldSetScheduledTime()
        {
            // Arrange
            var scheduledTime = DateTimeOffset.Now.AddHours(1);
            const string message = "Scheduling failed";

            // Act
            var exception = new NotificationSchedulingException(scheduledTime, message);

            // Assert
            exception.ScheduledTime.Should().Be(scheduledTime);
            exception.Message.Should().Be(message);
        }

        [Fact]
        public void NotificationExceptions_ShouldInheritFromNotificationException()
        {
            // Arrange & Act
            var deliveryException = new NotificationDeliveryException("test");
            var initException = new NotificationInitializationException("platform", "test");
            var schedulingException = new NotificationSchedulingException(DateTimeOffset.Now, "test");

            // Assert
            deliveryException.Should().BeAssignableTo<NotificationException>();
            initException.Should().BeAssignableTo<NotificationException>();
            schedulingException.Should().BeAssignableTo<NotificationException>();
        }

        [Fact]
        public void NotificationException_ShouldInheritFromException()
        {
            // Arrange & Act
            var exception = new NotificationDeliveryException("test");

            // Assert
            exception.Should().BeAssignableTo<Exception>();
        }
    }
} 