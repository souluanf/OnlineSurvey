using OnlineSurvey.Domain.Exceptions;

namespace OnlineSurvey.Domain.Entities;

public class Question : Entity
{
    public Guid SurveyId { get; private set; }
    public string Text { get; private set; } = null!;
    public int Order { get; private set; }
    public bool IsRequired { get; private set; }

    private readonly List<Option> _options = [];
    public IReadOnlyCollection<Option> Options => _options.AsReadOnly();

    private Question() { }

    public Question(Guid surveyId, string text, int order, bool isRequired = true)
    {
        SurveyId = surveyId;
        SetText(text);
        Order = order;
        IsRequired = isRequired;
    }

    public void SetText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Question text cannot be empty.");

        if (text.Length > 500)
            throw new DomainException("Question text cannot exceed 500 characters.");

        Text = text;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetOrder(int order)
    {
        if (order < 0)
            throw new DomainException("Question order cannot be negative.");

        Order = order;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddOption(Option option)
    {
        if (_options.Count >= 10)
            throw new DomainException("Question cannot have more than 10 options.");

        if (_options.Any(o => o.Text.Equals(option.Text, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException("Duplicate option text is not allowed.");

        _options.Add(option);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveOption(Guid optionId)
    {
        var option = _options.FirstOrDefault(o => o.Id == optionId);
        if (option is null)
            throw new DomainException("Option not found.");

        _options.Remove(option);
        UpdatedAt = DateTime.UtcNow;
    }
}
