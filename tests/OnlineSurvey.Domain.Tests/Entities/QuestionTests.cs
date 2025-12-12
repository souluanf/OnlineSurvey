using FluentAssertions;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Exceptions;

namespace OnlineSurvey.Domain.Tests.Entities;

public class QuestionTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateQuestion()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var text = "What is your favorite color?";
        var order = 1;

        // Act
        var question = new Question(surveyId, text, order);

        // Assert
        question.SurveyId.Should().Be(surveyId);
        question.Text.Should().Be(text);
        question.Order.Should().Be(order);
        question.IsRequired.Should().BeTrue();
        question.Options.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidText_ShouldThrowDomainException(string? invalidText)
    {
        // Act
        var act = () => new Question(Guid.NewGuid(), invalidText!, 1);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Question text cannot be empty.");
    }

    [Fact]
    public void AddOption_WithValidOption_ShouldAddToCollection()
    {
        // Arrange
        var question = new Question(Guid.NewGuid(), "Test question?", 1);
        var option = new Option(question.Id, "Option 1", 1);

        // Act
        question.AddOption(option);

        // Assert
        question.Options.Should().ContainSingle();
        question.Options.First().Should().Be(option);
    }

    [Fact]
    public void AddOption_WithDuplicateText_ShouldThrowDomainException()
    {
        // Arrange
        var question = new Question(Guid.NewGuid(), "Test question?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));

        // Act
        var act = () => question.AddOption(new Option(question.Id, "Option 1", 2));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Duplicate option text is not allowed.");
    }

    [Fact]
    public void AddOption_ExceedingMaxOptions_ShouldThrowDomainException()
    {
        // Arrange
        var question = new Question(Guid.NewGuid(), "Test question?", 1);
        for (int i = 1; i <= 10; i++)
        {
            question.AddOption(new Option(question.Id, $"Option {i}", i));
        }

        // Act
        var act = () => question.AddOption(new Option(question.Id, "Option 11", 11));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Question cannot have more than 10 options.");
    }

    [Fact]
    public void Constructor_WithTextExceeding500Characters_ShouldThrowDomainException()
    {
        // Arrange
        var longText = new string('a', 501);

        // Act
        var act = () => new Question(Guid.NewGuid(), longText, 1);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Question text cannot exceed 500 characters.");
    }

    [Fact]
    public void Constructor_WithIsRequiredFalse_ShouldCreateOptionalQuestion()
    {
        // Arrange & Act
        var question = new Question(Guid.NewGuid(), "Optional question?", 1, false);

        // Assert
        question.IsRequired.Should().BeFalse();
    }

    [Fact]
    public void SetText_WithValidText_ShouldUpdateText()
    {
        // Arrange
        var question = new Question(Guid.NewGuid(), "Original text?", 1);
        var newText = "Updated text?";

        // Act
        question.SetText(newText);

        // Assert
        question.Text.Should().Be(newText);
        question.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetText_WithTextExceeding500Characters_ShouldThrowDomainException()
    {
        // Arrange
        var question = new Question(Guid.NewGuid(), "Original text?", 1);
        var longText = new string('a', 501);

        // Act
        var act = () => question.SetText(longText);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Question text cannot exceed 500 characters.");
    }

    [Fact]
    public void SetOrder_WithValidOrder_ShouldUpdateOrder()
    {
        // Arrange
        var question = new Question(Guid.NewGuid(), "Test question?", 1);

        // Act
        question.SetOrder(5);

        // Assert
        question.Order.Should().Be(5);
        question.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetOrder_WithZero_ShouldUpdateOrder()
    {
        // Arrange
        var question = new Question(Guid.NewGuid(), "Test question?", 1);

        // Act
        question.SetOrder(0);

        // Assert
        question.Order.Should().Be(0);
    }

    [Fact]
    public void SetOrder_WithNegativeOrder_ShouldThrowDomainException()
    {
        // Arrange
        var question = new Question(Guid.NewGuid(), "Test question?", 1);

        // Act
        var act = () => question.SetOrder(-1);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Question order cannot be negative.");
    }

    [Fact]
    public void RemoveOption_WithValidId_ShouldRemoveOption()
    {
        // Arrange
        var question = new Question(Guid.NewGuid(), "Test question?", 1);
        var option = new Option(question.Id, "Option 1", 1);
        question.AddOption(option);

        // Act
        question.RemoveOption(option.Id);

        // Assert
        question.Options.Should().BeEmpty();
    }

    [Fact]
    public void RemoveOption_WithInvalidId_ShouldThrowDomainException()
    {
        // Arrange
        var question = new Question(Guid.NewGuid(), "Test question?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));

        // Act
        var act = () => question.RemoveOption(Guid.NewGuid());

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Option not found.");
    }

    [Fact]
    public void AddOption_WithDuplicateTextCaseInsensitive_ShouldThrowDomainException()
    {
        // Arrange
        var question = new Question(Guid.NewGuid(), "Test question?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));

        // Act
        var act = () => question.AddOption(new Option(question.Id, "OPTION 1", 2));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Duplicate option text is not allowed.");
    }
}
