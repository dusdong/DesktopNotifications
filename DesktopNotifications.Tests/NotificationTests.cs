using FluentAssertions;
using Xunit;

namespace DesktopNotifications.Tests
{
    /// <summary>
    /// Notification类的单元测试
    /// </summary>
    public class NotificationTests
    {
        [Fact]
        public void Constructor_ShouldInitializeButtonsCollection()
        {
            // Arrange & Act
            var notification = new Notification();

            // Assert
            notification.Buttons.Should().NotBeNull();
            notification.Buttons.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_ShouldSetDefaultValues()
        {
            // Arrange & Act
            var notification = new Notification();

            // Assert
            notification.Title.Should().BeNull();
            notification.Body.Should().BeNull();
            notification.BodyImagePath.Should().BeNull();
            notification.BodyImageAltText.Should().Be("Image");
        }

        [Fact]
        public void Properties_ShouldAllowSettingValues()
        {
            // Arrange
            var notification = new Notification();
            const string title = "Test Title";
            const string body = "Test Body";
            const string imagePath = "/path/to/image.png";
            const string altText = "Test Alt Text";

            // Act
            notification.Title = title;
            notification.Body = body;
            notification.BodyImagePath = imagePath;
            notification.BodyImageAltText = altText;

            // Assert
            notification.Title.Should().Be(title);
            notification.Body.Should().Be(body);
            notification.BodyImagePath.Should().Be(imagePath);
            notification.BodyImageAltText.Should().Be(altText);
        }

        [Fact]
        public void Buttons_ShouldAllowAddingButtons()
        {
            // Arrange
            var notification = new Notification();

            // Act
            notification.Buttons.Add(("Yes", "action_yes"));
            notification.Buttons.Add(("No", "action_no"));

            // Assert
            notification.Buttons.Should().HaveCount(2);
            notification.Buttons[0].Should().Be(("Yes", "action_yes"));
            notification.Buttons[1].Should().Be(("No", "action_no"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Valid Title")]
        public void Title_ShouldAcceptVariousValues(string? title)
        {
            // Arrange
            var notification = new Notification();

            // Act
            notification.Title = title;

            // Assert
            notification.Title.Should().Be(title);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Valid Body")]
        public void Body_ShouldAcceptVariousValues(string? body)
        {
            // Arrange
            var notification = new Notification();

            // Act
            notification.Body = body;

            // Assert
            notification.Body.Should().Be(body);
        }
    }
} 