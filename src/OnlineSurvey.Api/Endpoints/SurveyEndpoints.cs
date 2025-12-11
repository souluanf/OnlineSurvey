using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnlineSurvey.Application.DTOs;
using OnlineSurvey.Application.Interfaces;
using OnlineSurvey.Domain.Enums;

namespace OnlineSurvey.Api.Endpoints;

public static class SurveyEndpoints
{
    public static void MapSurveyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/surveys")
            .WithTags("Surveys");

        group.MapPost("/", CreateSurvey)
            .WithName("CreateSurvey")
            .WithDescription("Creates a new survey with questions and options")
            .Produces<SurveyDetailResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/", GetSurveys)
            .WithName("GetSurveys")
            .WithDescription("Gets paginated list of surveys")
            .Produces<PaginatedResponse<SurveyResponse>>();

        group.MapGet("/active", GetActiveSurveys)
            .WithName("GetActiveSurveys")
            .WithDescription("Gets all active surveys available for responses")
            .Produces<IEnumerable<SurveyResponse>>();

        group.MapGet("/{id:guid}", GetSurveyById)
            .WithName("GetSurveyById")
            .WithDescription("Gets a survey by ID with all questions and options")
            .Produces<SurveyDetailResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateSurvey)
            .WithName("UpdateSurvey")
            .WithDescription("Updates survey title and description (draft only)")
            .Produces<SurveyDetailResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost("/{id:guid}/activate", ActivateSurvey)
            .WithName("ActivateSurvey")
            .WithDescription("Activates a draft survey to start collecting responses")
            .Produces<SurveyDetailResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost("/{id:guid}/close", CloseSurvey)
            .WithName("CloseSurvey")
            .WithDescription("Closes an active survey")
            .Produces<SurveyDetailResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteSurvey)
            .WithName("DeleteSurvey")
            .WithDescription("Deletes a survey (not active)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateSurvey(
        [FromBody] CreateSurveyRequest request,
        ISurveyService surveyService,
        IValidator<CreateSurveyRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var survey = await surveyService.CreateSurveyAsync(request, cancellationToken);
        return Results.Created($"/api/surveys/{survey.Id}", survey);
    }

    private static async Task<IResult> GetSurveys(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] SurveyStatus? status,
        ISurveyService surveyService,
        CancellationToken cancellationToken)
    {
        var effectivePage = page is null or <= 0 ? 1 : page.Value;
        var effectivePageSize = pageSize is null or <= 0 ? 10 : Math.Min(pageSize.Value, 100);

        var result = await surveyService.GetSurveysAsync(effectivePage, effectivePageSize, status, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetActiveSurveys(
        ISurveyService surveyService,
        CancellationToken cancellationToken)
    {
        var surveys = await surveyService.GetActiveSurveysAsync(cancellationToken);
        return Results.Ok(surveys);
    }

    private static async Task<IResult> GetSurveyById(
        Guid id,
        ISurveyService surveyService,
        CancellationToken cancellationToken)
    {
        var survey = await surveyService.GetSurveyByIdAsync(id, cancellationToken);
        return survey is null ? Results.NotFound() : Results.Ok(survey);
    }

    private static async Task<IResult> UpdateSurvey(
        Guid id,
        [FromBody] UpdateSurveyRequest request,
        ISurveyService surveyService,
        CancellationToken cancellationToken)
    {
        var survey = await surveyService.UpdateSurveyAsync(id, request, cancellationToken);
        return Results.Ok(survey);
    }

    private static async Task<IResult> ActivateSurvey(
        Guid id,
        [FromBody] ActivateSurveyRequest request,
        ISurveyService surveyService,
        CancellationToken cancellationToken)
    {
        var survey = await surveyService.ActivateSurveyAsync(id, request, cancellationToken);
        return Results.Ok(survey);
    }

    private static async Task<IResult> CloseSurvey(
        Guid id,
        ISurveyService surveyService,
        CancellationToken cancellationToken)
    {
        var survey = await surveyService.CloseSurveyAsync(id, cancellationToken);
        return Results.Ok(survey);
    }

    private static async Task<IResult> DeleteSurvey(
        Guid id,
        ISurveyService surveyService,
        CancellationToken cancellationToken)
    {
        await surveyService.DeleteSurveyAsync(id, cancellationToken);
        return Results.NoContent();
    }
}
