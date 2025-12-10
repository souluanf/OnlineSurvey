using OnlineSurvey.Application.DTOs;
using OnlineSurvey.Application.Interfaces;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Exceptions;
using OnlineSurvey.Domain.Repositories;

namespace OnlineSurvey.Application.Services;

public class ResponseService : IResponseService
{
    private readonly IUnitOfWork _unitOfWork;

    public ResponseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> SubmitResponseAsync(SubmitResponseRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var survey = await _unitOfWork.Surveys.GetByIdWithQuestionsAndOptionsAsync(request.SurveyId, cancellationToken)
            ?? throw new DomainException("Survey not found.");

        if (!survey.IsOpen)
            throw new DomainException("Survey is not accepting responses.");

        if (!string.IsNullOrEmpty(request.ParticipantId))
        {
            var hasResponded = await _unitOfWork.Responses.HasRespondedAsync(
                request.SurveyId,
                request.ParticipantId,
                cancellationToken);

            if (hasResponded)
                throw new DomainException("You have already responded to this survey.");
        }

        var answeredQuestionIds = request.Answers.Select(a => a.QuestionId).ToHashSet();
        var unansweredRequired = survey.Questions
            .FirstOrDefault(q => q.IsRequired && !answeredQuestionIds.Contains(q.Id));

        if (unansweredRequired is not null)
            throw new DomainException($"Question '{unansweredRequired.Text}' is required.");

        foreach (var answerDto in request.Answers)
        {
            var question = survey.Questions.FirstOrDefault(q => q.Id == answerDto.QuestionId)
                ?? throw new DomainException($"Question with ID {answerDto.QuestionId} not found in this survey.");

            var optionExists = question.Options.Any(o => o.Id == answerDto.SelectedOptionId);
            if (!optionExists)
                throw new DomainException($"Option with ID {answerDto.SelectedOptionId} is not valid for question '{question.Text}'.");
        }

        var response = new Response(request.SurveyId, request.ParticipantId, ipAddress);

        foreach (var answerDto in request.Answers)
        {
            var answer = new Answer(response.Id, answerDto.QuestionId, answerDto.SelectedOptionId);
            response.AddAnswer(answer);
        }

        await _unitOfWork.Responses.AddAsync(response, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response.Id;
    }

    public async Task<SurveyResultResponse> GetSurveyResultsAsync(Guid surveyId, CancellationToken cancellationToken = default)
    {
        var survey = await _unitOfWork.Surveys.GetByIdWithQuestionsAndOptionsAsync(surveyId, cancellationToken)
            ?? throw new DomainException("Survey not found.");

        var totalResponses = await _unitOfWork.Responses.GetResponseCountBySurveyIdAsync(surveyId, cancellationToken);
        var optionCounts = await _unitOfWork.Responses.GetOptionCountsAsync(surveyId, cancellationToken);

        var questionResults = survey.Questions
            .OrderBy(q => q.Order)
            .Select(q => new QuestionResultResponse(
                q.Id,
                q.Text,
                q.Options
                    .OrderBy(o => o.Order)
                    .Select(o =>
                    {
                        var count = optionCounts.GetValueOrDefault(o.Id, 0);
                        var percentage = totalResponses > 0 ? Math.Round((double)count / totalResponses * 100, 2) : 0;
                        return new OptionResultResponse(o.Id, o.Text, count, percentage);
                    })
                    .ToList()
            ))
            .ToList();

        return new SurveyResultResponse(
            surveyId,
            survey.Title,
            totalResponses,
            questionResults
        );
    }

    public async Task<int> GetResponseCountAsync(Guid surveyId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Responses.GetResponseCountBySurveyIdAsync(surveyId, cancellationToken);
    }
}
