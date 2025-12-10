using FluentValidation;
using OnlineSurvey.Application.DTOs;

namespace OnlineSurvey.Application.Validators;

public class SubmitResponseValidator : AbstractValidator<SubmitResponseRequest>
{
    public SubmitResponseValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty().WithMessage("Survey ID is required.");

        RuleFor(x => x.Answers)
            .NotEmpty().WithMessage("At least one answer is required.");

        RuleForEach(x => x.Answers).SetValidator(new AnswerRequestValidator());
    }
}

public class AnswerRequestValidator : AbstractValidator<AnswerRequest>
{
    public AnswerRequestValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Question ID is required.");

        RuleFor(x => x.SelectedOptionId)
            .NotEmpty().WithMessage("Selected option ID is required.");
    }
}
