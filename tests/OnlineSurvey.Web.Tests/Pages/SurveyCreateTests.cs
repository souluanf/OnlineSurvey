using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OnlineSurvey.Web.Models;
using OnlineSurvey.Web.Pages.Surveys;
using OnlineSurvey.Web.Services;

namespace OnlineSurvey.Web.Tests.Pages;

public class SurveyCreateTests : BunitContext
{
    private readonly Mock<ISurveyApiService> _surveyServiceMock;

    public SurveyCreateTests()
    {
        _surveyServiceMock = new Mock<ISurveyApiService>();
        Services.AddSingleton(_surveyServiceMock.Object);
    }

    [Fact]
    public void ShouldRenderInitialFormWithOneQuestion()
    {
        // Act
        var cut = Render<SurveyCreate>();

        // Assert
        cut.Find("h1").TextContent.Should().Contain("Criar Nova Pesquisa");
        cut.FindAll(".card.border-primary").Count.Should().Be(1); // One question card
        cut.FindAll(".input-group").Count.Should().Be(2); // Two option inputs
    }

    [Fact]
    public void ShouldShowTitleInput()
    {
        // Act
        var cut = Render<SurveyCreate>();

        // Assert
        var titleInput = cut.Find("input[type='text']");
        titleInput.Should().NotBeNull();
    }

    [Fact]
    public void ShouldShowDescriptionTextarea()
    {
        // Act
        var cut = Render<SurveyCreate>();

        // Assert
        var textarea = cut.Find("textarea");
        textarea.Should().NotBeNull();
    }

    [Fact]
    public void AddQuestion_ShouldAddNewQuestionCard()
    {
        // Arrange
        var cut = Render<SurveyCreate>();
        var initialQuestionCount = cut.FindAll(".card.border-primary").Count;

        // Act
        var addButton = cut.Find("button.btn-success");
        addButton.Click();

        // Assert
        cut.FindAll(".card.border-primary").Count.Should().Be(initialQuestionCount + 1);
    }

    [Fact]
    public void RemoveQuestion_ShouldRemoveQuestionCard()
    {
        // Arrange
        var cut = Render<SurveyCreate>();
        // Add an extra question first
        cut.Find("button.btn-success").Click();
        var questionCountAfterAdd = cut.FindAll(".card.border-primary").Count;

        // Act
        var removeButton = cut.Find("button.btn-outline-danger");
        removeButton.Click();

        // Assert
        cut.FindAll(".card.border-primary").Count.Should().Be(questionCountAfterAdd - 1);
    }

    [Fact]
    public void AddOption_ShouldAddNewOptionInput()
    {
        // Arrange
        var cut = Render<SurveyCreate>();
        var initialOptionCount = cut.FindAll(".input-group").Count;

        // Act
        var addOptionButton = cut.Find("button.btn-outline-success");
        addOptionButton.Click();

        // Assert
        cut.FindAll(".input-group").Count.Should().Be(initialOptionCount + 1);
    }

