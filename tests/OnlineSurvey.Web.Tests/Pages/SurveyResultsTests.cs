using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OnlineSurvey.Web.Models;
using OnlineSurvey.Web.Pages.Surveys;
using OnlineSurvey.Web.Services;

namespace OnlineSurvey.Web.Tests.Pages;

public class SurveyResultsTests : BunitContext
{
    private readonly Mock<ISurveyApiService> _surveyServiceMock;

    public SurveyResultsTests()
    {
        _surveyServiceMock = new Mock<ISurveyApiService>();
        Services.AddSingleton(_surveyServiceMock.Object);
    }

    [Fact]
    public void ShouldShowLoadingMessage_WhenLoading()
    {
        // Arrange
        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(It.IsAny<Guid>()))
            .Returns(new TaskCompletionSource<SurveyResultResponse?>().Task);

        // Act
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, Guid.NewGuid()));

        // Assert
        cut.Markup.Should().Contain("Carregando resultados...");
    }

    [Fact]
    public async Task ShouldShowNotFoundMessage_WhenResultsIsNull()
    {
        // Arrange
        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((SurveyResultResponse?)null);

        // Act
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, Guid.NewGuid()));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("não encontrados"));
        cut.Find(".alert-danger").TextContent.Should().Contain("Resultados não encontrados");
    }

    [Fact]
    public async Task ShouldRenderSurveyTitle()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var results = CreateSurveyResultResponse(surveyId, "Customer Satisfaction Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        // Act
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Customer Satisfaction Survey"));
        cut.Find("h1").TextContent.Should().Contain("Customer Satisfaction Survey");
    }

    [Fact]
    public async Task ShouldRenderTotalResponses()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var results = CreateSurveyResultResponse(surveyId, "Survey", totalResponses: 42);

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        // Act
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("42"));
        cut.Find(".lead").TextContent.Should().Contain("Total de respostas");
        cut.Find(".lead strong").TextContent.Should().Contain("42");
    }

    [Fact]
    public async Task ShouldRenderQuestions()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var results = CreateSurveyResultResponse(surveyId, "Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        // Act
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Sample Question"));
        cut.Find(".card-header strong").TextContent.Should().Contain("Sample Question");
    }

    [Fact]
    public async Task ShouldRenderOptions_WithCountsAndPercentages()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var results = CreateSurveyResultResponse(surveyId, "Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        // Act
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Option A"));
        cut.Markup.Should().Contain("Option A");
        cut.Markup.Should().Contain("Option B");
        cut.Markup.Should().Contain("votos");
    }

    [Fact]
    public async Task ShouldRenderProgressBars()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var results = CreateSurveyResultResponse(surveyId, "Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        // Act
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("progress"));
        var progressBars = cut.FindAll(".progress-bar");
        progressBars.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task ShouldApplyCorrectProgressBarColor_ForHighPercentage()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var results = new SurveyResultResponse(
            SurveyId: surveyId,
            SurveyTitle: "Survey",
            TotalResponses: 100,
            Questions:
            [
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Question",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "High Vote", 60, 60.0)
                    ])
            ]);

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        // Act
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("bg-success"));
        cut.Find(".progress-bar").ClassList.Should().Contain("bg-success");
    }

    [Fact]
    public async Task ShouldApplyCorrectProgressBarColor_ForMediumPercentage()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var results = new SurveyResultResponse(
            SurveyId: surveyId,
            SurveyTitle: "Survey",
            TotalResponses: 100,
            Questions:
            [
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Question",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "Medium Vote", 30, 30.0)
                    ])
            ]);

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        // Act
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("bg-info"));
        cut.Find(".progress-bar").ClassList.Should().Contain("bg-info");
    }

    [Fact]
    public async Task ShouldApplyCorrectProgressBarColor_ForLowPercentage()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var results = new SurveyResultResponse(
            SurveyId: surveyId,
            SurveyTitle: "Survey",
            TotalResponses: 100,
            Questions:
            [
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Question",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "Low Vote", 15, 15.0)
                    ])
            ]);

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        // Act
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("bg-warning"));
        cut.Find(".progress-bar").ClassList.Should().Contain("bg-warning");
    }

    [Fact]
    public async Task ShouldApplyCorrectProgressBarColor_ForVeryLowPercentage()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var results = new SurveyResultResponse(
            SurveyId: surveyId,
            SurveyTitle: "Survey",
            TotalResponses: 100,
            Questions:
            [
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Question",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "Very Low Vote", 5, 5.0)
                    ])
            ]);

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        // Act
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("bg-secondary"));
        cut.Find(".progress-bar").ClassList.Should().Contain("bg-secondary");
    }

    [Fact]
    public async Task ShouldRenderBackToSurveysLink()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var results = CreateSurveyResultResponse(surveyId, "Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        // Act
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Voltar"));
        var backLink = cut.Find("a.btn-outline-primary");
        backLink.GetAttribute("href").Should().Be("/surveys");
        backLink.TextContent.Should().Contain("Voltar às Pesquisas");
    }

    [Fact]
    public async Task ShouldRenderMultipleQuestions()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var results = new SurveyResultResponse(
            SurveyId: surveyId,
            SurveyTitle: "Multi-Question Survey",
            TotalResponses: 50,
            Questions:
            [
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Question 1",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "Option 1A", 30, 60.0),
                        new OptionResultResponse(Guid.NewGuid(), "Option 1B", 20, 40.0)
                    ]),
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Question 2",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "Option 2A", 25, 50.0),
                        new OptionResultResponse(Guid.NewGuid(), "Option 2B", 25, 50.0)
                    ])
            ]);

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        // Act
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Question 1"));
        cut.Markup.Should().Contain("Question 1");
        cut.Markup.Should().Contain("Question 2");
        cut.FindAll(".card").Count.Should().Be(2);
    }

    [Fact]
    public async Task ShouldFormatPercentage_WithTwoDecimalPlaces()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var results = new SurveyResultResponse(
            SurveyId: surveyId,
            SurveyTitle: "Survey",
            TotalResponses: 3,
            Questions:
            [
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Question",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "Option", 1, 33.33)
                    ])
            ]);

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        // Act
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("33.33%"));
        cut.Markup.Should().Contain("33.33%");
    }

    private static SurveyResultResponse CreateSurveyResultResponse(
        Guid surveyId,
        string title,
        int totalResponses = 10)
    {
        return new SurveyResultResponse(
            SurveyId: surveyId,
            SurveyTitle: title,
            TotalResponses: totalResponses,
            Questions:
            [
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Sample Question?",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "Option A", 6, 60.0),
                        new OptionResultResponse(Guid.NewGuid(), "Option B", 4, 40.0)
                    ])
            ]);
    }
}
