using FluentAssertions;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Exceptions;

namespace OnlineSurvey.Domain.Tests.Entities;

public class ResponseTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateResponse()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var participantId = "user123";
        var ipAddress = "192.168.1.1";

        // Act
        var response = new Response(surveyId, participantId, ipAddress);

        // Assert
        response.SurveyId.Should().Be(surveyId);
        response.ParticipantId.Should().Be(participantId);
        response.IpAddress.Should().Be(ipAddress);
        response.SubmittedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        response.Answers.Should().BeEmpty();
    }

    [Fact]
    public void AddAnswer_WithValidAnswer_ShouldAddToCollection()
    {
        // Arrange
        var response = new Response(Guid.NewGuid());
        var answer = new Answer(response.Id, Guid.NewGuid(), Guid.NewGuid());

        // Act
        response.AddAnswer(answer);

        // Assert
        response.Answers.Should().ContainSingle();
        response.Answers.First().Should().Be(answer);
    }

    [Fact]
    public void AddAnswer_WithDuplicateQuestionId_ShouldThrowDomainException()
    {
        // Arrange
        var response = new Response(Guid.NewGuid());
        var questionId = Guid.NewGuid();
        response.AddAnswer(new Answer(response.Id, questionId, Guid.NewGuid()));

        // Act
        var act = () => response.AddAnswer(new Answer(response.Id, questionId, Guid.NewGuid()));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Answer for this question already exists.");
    }
}
