using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace DesktopNotifications.Tests
{
    /// <summary>
    /// 扩展方法和数据结构的单元测试
    /// </summary>
    public class ExtensionsTests
    {
        #region TryGetKey Tests

        [Fact]
        public void TryGetKey_WithNullDictionary_ShouldReturnFalse()
        {
            // Arrange
            Dictionary<string, int>? dict = null;

            // Act
            var result = dict!.TryGetKey(5, out var key);

            // Assert
            result.Should().BeFalse();
            key.Should().BeNull();
        }

        [Fact]
        public void TryGetKey_WithEmptyDictionary_ShouldReturnFalse()
        {
            // Arrange
            var dict = new Dictionary<string, int>();

            // Act
            var result = dict.TryGetKey(5, out var key);

            // Assert
            result.Should().BeFalse();
            key.Should().BeNull();
        }

        [Fact]
        public void TryGetKey_WithExistingValue_ShouldReturnTrueAndCorrectKey()
        {
            // Arrange
            var dict = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 3 }
            };

            // Act
            var result = dict.TryGetKey(2, out var key);

            // Assert
            result.Should().BeTrue();
            key.Should().Be("two");
        }

        [Fact]
        public void TryGetKey_WithNonExistingValue_ShouldReturnFalse()
        {
            // Arrange
            var dict = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 3 }
            };

            // Act
            var result = dict.TryGetKey(99, out var key);

            // Assert
            result.Should().BeFalse();
            key.Should().BeNull();
        }

        [Fact]
        public void TryGetKey_WithSmallDictionary_ShouldUseDirectIteration()
        {
            // Arrange - 小于等于10个元素，应该使用直接遍历
            var dict = new Dictionary<int, string>();
            for (int i = 1; i <= 10; i++)
            {
                dict.Add(i, $"value{i}");
            }

            // Act
            var result = dict.TryGetKey("value5", out var key);

            // Assert
            result.Should().BeTrue();
            key.Should().Be(5);
        }

        [Fact]
        public void TryGetKey_WithLargeDictionary_ShouldUseLinqOptimization()
        {
            // Arrange - 大于10个元素，应该使用LINQ优化
            var dict = new Dictionary<int, string>();
            for (int i = 1; i <= 100; i++)
            {
                dict.Add(i, $"value{i}");
            }

            // Act
            var result = dict.TryGetKey("value50", out var key);

            // Assert
            result.Should().BeTrue();
            key.Should().Be(50);
        }

        [Fact]
        public void TryGetKey_WithNullValues_ShouldHandleCorrectly()
        {
            // Arrange
            var dict = new Dictionary<string, string?>
            {
                { "key1", "value1" },
                { "key2", null },
                { "key3", "value3" }
            };

            // Act & Assert
            dict.TryGetKey(null, out var key).Should().BeTrue();
            key.Should().Be("key2");

            dict.TryGetKey("value1", out key).Should().BeTrue();
            key.Should().Be("key1");
        }

        #endregion

        #region BidirectionalDictionary Tests

        [Fact]
        public void BidirectionalDictionary_Constructor_ShouldInitializeEmpty()
        {
            // Arrange & Act
            var biDict = new BidirectionalDictionary<string, int>();

            // Assert
            biDict.Count.Should().Be(0);
            biDict.FirstKeys.Should().BeEmpty();
            biDict.SecondKeys.Should().BeEmpty();
        }

        [Fact]
        public void BidirectionalDictionary_ConstructorWithCapacity_ShouldInitializeEmpty()
        {
            // Arrange & Act
            var biDict = new BidirectionalDictionary<string, int>(10);

            // Assert
            biDict.Count.Should().Be(0);
        }

        [Fact]
        public void BidirectionalDictionary_Add_ShouldAddBothDirections()
        {
            // Arrange
            var biDict = new BidirectionalDictionary<string, int>();

            // Act
            biDict.Add("one", 1);
            biDict.Add("two", 2);

            // Assert
            biDict.Count.Should().Be(2);
            biDict.GetByFirst("one").Should().Be(1);
            biDict.GetBySecond(1).Should().Be("one");
            biDict.GetByFirst("two").Should().Be(2);
            biDict.GetBySecond(2).Should().Be("two");
        }

        [Fact]
        public void BidirectionalDictionary_Add_WithDuplicateFirst_ShouldThrowException()
        {
            // Arrange
            var biDict = new BidirectionalDictionary<string, int>();
            biDict.Add("one", 1);

            // Act & Assert
            var act = () => biDict.Add("one", 2);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*already exists*")
                .And.ParamName.Should().Be("first");
        }

        [Fact]
        public void BidirectionalDictionary_Add_WithDuplicateSecond_ShouldThrowException()
        {
            // Arrange
            var biDict = new BidirectionalDictionary<string, int>();
            biDict.Add("one", 1);

            // Act & Assert
            var act = () => biDict.Add("two", 1);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*already exists*")
                .And.ParamName.Should().Be("second");
        }

        [Fact]
        public void BidirectionalDictionary_TryAdd_WithNewPair_ShouldReturnTrue()
        {
            // Arrange
            var biDict = new BidirectionalDictionary<string, int>();

            // Act
            var result = biDict.TryAdd("one", 1);

            // Assert
            result.Should().BeTrue();
            biDict.Count.Should().Be(1);
            biDict.ContainsFirst("one").Should().BeTrue();
            biDict.ContainsSecond(1).Should().BeTrue();
        }

        [Fact]
        public void BidirectionalDictionary_TryAdd_WithDuplicate_ShouldReturnFalse()
        {
            // Arrange
            var biDict = new BidirectionalDictionary<string, int>();
            biDict.Add("one", 1);

            // Act & Assert
            biDict.TryAdd("one", 2).Should().BeFalse();
            biDict.TryAdd("two", 1).Should().BeFalse();
            biDict.Count.Should().Be(1);
        }

        [Fact]
        public void BidirectionalDictionary_TryGetMethods_ShouldWorkCorrectly()
        {
            // Arrange
            var biDict = new BidirectionalDictionary<string, int>();
            biDict.Add("one", 1);
            biDict.Add("two", 2);

            // Act & Assert
            biDict.TryGetByFirst("one", out var value).Should().BeTrue();
            value.Should().Be(1);

            biDict.TryGetBySecond(2, out var key).Should().BeTrue();
            key.Should().Be("two");

            biDict.TryGetByFirst("three", out _).Should().BeFalse();
            biDict.TryGetBySecond(3, out _).Should().BeFalse();
        }

        [Fact]
        public void BidirectionalDictionary_Contains_ShouldWorkCorrectly()
        {
            // Arrange
            var biDict = new BidirectionalDictionary<string, int>();
            biDict.Add("one", 1);

            // Act & Assert
            biDict.ContainsFirst("one").Should().BeTrue();
            biDict.ContainsFirst("two").Should().BeFalse();
            biDict.ContainsSecond(1).Should().BeTrue();
            biDict.ContainsSecond(2).Should().BeFalse();
        }

        [Fact]
        public void BidirectionalDictionary_RemoveByFirst_ShouldRemoveBothDirections()
        {
            // Arrange
            var biDict = new BidirectionalDictionary<string, int>();
            biDict.Add("one", 1);
            biDict.Add("two", 2);

            // Act
            var result = biDict.RemoveByFirst("one");

            // Assert
            result.Should().BeTrue();
            biDict.Count.Should().Be(1);
            biDict.ContainsFirst("one").Should().BeFalse();
            biDict.ContainsSecond(1).Should().BeFalse();
            biDict.ContainsFirst("two").Should().BeTrue();
            biDict.ContainsSecond(2).Should().BeTrue();
        }

        [Fact]
        public void BidirectionalDictionary_RemoveBySecond_ShouldRemoveBothDirections()
        {
            // Arrange
            var biDict = new BidirectionalDictionary<string, int>();
            biDict.Add("one", 1);
            biDict.Add("two", 2);

            // Act
            var result = biDict.RemoveBySecond(1);

            // Assert
            result.Should().BeTrue();
            biDict.Count.Should().Be(1);
            biDict.ContainsFirst("one").Should().BeFalse();
            biDict.ContainsSecond(1).Should().BeFalse();
            biDict.ContainsFirst("two").Should().BeTrue();
            biDict.ContainsSecond(2).Should().BeTrue();
        }

        [Fact]
        public void BidirectionalDictionary_RemoveNonExisting_ShouldReturnFalse()
        {
            // Arrange
            var biDict = new BidirectionalDictionary<string, int>();
            biDict.Add("one", 1);

            // Act & Assert
            biDict.RemoveByFirst("two").Should().BeFalse();
            biDict.RemoveBySecond(2).Should().BeFalse();
            biDict.Count.Should().Be(1);
        }

        [Fact]
        public void BidirectionalDictionary_Clear_ShouldRemoveAllEntries()
        {
            // Arrange
            var biDict = new BidirectionalDictionary<string, int>();
            biDict.Add("one", 1);
            biDict.Add("two", 2);
            biDict.Add("three", 3);

            // Act
            biDict.Clear();

            // Assert
            biDict.Count.Should().Be(0);
            biDict.FirstKeys.Should().BeEmpty();
            biDict.SecondKeys.Should().BeEmpty();
        }

        [Fact]
        public void BidirectionalDictionary_Keys_ShouldReflectContents()
        {
            // Arrange
            var biDict = new BidirectionalDictionary<string, int>();
            biDict.Add("one", 1);
            biDict.Add("two", 2);
            biDict.Add("three", 3);

            // Act & Assert
            biDict.FirstKeys.Should().BeEquivalentTo(new[] { "one", "two", "three" });
            biDict.SecondKeys.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        }

        #endregion
    }
} 