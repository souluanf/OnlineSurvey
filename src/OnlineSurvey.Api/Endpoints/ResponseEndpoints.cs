using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnlineSurvey.Application.DTOs;
using OnlineSurvey.Application.Interfaces;

namespace OnlineSurvey.Api.Endpoints;

public static class ResponseEndpoints
{
    public static void MapResponseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/responses")
            .WithTags("Responses");

        group.MapPost("/", SubmitResponse)
            .WithName("SubmitResponse")
            .WithDescription("Submits a response to a survey")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/surveys/{surveyId:guid}/results", GetSurveyResults)
            .WithName("GetSurveyResults")
            .WithDescription("Gets aggregated results for a survey")
            .Produces<SurveyResultResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/surveys/{surveyId:guid}/count", GetResponseCount)
            .WithName("GetResponseCount")
            .WithDescription("Gets the total response count for a survey")
            .Produces<int>();
    }

    private static async Task<IResult> SubmitResponse(
        [FromBody] SubmitResponseRequest request,
        HttpContext httpContext,
        IResponseService responseService,
        IValidator<SubmitResponseRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var responseId = await responseService.SubmitResponseAsync(request, ipAddress, cancellationToken);

        return Results.Created($"/api/responses/{responseId}", responseId);
    }

    private static async Task<IResult> GetSurveyResults(
        Guid surveyId,
        IResponseService responseService,
        CancellationToken cancellationToken)
    {
        var results = await responseService.GetSurveyResultsAsync(surveyId, cancellationToken);
        return Results.Ok(results);
    }

    private static async Task<IResult> GetResponseCount(
        Guid surveyId,
        IResponseService responseService,
        CancellationToken cancellationToken)
    {
        var count = await responseService.GetResponseCountAsync(surveyId, cancellationToken);
        return Results.Ok(count);
    }
}
