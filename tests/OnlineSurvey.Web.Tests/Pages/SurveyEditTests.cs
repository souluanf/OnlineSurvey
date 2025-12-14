using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OnlineSurvey.Web.Models;
using OnlineSurvey.Web.Pages.Surveys;
using OnlineSurvey.Web.Services;

namespace OnlineSurvey.Web.Tests.Pages;

public class SurveyEditTests : BunitContext
{
    private readonly Mock<ISurveyApiService> _surveyServiceMock;

    public SurveyEditTests()
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
            .Returns(new TaskCompletionSource<SurveyDetailResponse?>().Task);

        // Act
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, Guid.NewGuid()));

        // Assert
        cut.Markup.Should().Contain("Carregando...");
    }

    [Fact]
    public async Task ShouldShowSurveyNotFound_WhenSurveyIsNull()
    {
        // Arrange
        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((SurveyDetailResponse?)null);

        // Act
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, Guid.NewGuid()));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("não encontrada"));
        cut.Find(".alert-danger").TextContent.Should().Contain("Pesquisa não encontrada");
    }

    [Fact]
    public async Task ShouldShowCannotEditMessage_WhenSurveyIsActive()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse(status: SurveyStatus.Active);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("não pode ser editada"));
        cut.Find(".alert-warning h4").TextContent.Should().Contain("não pode ser editada");
    }

    [Fact]
    public async Task ShouldShowCannotEditMessage_WhenSurveyIsClosed()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse(status: SurveyStatus.Closed);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("não pode ser editada"));
        cut.Find(".alert-warning").TextContent.Should().Contain("Encerrada");
    }

    [Fact]
    public async Task ShouldRenderEditForm_WhenSurveyIsDraft()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse(status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Editar Pesquisa"));
        cut.Find("h1").TextContent.Should().Contain("Editar Pesquisa");
    }

    [Fact]
    public async Task ShouldPopulateTitleInput_WithExistingValue()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse("Existing Title", status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Editar Pesquisa"));
        var titleInput = cut.Find("input[type='text']");
        titleInput.GetAttribute("value").Should().Be("Existing Title");
    }

    [Fact]
    public async Task ShouldPopulateDescriptionTextarea_WithExistingValue()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse("Title", "Existing Description", SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Editar Pesquisa"));
        // Textarea value in Blazor is bound, check the markup contains the description
        cut.Markup.Should().Contain("Existing Description");
    }

    [Fact]
    public async Task ShouldDisplayQuestions_ReadOnly()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse(status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Pergunta 1"));
        cut.Markup.Should().Contain("Question 1?");
        cut.Markup.Should().Contain("Option A");
        cut.Markup.Should().Contain("Option B");
    }

    [Fact]
    public async Task ShouldShowRequiredBadge_ForRequiredQuestions()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse(status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Obrigatória"));
        cut.Find(".badge.bg-danger").TextContent.Should().Contain("Obrigatória");
    }

    [Fact]
    public async Task SaveChanges_WhenSuccessful_ShouldShowSuccessMessage()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse("Test Survey", id: surveyId, status: SurveyStatus.Draft);
        var updatedSurvey = CreateSurveyDetailResponse("Updated Title", id: surveyId, status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(surveyId))
            .ReturnsAsync(survey);

        _surveyServiceMock
            .Setup(x => x.UpdateSurveyAsync(surveyId, It.IsAny<UpdateSurveyRequest>()))
            .ReturnsAsync(updatedSurvey);

        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, surveyId));

        cut.WaitForState(() => cut.Markup.Contains("Editar Pesquisa"));

        // Change the title
        var titleInput = cut.Find("input[type='text']");
        titleInput.Change("Updated Title");

        // Act
        var saveButton = cut.Find("button.btn-primary");
        await cut.InvokeAsync(() => saveButton.Click());

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("atualizada com sucesso"));
        cut.Find(".alert-success").TextContent.Should().Contain("atualizada com sucesso");
    }

    [Fact]
    public async Task SaveChanges_WhenFailed_ShouldShowErrorMessage()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse("Test Survey", id: surveyId, status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(surveyId))
            .ReturnsAsync(survey);

        _surveyServiceMock
            .Setup(x => x.UpdateSurveyAsync(surveyId, It.IsAny<UpdateSurveyRequest>()))
            .ReturnsAsync((SurveyDetailResponse?)null);

        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, surveyId));

        cut.WaitForState(() => cut.Markup.Contains("Editar Pesquisa"));

        // Act
        var saveButton = cut.Find("button.btn-primary");
        await cut.InvokeAsync(() => saveButton.Click());

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Erro ao salvar"));
        cut.Find(".alert-danger").TextContent.Should().Contain("Erro ao salvar alterações");
    }

    [Fact]
    public async Task ActivateSurvey_WhenSuccessful_ShouldNavigateToSurveysList()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse("Test Survey", id: surveyId, status: SurveyStatus.Draft);
        var activatedSurvey = CreateSurveyDetailResponse("Test Survey", id: surveyId, status: SurveyStatus.Active);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(surveyId))
            .ReturnsAsync(survey);

        _surveyServiceMock
            .Setup(x => x.ActivateSurveyAsync(surveyId, It.IsAny<ActivateSurveyRequest>()))
            .ReturnsAsync(activatedSurvey);

        var navManager = Services.GetRequiredService<NavigationManager>();

        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, surveyId));

        cut.WaitForState(() => cut.Markup.Contains("Ativar Pesquisa"));

        // Act
        var activateButton = cut.Find("button.btn-success");
        await cut.InvokeAsync(() => activateButton.Click());

        // Assert
        navManager.Uri.Should().EndWith("/surveys");
    }

    [Fact]
    public async Task ActivateSurvey_WhenFailed_ShouldShowErrorMessage()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse("Test Survey", id: surveyId, status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(surveyId))
            .ReturnsAsync(survey);

        _surveyServiceMock
            .Setup(x => x.ActivateSurveyAsync(surveyId, It.IsAny<ActivateSurveyRequest>()))
            .ReturnsAsync((SurveyDetailResponse?)null);

        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, surveyId));

        cut.WaitForState(() => cut.Markup.Contains("Ativar Pesquisa"));

        // Act
        var activateButton = cut.Find("button.btn-success");
        await cut.InvokeAsync(() => activateButton.Click());

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Erro ao ativar"));
        cut.Find(".alert-danger").TextContent.Should().Contain("Erro ao ativar pesquisa");
    }

    [Fact]
    public async Task SaveButton_ShouldBeDisabled_WhenTitleIsEmpty()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse("Test Survey", status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        cut.WaitForState(() => cut.Markup.Contains("Editar Pesquisa"));

        // Clear the title
        var titleInput = cut.Find("input[type='text']");
        titleInput.Change("");

        // Assert
        var saveButton = cut.Find("button.btn-primary");
        saveButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public async Task BackButton_ShouldLinkToSurveysList()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse(status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        cut.WaitForState(() => cut.Markup.Contains("Voltar"));

        // Assert
        var backLink = cut.Find("a.btn-outline-secondary");
        backLink.GetAttribute("href").Should().Be("/surveys");
    }

    [Fact]
    public async Task ShouldShowInfoMessage_AboutEditingQuestionsLimitation()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse(status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Para editar perguntas"));
        cut.Find(".alert-info").TextContent.Should().Contain("exclua a pesquisa e crie uma nova");
    }

    private static SurveyDetailResponse CreateSurveyDetailResponse(
        string title = "Test Survey",
        string? description = null,
        SurveyStatus status = SurveyStatus.Draft,
        Guid? id = null)
    {
        return new SurveyDetailResponse(
            Id: id ?? Guid.NewGuid(),
            Title: title,
            Description: description,
            Status: status,
            StartDate: status == SurveyStatus.Active ? DateTime.UtcNow : null,
            EndDate: status == SurveyStatus.Active ? DateTime.UtcNow.AddDays(7) : null,
            Questions:
            [
                new QuestionResponse(
                    Id: Guid.NewGuid(),
                    Text: "Question 1?",
                    Order: 1,
                    IsRequired: true,
                    Options:
                    [
                        new OptionResponse(Guid.NewGuid(), "Option A", 1),
                        new OptionResponse(Guid.NewGuid(), "Option B", 2)
                    ])
            ],
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null
        );
    }
}
