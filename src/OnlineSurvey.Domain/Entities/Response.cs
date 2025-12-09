using OnlineSurvey.Domain.Exceptions;

namespace OnlineSurvey.Domain.Entities;

public class Response : Entity
{
    public Guid SurveyId { get; private set; }
    public string? ParticipantId { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime SubmittedAt { get; private set; }

    private readonly List<Answer> _answers = [];
    public IReadOnlyCollection<Answer> Answers => _answers.AsReadOnly();

    private Response() { }

    public Response(Guid surveyId, string? participantId = null, string? ipAddress = null)
    {
        SurveyId = surveyId;
        ParticipantId = participantId;
        IpAddress = ipAddress;
        SubmittedAt = DateTime.UtcNow;
    }

    public void AddAnswer(Answer answer)
    {
        if (_answers.Any(a => a.QuestionId == answer.QuestionId))
            throw new DomainException("Answer for this question already exists.");

        _answers.Add(answer);
    }
}

public class Answer : Entity
{
    public Guid ResponseId { get; private set; }
    public Guid QuestionId { get; private set; }
    public Guid SelectedOptionId { get; private set; }

    private Answer() { }

    public Answer(Guid responseId, Guid questionId, Guid selectedOptionId)
    {
        ResponseId = responseId;
        QuestionId = questionId;
        SelectedOptionId = selectedOptionId;
    }
}
