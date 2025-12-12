using FluentAssertions;
using OnlineSurvey.Domain.Entities;

namespace OnlineSurvey.Domain.Tests.Entities;

public class EntityTests
{
    [Fact]
    public void Equals_WithSameReference_ShouldReturnTrue()
    {
        // Arrange
        var survey = new Survey("Test Survey");

        // Act & Assert
        survey.Equals(survey).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        var survey1 = new Survey("Test Survey 1");
        var survey2 = new Survey("Test Survey 2");

        // Use reflection to set the same ID
        SetEntityId(survey2, survey1.Id);

        // Act & Assert
        survey1.Equals(survey2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        var survey1 = new Survey("Test Survey 1");
        var survey2 = new Survey("Test Survey 2");

        // Act & Assert
        survey1.Equals(survey2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var survey = new Survey("Test Survey");

        // Act & Assert
        survey.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var survey = new Survey("Test Survey");
        var question = new Question(survey.Id, "Test question?", 1);

        // Act & Assert
        survey.Equals(question).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNonEntityObject_ShouldReturnFalse()
    {
        // Arrange
        var survey = new Survey("Test Survey");
        var notAnEntity = "I am not an entity";

        // Act & Assert
        survey.Equals(notAnEntity).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameId_ShouldReturnSameHashCode()
    {
        // Arrange
        var survey1 = new Survey("Test Survey 1");
        var survey2 = new Survey("Test Survey 2");

        SetEntityId(survey2, survey1.Id);

        // Act & Assert
        survey1.GetHashCode().Should().Be(survey2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentId_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var survey1 = new Survey("Test Survey 1");
        var survey2 = new Survey("Test Survey 2");

        // Act & Assert
        survey1.GetHashCode().Should().NotBe(survey2.GetHashCode());
    }

    [Fact]
    public void Constructor_ShouldSetCreatedAt()
    {
        // Arrange & Act
        var survey = new Survey("Test Survey");

        // Assert
        survey.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueId()
    {
        // Arrange & Act
        var survey1 = new Survey("Test Survey 1");
        var survey2 = new Survey("Test Survey 2");

        // Assert
        survey1.Id.Should().NotBe(survey2.Id);
        survey1.Id.Should().NotBe(Guid.Empty);
        survey2.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void UpdatedAt_ShouldBeSetAfterModification()
    {
        // Arrange
        var survey = new Survey("Test Survey");
        var initialUpdatedAt = survey.UpdatedAt;

        // Act
        survey.SetTitle("Updated Title");

        // Assert
        survey.UpdatedAt.Should().NotBeNull();
        survey.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    private static void SetEntityId(Entity entity, Guid id)
    {
        var idProperty = typeof(Entity).GetProperty("Id");
        idProperty!.SetValue(entity, id);
    }
}
