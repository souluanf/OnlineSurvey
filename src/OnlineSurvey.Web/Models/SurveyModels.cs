namespace OnlineSurvey.Web.Models;

public record SurveyResponse(
    Guid Id,
    string Title,
    string? Description,
    SurveyStatus Status,
    DateTime? StartDate,
    DateTime? EndDate,
    int QuestionCount,
    int ResponseCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record SurveyDetailResponse(
    Guid Id,
    string Title,
    string? Description,
    SurveyStatus Status,
    DateTime? StartDate,
    DateTime? EndDate,
    List<QuestionResponse> Questions,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record QuestionResponse(
    Guid Id,
    string Text,
    int Order,
    bool IsRequired,
    List<OptionResponse> Options
);

public record OptionResponse(
    Guid Id,
    string Text,
    int Order
);

public record PaginatedResponse<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
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

public record SubmitResponseRequest(
    Guid SurveyId,
    string? ParticipantId,
    List<AnswerRequest> Answers
);

public record AnswerRequest(
    Guid QuestionId,
    Guid SelectedOptionId
);

public enum SurveyStatus
{
    Draft = 0,
    Active = 1,
    Closed = 2
}

// Request models for creating surveys
public record CreateSurveyRequest(
    string Title,
    string? Description,
    List<CreateQuestionRequest> Questions
);

public record CreateQuestionRequest(
    string Text,
    int Order,
    bool IsRequired,
    List<CreateOptionRequest> Options
);

public record CreateOptionRequest(
    string Text,
    int Order
);

public record ActivateSurveyRequest(
    DateTime? StartDate,
    DateTime? EndDate
);

public record UpdateSurveyRequest(
    string Title,
    string? Description
);
