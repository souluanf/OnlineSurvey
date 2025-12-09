namespace OnlineSurvey.Application.DTOs;

public record SubmitResponseRequest(
    Guid SurveyId,
    string? ParticipantId,
    List<AnswerRequest> Answers
);

public record AnswerRequest(
    Guid QuestionId,
    Guid SelectedOptionId
);

public record SurveyResultResponse(
    Guid SurveyId,
    string SurveyTitle,
    int TotalResponses,
    List<QuestionResultResponse> Questions
);

public record QuestionResultResponse(
    Guid QuestionId,
    string QuestionText,
    List<OptionResultResponse> Options
);

public record OptionResultResponse(
    Guid OptionId,
    string OptionText,
    int Count,
    double Percentage
);