    [Fact]
    public void SubmitButton_ShouldBeDisabledWhenFormIsInvalid()
    {
        // Arrange
        var cut = Render<SurveyCreate>();

        // Assert - button should be disabled because title is empty
        var submitButton = cut.Find("button[type='submit']");
        submitButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public async Task HandleSubmit_WhenSuccessful_ShouldShowSuccessMessage()
    {
        // Arrange
        var createdSurvey = CreateSurveyDetailResponse();

        _surveyServiceMock
            .Setup(x => x.CreateSurveyAsync(It.IsAny<CreateSurveyRequest>()))
            .ReturnsAsync(createdSurvey);

        var cut = Render<SurveyCreate>();

        // Fill in the form using InvokeAsync to prevent race conditions
        await cut.InvokeAsync(() =>
        {
            cut.Find("input[type='text']").Change("Test Survey");
        });
        await cut.InvokeAsync(() =>
        {
            cut.FindAll("input[type='text']")[1].Change("Test Question?");
        });
        await cut.InvokeAsync(() =>
        {
            var optionInputs = cut.FindAll(".input-group input");
            optionInputs[0].Change("Option A");
        });
        await cut.InvokeAsync(() =>
        {
            var optionInputs = cut.FindAll(".input-group input");
            optionInputs[1].Change("Option B");
        });

        // Act
        await cut.InvokeAsync(() => cut.Find("form").Submit());

        // Assert
        cut.Find(".alert-success").TextContent.Should().Contain("Pesquisa criada com sucesso");
    }

    [Fact]
    public async Task HandleSubmit_WhenFailed_ShouldShowErrorMessage()
    {
        // Arrange
        _surveyServiceMock
            .Setup(x => x.CreateSurveyAsync(It.IsAny<CreateSurveyRequest>()))
            .ReturnsAsync((SurveyDetailResponse?)null);

        var cut = Render<SurveyCreate>();

        // Fill in the form using InvokeAsync to prevent race conditions
        await cut.InvokeAsync(() =>
        {
            cut.Find("input[type='text']").Change("Test Survey");
        });
        await cut.InvokeAsync(() =>
        {
            cut.FindAll("input[type='text']")[1].Change("Test Question?");
        });
        await cut.InvokeAsync(() =>
        {
            var optionInputs = cut.FindAll(".input-group input");
            optionInputs[0].Change("Option A");
        });
        await cut.InvokeAsync(() =>
        {
            var optionInputs = cut.FindAll(".input-group input");
            optionInputs[1].Change("Option B");
        });

        // Act
        await cut.InvokeAsync(() => cut.Find("form").Submit());

        // Assert
        cut.Find(".alert-danger").TextContent.Should().Contain("Erro ao criar pesquisa");
    }

    [Fact]
    public async Task ActivateAndGo_WhenSuccessful_ShouldNavigate()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var createdSurvey = CreateSurveyDetailResponse(surveyId);
        var activatedSurvey = CreateSurveyDetailResponse(surveyId);

        _surveyServiceMock
            .Setup(x => x.CreateSurveyAsync(It.IsAny<CreateSurveyRequest>()))
            .ReturnsAsync(createdSurvey);

        _surveyServiceMock
            .Setup(x => x.ActivateSurveyAsync(surveyId, It.IsAny<ActivateSurveyRequest>()))
            .ReturnsAsync(activatedSurvey);

        var navManager = Services.GetRequiredService<NavigationManager>();

        var cut = Render<SurveyCreate>();

        // Fill and submit the form using InvokeAsync
        await cut.InvokeAsync(() => cut.Find("input[type='text']").Change("Test Survey"));
        await cut.InvokeAsync(() => cut.FindAll("input[type='text']")[1].Change("Test Question?"));
        await cut.InvokeAsync(() => cut.FindAll(".input-group input")[0].Change("Option A"));
        await cut.InvokeAsync(() => cut.FindAll(".input-group input")[1].Change("Option B"));

        await cut.InvokeAsync(() => cut.Find("form").Submit());

        // Act
        await cut.InvokeAsync(() => cut.Find(".alert-success button.btn-primary").Click());

        // Assert
        navManager.Uri.Should().Contain($"/surveys/{surveyId}");
    }

    [Fact]
    public void IsRequiredCheckbox_ShouldBeCheckedByDefault()
    {
        // Arrange
        var cut = Render<SurveyCreate>();

        // Assert
        var checkbox = cut.Find("input[type='checkbox']");
        checkbox.HasAttribute("checked").Should().BeTrue();
    }

    [Fact]
    public void CancelButton_ShouldLinkToSurveysList()
    {
        // Arrange
        var cut = Render<SurveyCreate>();

        // Assert
        var cancelLink = cut.Find("a.btn-outline-secondary");
        cancelLink.GetAttribute("href").Should().Be("/surveys");
    }

    private static SurveyDetailResponse CreateSurveyDetailResponse(Guid? id = null)
    {
        return new SurveyDetailResponse(
            Id: id ?? Guid.NewGuid(),
            Title: "Test Survey",
            Description: "Description",
            Status: SurveyStatus.Draft,
            StartDate: null,
            EndDate: null,
            Questions:
            [
                new QuestionResponse(
                    Id: Guid.NewGuid(),
                    Text: "Test Question?",
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
