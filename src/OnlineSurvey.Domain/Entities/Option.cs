using OnlineSurvey.Domain.Exceptions;

namespace OnlineSurvey.Domain.Entities;

public class Option : Entity
{
    public Guid QuestionId { get; private set; }
    public string Text { get; private set; } = null!;
    public int Order { get; private set; }

    private Option() { }

    public Option(Guid questionId, string text, int order)
    {
        QuestionId = questionId;
        SetText(text);
        Order = order;
    }

    public void SetText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Option text cannot be empty.");

        if (text.Length > 200)
            throw new DomainException("Option text cannot exceed 200 characters.");

        Text = text;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetOrder(int order)
    {
        if (order < 0)
            throw new DomainException("Option order cannot be negative.");

        Order = order;
        UpdatedAt = DateTime.UtcNow;
    }
}
