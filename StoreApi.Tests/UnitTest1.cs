using Moq;

namespace StoreApi.Tests;

// Example interface to mock
public interface ICalculator
{
    int Add(int a, int b);
    int Multiply(int a, int b);
}

// Example service that depends on ICalculator
public class MathService
{
    private readonly ICalculator _calculator;

    public MathService(ICalculator calculator)
    {
        _calculator = calculator;
    }

    public int CalculateSum(int a, int b)
    {
        return _calculator.Add(a, b);
    }

    public int CalculateProduct(int a, int b)
    {
        return _calculator.Multiply(a, b);
    }
}

// Simple class without dependencies - no need to mock
public class StringHelper
{
    public string Reverse(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        char[] chars = input.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    public bool IsPalindrome(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        string reversed = Reverse(input);
        return input.Equals(reversed, StringComparison.OrdinalIgnoreCase);
    }

    public int CountVowels(string input)
    {
        if (string.IsNullOrEmpty(input))
            return 0;

        return input.Count(c => "aeiouAEIOU".Contains(c));
    }
}

public class UnitTest1
{
    [Fact]
    public void Add_ShouldReturnCorrectSum()
    {
        // Arrange - Setup mock
        var mockCalculator = new Mock<ICalculator>();
        mockCalculator.Setup(x => x.Add(2, 3)).Returns(5);

        var service = new MathService(mockCalculator.Object);

        // Act
        var result = service.CalculateSum(2, 3);

        // Assert
        Assert.Equal(5, result);
        mockCalculator.Verify(x => x.Add(2, 3), Times.Once);
    }

    [Fact]
    public void Multiply_ShouldReturnCorrectProduct()
    {
        // Arrange
        var mockCalculator = new Mock<ICalculator>();
        mockCalculator.Setup(x => x.Multiply(4, 5)).Returns(20);

        var service = new MathService(mockCalculator.Object);

        // Act
        var result = service.CalculateProduct(4, 5);

        // Assert
        Assert.Equal(20, result);
        mockCalculator.Verify(x => x.Multiply(4, 5), Times.Once);
    }

    [Fact]
    public void Calculator_ShouldVerifyMethodWasNeverCalled()
    {
        // Arrange
        var mockCalculator = new Mock<ICalculator>();
        var service = new MathService(mockCalculator.Object);

        // Act - Don't call anything

        // Assert
        mockCalculator.Verify(x => x.Add(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    // Tests without mocks - testing pure logic
    [Fact]
    public void Reverse_ShouldReverseString()
    {
        // Arrange
        var helper = new StringHelper();

        // Act
        var result = helper.Reverse("hello");

        // Assert
        Assert.Equal("olleh", result);
    }

    [Fact]
    public void IsPalindrome_ShouldReturnTrue_ForPalindromeString()
    {
        // Arrange
        var helper = new StringHelper();

        // Act & Assert
        Assert.True(helper.IsPalindrome("racecar"));
        Assert.True(helper.IsPalindrome("Madam"));
        Assert.True(helper.IsPalindrome("A"));
    }

    [Fact]
    public void IsPalindrome_ShouldReturnFalse_ForNonPalindromeString()
    {
        // Arrange
        var helper = new StringHelper();

        // Act & Assert
        Assert.False(helper.IsPalindrome("hello"));
        Assert.False(helper.IsPalindrome("world"));
    }

    [Fact]
    public void CountVowels_ShouldReturnCorrectCount()
    {
        // Arrange
        var helper = new StringHelper();

        // Act
        var result = helper.CountVowels("hello world");

        // Assert
        Assert.Equal(3, result);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("bcdfg", 0)]
    [InlineData("aeiou", 5)]
    [InlineData("AEIOU", 5)]
    [InlineData("Programming", 3)]
    public void CountVowels_VariousInputs_ShouldReturnCorrectCount(string input, int expected)
    {
        // Arrange
        var helper = new StringHelper();

        // Act
        var result = helper.CountVowels(input);

        // Assert
        Assert.Equal(expected, result);
    }
}