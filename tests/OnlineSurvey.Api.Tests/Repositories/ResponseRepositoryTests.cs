using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Infrastructure.Data;
using OnlineSurvey.Infrastructure.Repositories;

namespace OnlineSurvey.Api.Tests.Repositories;

public class ResponseRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ResponseRepository _repository;
    private readonly SurveyRepository _surveyRepository;

    public ResponseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new ResponseRepository(_context);
        _surveyRepository = new SurveyRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AddAsync_ShouldAddResponse()
    {
        // Arrange
        var survey = await CreateActiveSurveyAsync();
        var response = new Response(survey.Id, "participant1", "192.168.1.1");

        // Act
        await _repository.AddAsync(response);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Responses.FindAsync(response.Id);
        result.Should().NotBeNull();
        result!.ParticipantId.Should().Be("participant1");
    }

    [Fact]
    public async Task GetBySurveyIdAsync_ShouldReturnResponsesForSurvey()
    {
        // Arrange
        var survey = await CreateActiveSurveyAsync();
        var response1 = new Response(survey.Id, "participant1", null);
        var response2 = new Response(survey.Id, "participant2", null);

        await _repository.AddAsync(response1);
        await _repository.AddAsync(response2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySurveyIdAsync(survey.Id);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetResponseCountBySurveyIdAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var survey = await CreateActiveSurveyAsync();
        await _repository.AddAsync(new Response(survey.Id, "p1", null));
        await _repository.AddAsync(new Response(survey.Id, "p2", null));
        await _repository.AddAsync(new Response(survey.Id, "p3", null));
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.GetResponseCountBySurveyIdAsync(survey.Id);

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public async Task GetResponseCountBySurveyIdAsync_WhenNoResponses_ShouldReturnZero()
    {
        // Arrange
        var survey = await CreateActiveSurveyAsync();

        // Act
        var count = await _repository.GetResponseCountBySurveyIdAsync(survey.Id);

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task HasRespondedAsync_WhenParticipantHasResponded_ShouldReturnTrue()
    {
        // Arrange
        var survey = await CreateActiveSurveyAsync();
        var response = new Response(survey.Id, "participant123", null);
        await _repository.AddAsync(response);
        await _context.SaveChangesAsync();

        // Act
        var hasResponded = await _repository.HasRespondedAsync(survey.Id, "participant123");

        // Assert
        hasResponded.Should().BeTrue();
    }

    [Fact]
    public async Task HasRespondedAsync_WhenParticipantHasNotResponded_ShouldReturnFalse()
    {
        // Arrange
        var survey = await CreateActiveSurveyAsync();

        // Act
        var hasResponded = await _repository.HasRespondedAsync(survey.Id, "participant123");

        // Assert
        hasResponded.Should().BeFalse();
    }

    [Fact]
    public async Task GetOptionCountsAsync_ShouldReturnCorrectCounts()
    {
        // Arrange
        var survey = await CreateActiveSurveyAsync();
        var question = survey.Questions.First();
        var option1 = question.Options.First();
        var option2 = question.Options.Last();

        // Create responses with answers
        var response1 = new Response(survey.Id, "p1", null);
        response1.AddAnswer(new Answer(response1.Id, question.Id, option1.Id));

        var response2 = new Response(survey.Id, "p2", null);
        response2.AddAnswer(new Answer(response2.Id, question.Id, option1.Id));

        var response3 = new Response(survey.Id, "p3", null);
        response3.AddAnswer(new Answer(response3.Id, question.Id, option2.Id));

        await _repository.AddAsync(response1);
        await _repository.AddAsync(response2);
        await _repository.AddAsync(response3);
        await _context.SaveChangesAsync();

        // Act
        var optionCounts = await _repository.GetOptionCountsAsync(survey.Id);

        // Assert
        optionCounts.Should().HaveCount(2);
        optionCounts[option1.Id].Should().Be(2);
        optionCounts[option2.Id].Should().Be(1);
    }

    [Fact]
    public async Task GetOptionCountsAsync_WhenNoResponses_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var survey = await CreateActiveSurveyAsync();

        // Act
        var optionCounts = await _repository.GetOptionCountsAsync(survey.Id);

        // Assert
        optionCounts.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBySurveyIdAsync_ShouldIncludeAnswers()
    {
        // Arrange
        var survey = await CreateActiveSurveyAsync();
        var question = survey.Questions.First();
        var option = question.Options.First();

        var response = new Response(survey.Id, "p1", null);
        response.AddAnswer(new Answer(response.Id, question.Id, option.Id));

        await _repository.AddAsync(response);
        await _context.SaveChangesAsync();

        // Act
        var responses = await _repository.GetBySurveyIdAsync(survey.Id);

        // Assert
        responses.Should().HaveCount(1);
        responses.First().Answers.Should().HaveCount(1);
    }

    private async Task<Survey> CreateActiveSurveyAsync()
    {
        var survey = new Survey("Test Survey", "Description");
        var question = new Question(survey.Id, "Test question?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));
        question.AddOption(new Option(question.Id, "Option 2", 2));
        survey.AddQuestion(question);
        survey.Activate();

        await _surveyRepository.AddAsync(survey);
        await _context.SaveChangesAsync();

        return survey;
    }
}
