using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OnlineSurvey.Web.Models;
using OnlineSurvey.Web.Pages.Surveys;
using OnlineSurvey.Web.Services;

namespace OnlineSurvey.Web.Tests.Pages;

public class SurveyListTests : BunitContext
{
    private readonly Mock<ISurveyApiService> _surveyServiceMock;

    public SurveyListTests()
    {
        _surveyServiceMock = new Mock<ISurveyApiService>();
        Services.AddSingleton(_surveyServiceMock.Object);
    }

    [Fact]
    public void ShouldShowLoadingMessage_WhenLoading()
    {
        // Arrange
        _surveyServiceMock
            .Setup(x => x.GetSurveysAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SurveyStatus?>()))
            .Returns(new TaskCompletionSource<PaginatedResponse<SurveyResponse>?>().Task);

        // Act
        var cut = Render<SurveyList>();

        // Assert
        cut.Markup.Should().Contain("Carregando...");
    }

    [Fact]
    public async Task ShouldRenderPageTitle()
    {
        // Arrange
        SetupEmptySurveys();

        // Act
        var cut = Render<SurveyList>();
        cut.WaitForState(() => !cut.Markup.Contains("Carregando..."));

        // Assert
        cut.Find("h1").TextContent.Should().Contain("Pesquisas");
    }

    [Fact]
    public async Task ShouldShowCreateButton()
    {
        // Arrange
        SetupEmptySurveys();

        // Act
        var cut = Render<SurveyList>();
        cut.WaitForState(() => !cut.Markup.Contains("Carregando..."));

        // Assert
        var createLink = cut.Find("a.btn-success");
        createLink.GetAttribute("href").Should().Be("/surveys/create");
    }

    [Fact]
    public async Task ShouldShowTabs()
    {
        // Arrange
        SetupEmptySurveys();

        // Act
        var cut = Render<SurveyList>();
        cut.WaitForState(() => !cut.Markup.Contains("Carregando..."));

        // Assert
        cut.Markup.Should().Contain("Ativas");
        cut.Markup.Should().Contain("Rascunhos");
        cut.Markup.Should().Contain("Encerradas");
    }

    [Fact]
    public async Task ShouldShowEmptyMessage_WhenNoActiveSurveys()
    {
        // Arrange
        SetupEmptySurveys();

        // Act
        var cut = Render<SurveyList>();
        cut.WaitForState(() => !cut.Markup.Contains("Carregando..."));

        // Assert
        cut.Find(".alert-info").TextContent.Should().Contain("Nenhuma pesquisa ativa");
    }

    [Fact]
    public async Task ShouldShowActiveSurveys()
    {
        // Arrange
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Active Survey 1", SurveyStatus.Active),
            CreateSurveyResponse("Active Survey 2", SurveyStatus.Active)
        };
        SetupSurveys(surveys);

        // Act
        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Active Survey 1"));

        // Assert
        cut.Markup.Should().Contain("Active Survey 1");
        cut.Markup.Should().Contain("Active Survey 2");
        cut.FindAll(".card.border-success").Count.Should().Be(2);
    }

    [Fact]
    public async Task ShouldSwitchToDraftTab()
    {
        // Arrange
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Draft Survey", SurveyStatus.Draft)
        };
        SetupSurveys(surveys);

        var cut = Render<SurveyList>();
        cut.WaitForState(() => !cut.Markup.Contains("Carregando..."));

        // Act
        var draftTab = cut.FindAll(".nav-link")[1];
        await cut.InvokeAsync(() => draftTab.Click());

        // Assert
        cut.Markup.Should().Contain("Draft Survey");
        cut.FindAll(".card.border-warning").Count.Should().Be(1);
    }

    [Fact]
    public async Task ShouldSwitchToClosedTab()
    {
        // Arrange
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Closed Survey", SurveyStatus.Closed)
        };
        SetupSurveys(surveys);

        var cut = Render<SurveyList>();
        cut.WaitForState(() => !cut.Markup.Contains("Carregando..."));

        // Act
        var closedTab = cut.FindAll(".nav-link")[2];
        await cut.InvokeAsync(() => closedTab.Click());

        // Assert
        cut.Markup.Should().Contain("Closed Survey");
        cut.FindAll(".card.border-secondary").Count.Should().Be(1);
    }

    [Fact]
    public async Task ActivateSurvey_WhenSuccessful_ShouldShowSuccessMessage()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Draft Survey", SurveyStatus.Draft, surveyId)
        };
        SetupSurveys(surveys);

        _surveyServiceMock
            .Setup(x => x.ActivateSurveyAsync(surveyId, It.IsAny<ActivateSurveyRequest>()))
            .ReturnsAsync(new SurveyDetailResponse(surveyId, "Draft Survey", null, SurveyStatus.Active, null, null, [], DateTime.UtcNow, null));

        var cut = Render<SurveyList>();
        cut.WaitForState(() => !cut.Markup.Contains("Carregando..."));

        // Switch to draft tab
        await cut.InvokeAsync(() => cut.FindAll(".nav-link")[1].Click());

        // Act
        var activateButton = cut.Find("button.btn-success");
        await cut.InvokeAsync(() => activateButton.Click());

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("ativada com sucesso"));
        cut.Find(".alert-success").TextContent.Should().Contain("ativada com sucesso");
    }

    [Fact]
    public async Task ActivateSurvey_WhenFailed_ShouldShowErrorMessage()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Draft Survey", SurveyStatus.Draft, surveyId)
        };
        SetupSurveys(surveys);

        _surveyServiceMock
            .Setup(x => x.ActivateSurveyAsync(surveyId, It.IsAny<ActivateSurveyRequest>()))
            .ReturnsAsync((SurveyDetailResponse?)null);

        var cut = Render<SurveyList>();
        cut.WaitForState(() => !cut.Markup.Contains("Carregando..."));

        // Switch to draft tab
        await cut.InvokeAsync(() => cut.FindAll(".nav-link")[1].Click());

        // Act
        var activateButton = cut.Find("button.btn-success");
        await cut.InvokeAsync(() => activateButton.Click());

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Erro ao ativar"));
        cut.Find(".alert-danger").TextContent.Should().Contain("Erro ao ativar");
    }

    [Fact]
    public async Task DeleteSurvey_WhenSuccessful_ShouldShowSuccessMessage()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Draft Survey", SurveyStatus.Draft, surveyId)
        };
        SetupSurveys(surveys);

        _surveyServiceMock
            .Setup(x => x.DeleteSurveyAsync(surveyId))
            .ReturnsAsync(true);

        var cut = Render<SurveyList>();
        cut.WaitForState(() => !cut.Markup.Contains("Carregando..."));

        // Switch to draft tab
        await cut.InvokeAsync(() => cut.FindAll(".nav-link")[1].Click());

        // Act
        var deleteButton = cut.Find("button.btn-outline-danger");
        await cut.InvokeAsync(() => deleteButton.Click());

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("excluída com sucesso"));
        cut.Find(".alert-success").TextContent.Should().Contain("excluída com sucesso");
    }

    [Fact]
    public async Task DeleteSurvey_WhenFailed_ShouldShowErrorMessage()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Draft Survey", SurveyStatus.Draft, surveyId)
        };
        SetupSurveys(surveys);

        _surveyServiceMock
            .Setup(x => x.DeleteSurveyAsync(surveyId))
            .ReturnsAsync(false);

        var cut = Render<SurveyList>();
        cut.WaitForState(() => !cut.Markup.Contains("Carregando..."));

        // Switch to draft tab
        await cut.InvokeAsync(() => cut.FindAll(".nav-link")[1].Click());

        // Act
        var deleteButton = cut.Find("button.btn-outline-danger");
        await cut.InvokeAsync(() => deleteButton.Click());

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Erro ao excluir"));
        cut.Find(".alert-danger").TextContent.Should().Contain("Erro ao excluir");
    }

    [Fact]
    public async Task CloseSurvey_WhenSuccessful_ShouldShowSuccessMessage()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Active Survey", SurveyStatus.Active, surveyId)
        };
        SetupSurveys(surveys);

        _surveyServiceMock
            .Setup(x => x.CloseSurveyAsync(surveyId))
            .ReturnsAsync(new SurveyDetailResponse(surveyId, "Active Survey", null, SurveyStatus.Closed, null, null, [], DateTime.UtcNow, null));

        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Active Survey"));

        // Act
        var closeButton = cut.Find("button.btn-outline-warning");
        await cut.InvokeAsync(() => closeButton.Click());

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("encerrada com sucesso"));
        cut.Find(".alert-success").TextContent.Should().Contain("encerrada com sucesso");
    }

    [Fact]
    public async Task CloseSurvey_WhenFailed_ShouldShowErrorMessage()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Active Survey", SurveyStatus.Active, surveyId)
        };
        SetupSurveys(surveys);

        _surveyServiceMock
            .Setup(x => x.CloseSurveyAsync(surveyId))
            .ReturnsAsync((SurveyDetailResponse?)null);

        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Active Survey"));

        // Act
        var closeButton = cut.Find("button.btn-outline-warning");
        await cut.InvokeAsync(() => closeButton.Click());

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Erro ao encerrar"));
        cut.Find(".alert-danger").TextContent.Should().Contain("Erro ao encerrar");
    }

    [Fact]
    public async Task ShouldShowSurveyDetails()
    {
        // Arrange
        var surveys = new List<SurveyResponse>
        {
            new(Guid.NewGuid(), "Test Survey", "Test Description", SurveyStatus.Active,
                DateTime.UtcNow, DateTime.UtcNow.AddDays(7), 5, 10, DateTime.UtcNow, null)
        };
        SetupSurveys(surveys);

        // Act
        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Test Survey"));

        // Assert
        cut.Markup.Should().Contain("Test Survey");
        cut.Markup.Should().Contain("Test Description");
        cut.Markup.Should().Contain("5 perguntas");
        cut.Markup.Should().Contain("10 respostas");
    }

    [Fact]
    public async Task ShouldDismissMessage_WhenCloseButtonClicked()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Draft Survey", SurveyStatus.Draft, surveyId)
        };
        SetupSurveys(surveys);

        _surveyServiceMock
            .Setup(x => x.DeleteSurveyAsync(surveyId))
            .ReturnsAsync(true);

        var cut = Render<SurveyList>();
        cut.WaitForState(() => !cut.Markup.Contains("Carregando..."));

        await cut.InvokeAsync(() => cut.FindAll(".nav-link")[1].Click());
        await cut.InvokeAsync(() => cut.Find("button.btn-outline-danger").Click());

        cut.WaitForState(() => cut.Markup.Contains("excluída com sucesso"));

        // Act
        var closeButton = cut.Find(".btn-close");
        await cut.InvokeAsync(() => closeButton.Click());

        // Assert
        cut.FindAll(".alert-success").Count.Should().Be(0);
    }

    private void SetupEmptySurveys()
    {
        _surveyServiceMock
            .Setup(x => x.GetSurveysAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SurveyStatus?>()))
            .ReturnsAsync(new PaginatedResponse<SurveyResponse>([], 1, 100, 0, 0));
    }

    private void SetupSurveys(List<SurveyResponse> surveys)
    {
        _surveyServiceMock
            .Setup(x => x.GetSurveysAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SurveyStatus?>()))
            .ReturnsAsync(new PaginatedResponse<SurveyResponse>(surveys, 1, 100, surveys.Count, 1));
    }

    private static SurveyResponse CreateSurveyResponse(string title, SurveyStatus status, Guid? id = null)
    {
        return new SurveyResponse(
            Id: id ?? Guid.NewGuid(),
            Title: title,
            Description: null,
            Status: status,
            StartDate: status == SurveyStatus.Active ? DateTime.UtcNow : null,
            EndDate: status == SurveyStatus.Active ? DateTime.UtcNow.AddDays(7) : null,
            QuestionCount: 1,
            ResponseCount: 0,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null
        );
    }
}
