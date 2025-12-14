using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OnlineSurvey.Web.Models;
using OnlineSurvey.Web.Pages.Surveys;
using OnlineSurvey.Web.Services;

namespace OnlineSurvey.Web.Tests.Pages;

public class SurveyAnswerTests : BunitContext
{
    private readonly Mock<ISurveyApiService> _surveyServiceMock;

    public SurveyAnswerTests()
    {
        _surveyServiceMock = new Mock<ISurveyApiService>();
        Services.AddSingleton(_surveyServiceMock.Object);
    }

    [Fact]
    public void ShouldShowLoadingMessage_WhenLoading()
    {
        // Arrange
        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .Returns(new TaskCompletionSource<SurveyDetailResponse?>().Task); // Never completes

        // Act
        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, Guid.NewGuid()));

        // Assert
        cut.Markup.Should().Contain("Carregando pesquisa...");
    }

    [Fact]
    public async Task ShouldShowSurveyNotFound_WhenSurveyIsNull()
    {
        // Arrange
        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((SurveyDetailResponse?)null);

        // Act
        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, Guid.NewGuid()));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("não encontrada"));
        cut.Find(".alert-danger").TextContent.Should().Contain("Pesquisa não encontrada");
    }

    [Fact]
    public async Task ShouldRenderSurveyTitle_WhenLoaded()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse("Test Survey Title");

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Test Survey Title"));
        cut.Find("h1").TextContent.Should().Contain("Test Survey Title");
    }

    [Fact]
    public async Task ShouldRenderDescription_WhenPresent()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse("Test Survey", description: "Test Description");

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Test Description"));
        cut.Find(".lead").TextContent.Should().Contain("Test Description");
    }

    [Fact]
    public async Task ShouldRenderQuestions()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse("Test Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Question 1"));
        cut.FindAll(".card").Count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task ShouldRenderOptions_AsRadioButtons()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse("Test Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("radio"));
        var radioButtons = cut.FindAll("input[type='radio']");
        radioButtons.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task ShouldShowRequiredIndicator_ForRequiredQuestions()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse("Test Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("text-danger"));
        cut.Find(".text-danger").TextContent.Should().Contain("*");
    }

    [Fact]
    public async Task HandleSubmit_WhenRequiredQuestionUnanswered_ShouldShowError()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse("Test Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        cut.WaitForState(() => cut.Markup.Contains("Enviar Respostas"));

        // Act - submit without selecting any option
        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        // Assert
        cut.Find(".alert-danger").TextContent.Should().Contain("Por favor, responda a pergunta");
    }

    [Fact]
    public async Task HandleSubmit_WhenSuccessful_ShouldShowSuccessMessage()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse("Test Survey", questionId: questionId, optionId: optionId);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        _surveyServiceMock
            .Setup(x => x.SubmitResponseAsync(It.IsAny<SubmitResponseRequest>()))
            .ReturnsAsync(true);

        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        cut.WaitForState(() => cut.Markup.Contains("radio"));

        // Select an option
        var radioButton = cut.Find($"input[id='{optionId}']");
        radioButton.Change(new ChangeEventArgs { Value = optionId.ToString() });

        // Act
        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Obrigado por participar"));
        cut.Find(".alert-success").TextContent.Should().Contain("Obrigado por participar");
    }

    [Fact]
    public async Task HandleSubmit_WhenFailed_ShouldShowErrorMessage()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse("Test Survey", questionId: questionId, optionId: optionId);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        _surveyServiceMock
            .Setup(x => x.SubmitResponseAsync(It.IsAny<SubmitResponseRequest>()))
            .ReturnsAsync(false);

        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        cut.WaitForState(() => cut.Markup.Contains("radio"));

        // Select an option
        var radioButton = cut.Find($"input[id='{optionId}']");
        radioButton.Change(new ChangeEventArgs { Value = optionId.ToString() });

        // Act
        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Erro ao enviar"));
        cut.Find(".alert-danger").TextContent.Should().Contain("Erro ao enviar resposta");
    }

    [Fact]
    public async Task SuccessMessage_ShouldContainLinksToResultsAndSurveys()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse("Test Survey", surveyId, questionId: questionId, optionId: optionId);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(surveyId))
            .ReturnsAsync(survey);

        _surveyServiceMock
            .Setup(x => x.SubmitResponseAsync(It.IsAny<SubmitResponseRequest>()))
            .ReturnsAsync(true);

        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        cut.WaitForState(() => cut.Markup.Contains("radio"));

        // Select and submit
        var radioButton = cut.Find($"input[id='{optionId}']");
        radioButton.Change(new ChangeEventArgs { Value = optionId.ToString() });
        await cut.InvokeAsync(() => cut.Find("form").Submit());

        cut.WaitForState(() => cut.Markup.Contains("Obrigado"));

        // Assert
        var links = cut.FindAll(".alert-success a");
        links.Should().HaveCountGreaterThanOrEqualTo(2);
        links[0].GetAttribute("href").Should().Contain($"/surveys/{surveyId}/results");
        links[1].GetAttribute("href").Should().Be("/surveys");
    }

    [Fact]
    public async Task SubmitButton_ShouldBeDisabled_WhileSubmitting()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse("Test Survey", questionId: questionId, optionId: optionId);
        var submitTcs = new TaskCompletionSource<bool>();

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        _surveyServiceMock
            .Setup(x => x.SubmitResponseAsync(It.IsAny<SubmitResponseRequest>()))
            .Returns(submitTcs.Task);

        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        cut.WaitForState(() => cut.Markup.Contains("radio"));

        // Select an option
        var radioButton = cut.Find($"input[id='{optionId}']");
        radioButton.Change(new ChangeEventArgs { Value = optionId.ToString() });

        // Act - start submitting
        var submitTask = cut.InvokeAsync(() => cut.Find("form").Submit());

        // Assert - button should show loading state
        cut.WaitForState(() => cut.Markup.Contains("Enviando"));
        cut.Find("button[type='submit']").HasAttribute("disabled").Should().BeTrue();

        // Cleanup
        submitTcs.SetResult(true);
        await submitTask;
    }

    private static SurveyDetailResponse CreateSurveyDetailResponse(
        string title,
        Guid? id = null,
        string? description = null,
        Guid? questionId = null,
        Guid? optionId = null)
    {
        var qId = questionId ?? Guid.NewGuid();
        var oId = optionId ?? Guid.NewGuid();

        return new SurveyDetailResponse(
            Id: id ?? Guid.NewGuid(),
            Title: title,
            Description: description,
            Status: SurveyStatus.Active,
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddDays(7),
            Questions:
            [
                new QuestionResponse(
                    Id: qId,
                    Text: "Question 1?",
                    Order: 1,
                    IsRequired: true,
                    Options:
                    [
                        new OptionResponse(oId, "Option A", 1),
                        new OptionResponse(Guid.NewGuid(), "Option B", 2)
                    ])
            ],
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null
        );
    }
}
