using FluentAssertions;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Exceptions;

namespace OnlineSurvey.Domain.Tests.Entities;

public class OptionTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateOption()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var text = "Option A";
        var order = 1;

        // Act
        var option = new Option(questionId, text, order);

        // Assert
        option.QuestionId.Should().Be(questionId);
        option.Text.Should().Be(text);
        option.Order.Should().Be(order);
        option.Id.Should().NotBeEmpty();
        option.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidText_ShouldThrowDomainException(string? invalidText)
    {
        // Arrange
        var questionId = Guid.NewGuid();

        // Act
        var act = () => new Option(questionId, invalidText!, 1);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Option text cannot be empty.");
    }

    [Fact]
    public void Constructor_WithTextExceeding200Characters_ShouldThrowDomainException()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var longText = new string('a', 201);

        // Act
        var act = () => new Option(questionId, longText, 1);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Option text cannot exceed 200 characters.");
    }

    [Fact]
    public void SetText_WithValidText_ShouldUpdateText()
    {
        // Arrange
        var option = new Option(Guid.NewGuid(), "Original text", 1);
        var newText = "Updated text";

        // Act
        option.SetText(newText);

        // Assert
        option.Text.Should().Be(newText);
        option.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SetText_WithInvalidText_ShouldThrowDomainException(string? invalidText)
    {
        // Arrange
        var option = new Option(Guid.NewGuid(), "Valid text", 1);

        // Act
        var act = () => option.SetText(invalidText!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Option text cannot be empty.");
    }

    [Fact]
    public void SetText_WithTextExceeding200Characters_ShouldThrowDomainException()
    {
        // Arrange
        var option = new Option(Guid.NewGuid(), "Valid text", 1);
        var longText = new string('b', 201);

        // Act
        var act = () => option.SetText(longText);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Option text cannot exceed 200 characters.");
    }

    [Fact]
    public void SetOrder_WithValidOrder_ShouldUpdateOrder()
    {
        // Arrange
        var option = new Option(Guid.NewGuid(), "Test option", 1);

        // Act
        option.SetOrder(5);

        // Assert
        option.Order.Should().Be(5);
        option.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetOrder_WithZero_ShouldUpdateOrder()
    {
        // Arrange
        var option = new Option(Guid.NewGuid(), "Test option", 1);

        // Act
        option.SetOrder(0);

        // Assert
        option.Order.Should().Be(0);
    }

    [Fact]
    public void SetOrder_WithNegativeOrder_ShouldThrowDomainException()
    {
        // Arrange
        var option = new Option(Guid.NewGuid(), "Test option", 1);

        // Act
        var act = () => option.SetOrder(-1);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Option order cannot be negative.");
    }

    [Fact]
    public void SetText_WithExactly200Characters_ShouldSucceed()
    {
        // Arrange
        var option = new Option(Guid.NewGuid(), "Test", 1);
        var text200Chars = new string('a', 200);

        // Act
        option.SetText(text200Chars);

        // Assert
        option.Text.Should().Be(text200Chars);
    }
}
